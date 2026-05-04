import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { BillingApiService } from '../../../core/services/billing-api.service';
import { Invoice, Product } from '../../../core/types/api.models';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-invoice-view',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './invoice-view.component.html',
  styleUrl: './invoice-view.component.scss'
})
export class InvoiceViewComponent {
  @Input() invoice!: Invoice;
  @Input() products: Product[] = [];

  private readonly billingApi = inject(BillingApiService);
  private readonly toast = inject(ToastService);

  getProductName(productId: number): string {
    return this.products.find((p) => p.id === productId)?.name ?? `Product #${productId}`;
  }

  getSubtotal(): number {
    if (typeof this.invoice?.subtotalAmount === 'number') return this.invoice.subtotalAmount;
    if (!this.invoice?.items?.length) return this.invoice?.totalAmount ?? 0;
    return this.invoice.items.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  getTotalTax(): number {
    if (typeof this.invoice?.taxAmount === 'number') return this.invoice.taxAmount;
    if (!this.invoice?.items?.length) return 0;
    return this.invoice.items.reduce((sum, item) => sum + (item.taxAmount ?? 0), 0);
  }

  getCurrencyCode(): string {
    return this.invoice?.currencyCode || 'INR';
  }

  getDiscount(): number {
    return this.invoice?.discountAmount ?? 0;
  }

  getStatusClass(status: string | null): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'paid' || s === 'completed') return 'success';
    if (s === 'refunded') return 'danger';
    if (s === 'pending') return 'pending';
    return 'info';
  }

  printInvoice(): void {
    window.print();
  }

  downloadPdf(): void {
    this.billingApi.downloadInvoicePdf(this.invoice.billId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `Invoice_${this.invoice.billId}.pdf`;
        anchor.click();
        URL.revokeObjectURL(url);
        this.toast.success(`Invoice #${this.invoice.billId} downloaded.`);
      },
      error: () => this.toast.error('Unable to download invoice PDF.')
    });
  }
}
