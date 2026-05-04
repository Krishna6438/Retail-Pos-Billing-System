import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import { Dashboard } from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${apiConfig.admin}/dashboard`);
  }
}
