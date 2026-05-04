import { CommonModule, CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal, OnDestroy } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { BrowserMultiFormatReader, IScannerControls } from '@zxing/browser';
import { BillingApiService } from '../../core/services/billing-api.service';
import { CartApiService } from '../../core/services/cart-api.service';
import { ProductsApiService } from '../../core/services/products-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { Cart, CartItem, Invoice, PaymentMethod, Product } from '../../core/types/api.models';
import { InvoiceViewComponent } from '../../shared/components/invoice-view/invoice-view.component';
import { ToastService } from '../../shared/services/toast.service';
import { ShiftApiService } from '../../core/services/shift-api.service';

export type CartStep = 1 | 2 | 3 | 4;

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, InvoiceViewComponent],
  templateUrl: './cart-page.component.html',
  styleUrl: './cart-page.component.scss'
})
export class CartPageComponent implements OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly cartApi = inject(CartApiService);
  private readonly productsApi = inject(ProductsApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly shiftApi = inject(ShiftApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly authStore = inject(AuthStore);
  readonly cart = signal<Cart | null>(null);
  readonly products = signal<Product[]>([]);
  readonly heldCartId = signal<number | null>(null);
  readonly busy = signal(false);
  readonly currentStep = signal<CartStep>(1);
  readonly paymentProcessing = signal(false);
  readonly paymentDone = signal(false);
  readonly invoice = signal<Invoice | null>(null);
  readonly selectedPaymentMethod = signal<PaymentMethod>('Cash');
  readonly upiId = signal('');
  readonly cardNumber = signal('');
  readonly couponCode = signal('');
  readonly couponApplied = signal(false);
  readonly discountPercentage = signal(0);
  readonly barcodeSearch = signal('');
  readonly amountReceived = signal<number | null>(null);

  readonly isScannerActive = signal(false);
  readonly availableCameras = signal<MediaDeviceInfo[]>([]);
  readonly selectedCameraId = signal<string>('');
  private codeReader = new BrowserMultiFormatReader();
  private scannerControls: IScannerControls | null = null;
  private lastScannedCode = '';
  private lastScanTime = 0;

  readonly addItemForm = this.fb.nonNullable.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  readonly steps = [
    { num: 1, label: 'Cart' },
    { num: 2, label: 'Checkout' },
    { num: 3, label: 'Payment' },
    { num: 4, label: 'Invoice' }
  ];

  readonly paymentMethods: { method: PaymentMethod; icon: string; label: string }[] = [
    { method: 'Cash', icon: '💵', label: 'Cash' },
    { method: 'UPI', icon: '📱', label: 'UPI' },
    { method: 'Card', icon: '💳', label: 'Card' }
  ];

  readonly enrichedCartItems = computed<(CartItem & { productName: string; categoryName: string })[]>(() => {
    const cart = this.cart();
    const prods = this.products();
    if (!cart) return [];
    return cart.items.map((item) => {
      const product = prods.find((p) => p.id === item.productId);
      return {
        ...item,
        productName: product?.name ?? `Product #${item.productId}`,
        categoryName: product?.categoryName ?? ''
      };
    });
  });

  readonly cartTotal = computed(() => this.cart()?.totalAmount ?? 0);
  readonly cartSubtotal = computed(() => this.cart()?.subtotalAmount ?? 0);
  readonly cartTax = computed(() => this.cart()?.taxAmount ?? 0);
  readonly cartCount = computed(() => this.cart()?.items?.reduce((sum, i) => sum + i.quantity, 0) ?? 0);

  readonly quickProducts = signal<Product[]>([]);

  constructor() {
    this.loadData();
  }

  getCategoryIcon(categoryName: string): string {
    const cat = (categoryName || '').toLowerCase();
    if (cat.includes('food') || cat.includes('snack') || cat.includes('beverage')) return '🍱';
    if (cat.includes('electronic') || cat.includes('tech')) return '💻';
    if (cat.includes('cloth') || cat.includes('wear') || cat.includes('apparel')) return '👕';
    if (cat.includes('health') || cat.includes('pharma') || cat.includes('medic')) return '💊';
    if (cat.includes('beauty') || cat.includes('cosmetic')) return '💄';
    if (cat.includes('book') || cat.includes('stationery')) return '📚';
    if (cat.includes('grocery')) return '🛒';
    return '📦';
  }

  loadData(): void {
    this.busy.set(true);
    forkJoin({ products: this.productsApi.getProducts() }).subscribe({
      next: ({ products }) => {
        this.products.set(products);
        this.quickProducts.set(products.slice(0, 9));
        this.loadCart();
      },
      error: (error) => {
        this.toast.error('Unable to load products.');
        this.busy.set(false);
      }
    });
  }

  private loadCart(): void {
    this.cartApi.getCart(this.authStore.userId()).subscribe({
      next: (cart) => {
        this.cart.set(cart);
        this.busy.set(false);
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.cart.set(null);
          this.busy.set(false);
          return;
        }
        this.toast.error('Unable to load your cart.');
        this.busy.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.stopScanner();
  }

  async toggleScanner(): Promise<void> {
    if (this.isScannerActive()) {
      this.stopScanner();
    } else {
      await this.startScanner();
    }
  }

  async startScanner(): Promise<void> {
    this.isScannerActive.set(true);
    try {
      const videoInputDevices = await BrowserMultiFormatReader.listVideoInputDevices();
      this.availableCameras.set(videoInputDevices);
      
      if (videoInputDevices.length > 0) {
        const defaultCam = videoInputDevices[videoInputDevices.length - 1].deviceId;
        this.selectedCameraId.set(defaultCam);
        this.startDecodingFromDevice(defaultCam);
      } else {
        this.toast.error('No cameras found on this device.');
        this.isScannerActive.set(false);
      }
    } catch (err) {
      this.toast.error('Camera permission denied or not available.');
      this.isScannerActive.set(false);
    }
  }

  private startDecodingFromDevice(deviceId: string): void {
    this.stopScannerControls();
    this.codeReader.decodeFromVideoDevice(deviceId, 'barcode-video', (result, err, controls) => {
      this.scannerControls = controls;
      if (result) {
        const barcode = result.getText();
        const now = Date.now();
        if (this.lastScannedCode !== barcode || (now - this.lastScanTime > 2000)) {
          this.lastScannedCode = barcode;
          this.lastScanTime = now;
          this.barcodeSearch.set(barcode);
          this.addByBarcode();
        }
      }
    }).catch(e => console.error(e));
  }

  switchCamera(deviceId: string): void {
    this.selectedCameraId.set(deviceId);
    this.startDecodingFromDevice(deviceId);
  }

  private stopScannerControls(): void {
    if (this.scannerControls) {
      this.scannerControls.stop();
      this.scannerControls = null;
    }
  }

  stopScanner(): void {
    this.stopScannerControls();
    this.isScannerActive.set(false);
  }

  addItem(): void {
    if (this.addItemForm.invalid) {
      this.addItemForm.markAllAsTouched();
      return;
    }
    this.cartApi
      .addItem({ userId: this.authStore.userId(), ...this.addItemForm.getRawValue() })
      .subscribe({
        next: (response) => {
          this.toast.success(response.message);
          this.loadData();
          this.addItemForm.patchValue({ productId: 0, quantity: 1 });
        },
        error: () => this.toast.error('Unable to add item.')
      });
  }

  addByBarcode(): void {
    const barcode = this.barcodeSearch().trim();

    if (!barcode) {
      this.toast.warning('Scan or enter a barcode first.');
      return;
    }

    this.productsApi.getProductByBarcode(barcode).subscribe({
      next: (product) => {
        this.cartApi.addItem({ userId: this.authStore.userId(), productId: product.id, quantity: 1 }).subscribe({
          next: (response) => {
            this.toast.success(response.message);
            this.barcodeSearch.set('');
            this.loadData();
          },
          error: () => this.toast.error('Unable to add scanned item.')
        });
      },
      error: () => {
        this.toast.error(`Product not found for barcode: ${barcode}`);
        this.barcodeSearch.set('');
      }
    });
  }

  quickAdd(productId: number): void {
    this.cartApi.addItem({ userId: this.authStore.userId(), productId, quantity: 1 }).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.loadData();
      },
      error: () => this.toast.error('Unable to add quick item.')
    });
  }

  updateItem(productId: number, quantityValue: string): void {
    const quantity = Number(quantityValue);
    if (!quantity || quantity < 1) return;
    this.cartApi.updateItem({ userId: this.authStore.userId(), productId, quantity }).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.loadData();
      },
      error: () => this.toast.error('Unable to update item.')
    });
  }

  removeItem(productId: number): void {
    this.cartApi.removeItem(this.authStore.userId(), productId).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.loadData();
      },
      error: () => this.toast.error('Unable to remove item.')
    });
  }

  holdCart(): void {
    const cartId = this.cart()?.cartId;
    if (!cartId) return;
    this.cartApi.holdCart(cartId).subscribe({
      next: (response) => {
        this.heldCartId.set(cartId);
        this.toast.info(response.message);
        this.loadData();
      },
      error: () => this.toast.error('Unable to hold cart.')
    });
  }

  resumeCart(): void {
    const cartId = this.heldCartId();
    if (!cartId) return;
    this.cartApi.resumeCart(cartId).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.heldCartId.set(null);
        this.loadData();
      },
      error: () => this.toast.error('Unable to resume cart.')
    });
  }

  clearCart(): void {
    const cartId = this.cart()?.cartId;
    if (!cartId) return;
    this.cartApi.clearCart(cartId).subscribe({
      next: (response) => {
        this.toast.info(response.message);
        this.loadData();
      },
      error: () => this.toast.error('Unable to clear cart.')
    });
  }

  selectPaymentMethod(method: PaymentMethod): void {
    this.selectedPaymentMethod.set(method);
    if (method !== 'Cash') {
      this.amountReceived.set(null);
    }
  }

  proceedToCheckout(): void {
    if (!this.cart()?.items?.length) {
      this.toast.warning('Add items to your cart first.');
      return;
    }
    this.currentStep.set(2);
  }

  readonly discountedTotal = computed(() => {
    const total = this.cartTotal();
    const discount = this.discountPercentage();
    return total - (total * (discount / 100));
  });

  readonly changeToReturn = computed(() => {
    const received = this.amountReceived();
    const total = this.discountedTotal();
    if (received === null || received < total) return 0;
    return received - total;
  });

  proceedToPayment(): void {
    if (this.selectedPaymentMethod() === 'UPI' && !this.upiId().trim()) {
      this.toast.warning('Please enter a valid UPI ID.');
      return;
    }
    if (this.selectedPaymentMethod() === 'Card' && !this.cardNumber().trim()) {
      this.toast.warning('Please enter a valid Card Number.');
      return;
    }
    if (this.selectedPaymentMethod() === 'Cash') {
      const received = this.amountReceived() ?? 0;
      if (received < this.discountedTotal()) {
        this.toast.warning(`Amount received (₹${received}) is less than total due (₹${this.discountedTotal()}).`);
        return;
      }
    }

    this.currentStep.set(3);
    this.paymentProcessing.set(true);
    this.paymentDone.set(false);

    // Simulate brief processing animation then call the real checkout
    setTimeout(() => {
      const cartId = this.cart()?.cartId;
      if (!cartId) return;

      this.cartApi
        .checkoutCart(cartId, {
          paymentMethod: this.selectedPaymentMethod(),
          couponCode: this.couponCode().trim() || undefined,
          upiId: this.upiId().trim() || undefined,
          cardNumber: this.cardNumber().trim() || undefined
        })
        .subscribe({
          next: (summary) => {
            this.paymentProcessing.set(false);
            this.paymentDone.set(true);
            this.toast.success(`Payment successful! Bill #${summary.billId} created.`);
            // Fetch invoice after short delay for the animation
            setTimeout(() => {
              this.billingApi.getInvoice(summary.billId).subscribe({
                next: (inv) => {
                  this.invoice.set(inv);
                  this.cart.set(null);
                  this.currentStep.set(4);
                  setTimeout(() => window.print(), 500);
                },
                error: () => {
                  // Still navigate to step 4 even if invoice detail fails
                  this.invoice.set({
                    billId: summary.billId,
                    userId: this.authStore.userId(),
                    totalAmount: summary.totalAmount,
                    createdAt: new Date().toISOString(),
                    paymentMethod: this.selectedPaymentMethod(),
                    paymentStatus: 'Paid',
                    items: null
                  });
                  this.cart.set(null);
                  this.currentStep.set(4);
                  setTimeout(() => window.print(), 500);
                }
              });
            }, 1200);
          },
          error: (err: HttpErrorResponse) => {
            this.paymentProcessing.set(false);
            const msg = err.error?.message || 'Payment failed. Please try again.';
            this.toast.error(msg);
            this.currentStep.set(2);
          }
        });
    }, 1800);
  }

  applyCoupon(): void {
    const code = this.couponCode().trim();
    if (!code) {
      this.toast.warning('Please enter a coupon code first.');
      return;
    }
    this.busy.set(true);
    this.billingApi.validateCoupon(code).subscribe({
      next: (res) => {
        this.discountPercentage.set(res.discountPercentage);
        this.couponApplied.set(true);
        this.busy.set(false);
        this.toast.success(`Coupon "${code}" applied! ${res.discountPercentage}% off.`);
      },
      error: () => {
        this.busy.set(false);
        this.toast.error('Invalid or expired coupon.');
        this.couponCode.set('');
        this.discountPercentage.set(0);
        this.couponApplied.set(false);
      }
    });
  }

  proceedToPaymentWithCouponCheck(): void {
    const code = this.couponCode().trim();
    if (code && !this.couponApplied()) {
      this.toast.warning('You entered a coupon but forgot to click Apply!');
      return;
    }

    // Check shift status inline
    this.shiftApi.getCurrentShift().subscribe({
      next: (shift) => {
        if (!shift || !shift.id) {
          this.toast.info('You must open a shift before taking payment.');
          this.router.navigateByUrl('/shift');
        } else {
          this.proceedToPayment();
        }
      },
      error: (err) => {
        console.error('Shift check error:', err);
        // If there's a JSON parse error or any other error, assume no shift is open
        this.toast.info('You must open a shift before taking payment.');
        this.router.navigateByUrl('/shift');
      }
    });
  }

  goToOrders(): void {
    this.router.navigateByUrl('/orders');
  }

  startNewCart(): void {
    this.invoice.set(null);
    this.currentStep.set(1);
    this.paymentDone.set(false);
    this.paymentProcessing.set(false);
    this.couponCode.set('');
    this.couponApplied.set(false);
    this.discountPercentage.set(0);
    this.selectedPaymentMethod.set('Cash');
    this.amountReceived.set(null);
    this.loadData();
  }

  toStr(n: number): string {
    return String(n);
  }
}
