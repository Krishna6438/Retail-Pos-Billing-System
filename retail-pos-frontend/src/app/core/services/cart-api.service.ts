import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import {
  AddToCartRequest,
  BillSummary,
  Cart,
  CheckoutRequest,
  UpdateCartRequest
} from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class CartApiService {
  private readonly http = inject(HttpClient);

  getCart(userId: number): Observable<Cart> {
    return this.http.get<Cart>(`${apiConfig.bills}/cart/${userId}`);
  }

  addItem(payload: AddToCartRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.bills}/cart/add-item`, payload);
  }

  updateItem(payload: UpdateCartRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${apiConfig.bills}/cart/update-item`, payload);
  }

  removeItem(userId: number, productId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${apiConfig.bills}/cart/remove-item/${userId}/${productId}`
    );
  }

  holdCart(cartId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.bills}/cart/${cartId}/hold`, {});
  }

  resumeCart(cartId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${apiConfig.bills}/cart/${cartId}/resume`, {});
  }

  clearCart(cartId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${apiConfig.bills}/cart/${cartId}/clear`);
  }

  checkoutCart(cartId: number, payload: CheckoutRequest): Observable<BillSummary> {
    return this.http.post<BillSummary>(`${apiConfig.bills}/cart/${cartId}/checkout`, payload);
  }
}
