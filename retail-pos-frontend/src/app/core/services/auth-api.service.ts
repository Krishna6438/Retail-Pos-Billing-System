import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import { AuthResponse, LoginRequest, RegisterRequest } from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);

  login(payload: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${apiConfig.auth}/login`, payload);
  }

  register(payload: RegisterRequest): Observable<string> {
    return this.http.post(`${apiConfig.auth}/register`, payload, {
      responseType: 'text'
    });
  }
}
