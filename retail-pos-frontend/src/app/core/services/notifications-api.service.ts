import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiConfig } from '../config/api.config';
import { NotificationItem } from '../types/api.models';

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);

  getNotifications(userId: number): Observable<NotificationItem[]> {
    return this.http.get<NotificationItem[]>(`${apiConfig.notifications}/${userId}`);
  }

  markRead(notificationId: number): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${apiConfig.notifications}/read/${notificationId}`,
      {}
    );
  }
}
