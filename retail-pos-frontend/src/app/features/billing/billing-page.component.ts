import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { BillingApiService } from '../../core/services/billing-api.service';
import { ProductsApiService } from '../../core/services/products-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { BillLineRequest, Invoice, Product } from '../../core/types/api.models';
import { InvoiceViewComponent } from '../../shared/components/invoice-view/invoice-view.component';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-billing-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DatePipe, CurrencyPipe, InvoiceViewComponent],
  templateUrl: './billing-page.component.html',
  styleUrl: './billing-page.component.scss'
})
export class BillingPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly billingApi = inject(BillingApiService);
  private readonly productsApi = inject(ProductsApiService);
  private readonly toast = inject(ToastService);

  readonly authStore = inject(AuthStore);
  readonly busy = signal(false);
  readonly products = signal<Product[]>([]);
  readonly history = signal<Invoice[]>([]);
  readonly selectedInvoice = signal<Invoice | null>(null);
  readonly draftLines = signal<BillLineRequest[]>([]);
  readonly activeTab = signal<'history' | 'compose'>('history');

  readonly billLookupForm = this.fb.nonNullable.group({
    billId: [0, [Validators.required, Validators.min(1)]]
  });

  readonly billComposerForm = this.fb.nonNullable.group({
    userId: [this.authStore.userId(), [Validators.required, Validators.min(1)]],
    paymentMethod: ['Cash', Validators.required],
    productId: [0, [Validators.required, Validators.min(1)]],
    quantity: [1, [Validators.required, Validators.min(1)]]
  });

  readonly couponCreateForm = this.fb.nonNullable.group({
    code: ['', Validators.required],
    discountPercentage: [10, [Validators.required, Validators.min(0.01)]],
    expiryDate: ['', Validators.required]
  });

  constructor() {
    this.loadData();
  }

  loadData(): void {
    this.busy.set(true);
    forkJoin({
      products: this.productsApi.getProducts(),
      history: this.billingApi.getHistory(this.authStore.userId())
    })
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: ({ products, history }) => {
          this.products.set(products);
          this.history.set(history);
        },
        error: () => this.toast.error('Unable to load billing data.')
      });
  }

  addLine(): void {
    const productId = this.billComposerForm.controls.productId.value;
    const quantity = this.billComposerForm.controls.quantity.value;
    if (!productId || quantity < 1) return;
    this.draftLines.update((lines) => [...lines, { productId, quantity }]);
    this.billComposerForm.patchValue({ productId: 0, quantity: 1 });
  }

  removeLine(index: number): void {
    this.draftLines.update((lines) => lines.filter((_, i) => i !== index));
  }

  getProductName(productId: number): string {
    return this.products().find((p) => p.id === productId)?.name ?? `Product #${productId}`;
  }

  createBill(): void {
    const paymentMethod = this.billComposerForm.controls.paymentMethod.value;
    const userId = this.authStore.isAdmin()
      ? this.billComposerForm.controls.userId.value
      : this.authStore.userId();

    if (!this.draftLines().length) {
      this.toast.warning('Add at least one line item before creating a bill.');
      return;
    }

    this.billingApi
      .createBill({ userId, paymentMethod, items: this.draftLines() })
      .subscribe({
        next: (summary) => {
          this.toast.success(`Bill #${summary.billId} created for ${summary.totalAmount} INR.`);
          this.draftLines.set([]);
          this.selectInvoice(summary.billId);
          this.loadData();
          this.activeTab.set('history');
        },
        error: () => this.toast.error('Unable to create bill.')
      });
  }

  lookupBill(): void {
    if (this.billLookupForm.invalid) {
      this.billLookupForm.markAllAsTouched();
      return;
    }
    this.billingApi.getBill(this.billLookupForm.controls.billId.value).subscribe({
      next: () => {
        this.selectInvoice(this.billLookupForm.controls.billId.value);
      },
      error: () => this.toast.error('Bill not found.')
    });
  }

  selectInvoice(billId: number): void {
    this.billingApi.getInvoice(billId).subscribe({
      next: (invoice) => this.selectedInvoice.set(invoice),
      error: () => this.toast.error('Unable to load invoice detail.')
    });
  }

  updatePaymentStatus(status: string): void {
    const invoice = this.selectedInvoice();
    if (!invoice) return;
    this.billingApi.updatePaymentStatus(invoice.billId, status).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.selectInvoice(invoice.billId);
        this.loadData();
      },
      error: () => this.toast.error('Unable to update payment status.')
    });
  }

  refund(): void {
    const invoice = this.selectedInvoice();
    if (!invoice) return;
    this.billingApi.refund(invoice.billId).subscribe({
      next: (response) => {
        this.toast.success(response.message);
        this.selectInvoice(invoice.billId);
        this.loadData();
      },
      error: () => this.toast.error('Unable to process refund.')
    });
  }

  createCoupon(): void {
    if (this.couponCreateForm.invalid) {
      this.couponCreateForm.markAllAsTouched();
      return;
    }
    this.billingApi.createCoupon(this.couponCreateForm.getRawValue()).subscribe({
      next: (response) => this.toast.success(response.message),
      error: () => this.toast.error('Unable to create coupon.')
    });
  }

  getStatusClass(status: string | null): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'paid' || s === 'completed') return 'success';
    if (s === 'refunded') return 'danger';
    if (s === 'pending') return 'pending';
    return 'info';
  }

  getTotalOrders(): number {
    return this.history().length;
  }

  getTotalSpent(): number {
    return this.history().reduce((sum, inv) => sum + inv.totalAmount, 0);
  }
}
