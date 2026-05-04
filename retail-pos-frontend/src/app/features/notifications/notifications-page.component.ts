import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { NotificationsApiService } from '../../core/services/notifications-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { NotificationItem } from '../../core/types/api.models';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.scss'
})
export class NotificationsPageComponent {
  private readonly notificationsApi = inject(NotificationsApiService);
  private readonly authStore = inject(AuthStore);
  private readonly toast = inject(ToastService);

  readonly notifications = signal<NotificationItem[]>([]);
  readonly busy = signal(false);

  constructor() {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.busy.set(true);
    this.notificationsApi
      .getNotifications(this.authStore.userId())
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (items) => this.notifications.set(items),
        error: () => this.toast.error('Unable to load notifications.')
      });
  }

  markRead(item: NotificationItem): void {
    this.notificationsApi.markRead(item.id).subscribe({
      next: () => {
        this.notifications.update((items) =>
          items.map((n) => (n.id === item.id ? { ...n, isRead: true } : n))
        );
        this.toast.success('Notification marked as read.');
      },
      error: () => this.toast.error('Unable to update notification.')
    });
  }

  markAllRead(): void {
    const unread = this.notifications().filter((n) => !n.isRead);
    unread.forEach((item) => this.markRead(item));
    if (!unread.length) this.toast.info('All notifications already read.');
  }

  getUnreadCount(): number {
    return this.notifications().filter((n) => !n.isRead).length;
  }

  getTypeIcon(type: string | null): string {
    const t = (type || '').toLowerCase();
    if (t.includes('stock') || t.includes('inventory')) return '📦';
    if (t.includes('order') || t.includes('bill')) return '📋';
    if (t.includes('payment')) return '💳';
    if (t.includes('alert') || t.includes('warn')) return '⚠️';
    if (t.includes('success')) return '✅';
    return '🔔';
  }
}
