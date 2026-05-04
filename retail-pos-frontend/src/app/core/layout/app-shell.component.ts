import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { CartApiService } from '../services/cart-api.service';
import { ShiftApiService } from '../services/shift-api.service';
import { ToastService } from '../../shared/services/toast.service';
import { AuthStore } from '../store/auth.store';

interface NavItem {
  label: string;
  path: string;
  icon: string;
  exact?: boolean;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  private readonly router = inject(Router);
  private readonly cartApi = inject(CartApiService);
  private readonly shiftApi = inject(ShiftApiService);
  private readonly toast = inject(ToastService);
  readonly authStore = inject(AuthStore);

  readonly menuOpen = signal(false);
  readonly cartBadge = signal(0);

  readonly navItems = computed<NavItem[]>(() => {
    const isAdmin = this.authStore.isAdmin();
    
    // Core items for all (or tailored based on role)
    const items: NavItem[] = [
      { label: 'Overview', path: '/', icon: '🏠', exact: true },
      { label: 'Products', path: '/products', icon: '🏷️' },
    ];

    if (isAdmin) {
      // Admin specific items
      items.push({ label: 'Admin', path: '/admin', icon: '⚙️' });
    } else {
      // Cashier specific items
      items.push(
        { label: 'Cart', path: '/cart', icon: '🛒' },
        { label: 'Orders', path: '/orders', icon: '📋' },
        { label: 'Shift', path: '/shift', icon: '⏱️' }
      );
    }

    // Notifications for everyone
    items.push({ label: 'Notifications', path: '/notifications', icon: '🔔' });

    return items;
  });

  constructor() {
    // Close menu on navigation
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)).subscribe(() => {
      this.menuOpen.set(false);
    });

    // Load cart badge
    this.loadCartBadge();
  }

  private loadCartBadge(): void {
    const userId = this.authStore.userId();
    if (!userId) return;
    this.cartApi.getCart(userId).subscribe({
      next: (cart) => {
        const count = cart?.items?.reduce((s, i) => s + i.quantity, 0) ?? 0;
        this.cartBadge.set(count);
      },
      error: () => this.cartBadge.set(0)
    });
  }

  toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  logout(): void {
    // Intercept logout to verify shift is closed
    this.shiftApi.getCurrentShift().subscribe({
      next: (shift) => {
        if (shift && shift.id) {
          this.toast.error('You must close your active shift before logging out.');
          this.router.navigateByUrl('/shift');
        } else {
          this.proceedLogout();
        }
      },
      error: () => {
        this.proceedLogout();
      }
    });
  }

  private proceedLogout(): void {
    this.authStore.clear();
    this.router.navigateByUrl('/auth');
  }
}
