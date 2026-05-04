import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationsApiService } from '../../core/services/notifications-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { NotificationItem } from '../../core/types/api.models';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe, CurrencyPipe],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {
  readonly authStore = inject(AuthStore);
  private readonly notificationsApi = inject(NotificationsApiService);

  readonly recentNotifications = signal<NotificationItem[]>([]);

  constructor() {
    this.notificationsApi.getNotifications(this.authStore.userId()).subscribe({
      next: (items) => this.recentNotifications.set(items.slice(0, 3)),
      error: () => this.recentNotifications.set([])
    });
  }
}
