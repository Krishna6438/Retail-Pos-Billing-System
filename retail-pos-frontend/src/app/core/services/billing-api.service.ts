import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import {
  ApplyCouponRequest,
  BillSummary,
  CreateBillRequest,
  CouponRequest,
  DailySales,
  Invoice,
  TopProduct
} from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class BillingApiService {
  private readonly http = inject(HttpClient);

  createBill(payload: CreateBillRequest): Observable<BillSummary> {
    return this.http.post<BillSummary>(apiConfig.billing, payload);
  }

  getBill(billId: number): Observable<BillSummary> {
    return this.http.get<BillSummary>(`${apiConfig.billing}/${billId}`);
  }

  getHistory(userId: number): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${apiConfig.billing}/history/${userId}`);
  }

  getInvoice(billId: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${apiConfig.billing}/invoice/${billId}`);
  }

  downloadInvoicePdf(billId: number): Observable<Blob> {
    return this.http.get(`${apiConfig.billing}/invoice/pdf/${billId}`, {
      responseType: 'blob'
    });
  }

  applyCoupon(payload: ApplyCouponRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.billing}/coupon/apply`, payload);
  }

  createCoupon(payload: CouponRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.billing}/coupon`, payload);
  }

  updatePaymentStatus(billId: number, status: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${apiConfig.billing}/payment/${billId}?status=${encodeURIComponent(status)}`,
      {}
    );
  }

  refund(billId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.billing}/refund/${billId}`, {});
  }

  validateCoupon(code: string): Observable<{ discountPercentage: number }> {
    return this.http.get<{ discountPercentage: number }>(`${apiConfig.billing}/coupon/${code}`);
  }

  getTodaySales(): Observable<DailySales> {
    return this.http.get<DailySales>(`${apiConfig.billing}/analytics/today`);
  }

  getTopProducts(): Observable<TopProduct[]> {
    return this.http.get<TopProduct[]>(`${apiConfig.billing}/analytics/top-products`);
  }

  getRevenue(): Observable<number> {
    return this.http.get<number>(`${apiConfig.billing}/analytics/revenue`);
  }
}
