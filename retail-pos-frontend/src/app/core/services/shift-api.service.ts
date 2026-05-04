import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';

export interface Shift {
  id: number;
  userId: number;
  openedAt: string;
  closedAt?: string;
  startingFloat: number;
  expectedCash: number;
  actualCash?: number;
  variance?: number;
  status: 'Open' | 'Closed';
}

@Injectable({ providedIn: 'root' })
export class ShiftApiService {
  private readonly http = inject(HttpClient);

  getCurrentShift(): Observable<Shift | null> {
    return this.http.get<Shift | null>(`${apiConfig.bills}/shifts/current`);
  }

  openShift(startingFloat: number): Observable<{ message: string; shiftId: number }> {
    return this.http.post<{ message: string; shiftId: number }>(`${apiConfig.bills}/shifts/open`, { startingFloat });
  }

  closeShift(shiftId: number, actualCash: number): Observable<any> {
    return this.http.post<any>(`${apiConfig.bills}/shifts/${shiftId}/close`, { actualCash });
  }

  getAllShifts(): Observable<Shift[]> {
    return this.http.get<Shift[]>(`${apiConfig.bills}/shifts/all`);
  }
}
