import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import {
  Category,
  CreateCategoryRequest,
  CreateProductRequest,
  CreateTaxRequest,
  Product,
  TaxConfig,
  UpdateCategoryRequest,
  UpdateTaxRequest
} from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class ProductsApiService {
  private readonly http = inject(HttpClient);

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(apiConfig.products);
  }

  getLowStock(): Observable<Product[]> {
    return this.http.get<Product[]>(`${apiConfig.products}/low-stock`);
  }

  getProductByBarcode(barcode: string): Observable<Product> {
    return this.http.get<Product>(`${apiConfig.products}/barcode/${encodeURIComponent(barcode)}`);
  }

  createProduct(payload: CreateProductRequest): Observable<string> {
    return this.http.post(`${apiConfig.products}`, payload, { responseType: 'text' });
  }

  deleteProduct(productId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${apiConfig.products}/${productId}`);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(apiConfig.categories);
  }

  createCategory(payload: CreateCategoryRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(apiConfig.categories, payload);
  }

  updateCategory(payload: UpdateCategoryRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(apiConfig.categories, payload);
  }

  deleteCategory(categoryId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${apiConfig.categories}/${categoryId}`);
  }

  getTaxes(): Observable<TaxConfig[]> {
    return this.http.get<TaxConfig[]>(apiConfig.tax);
  }

  createTax(payload: CreateTaxRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(apiConfig.tax, payload);
  }

  updateTax(payload: UpdateTaxRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(apiConfig.tax, payload);
  }

  deleteTax(taxId: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${apiConfig.tax}/${taxId}`);
  }
}
