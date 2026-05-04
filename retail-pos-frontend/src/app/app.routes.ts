import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/auth-page.component').then((m) => m.AuthPageComponent),
    title: 'RetailPOS | Sign In'
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./core/layout/app-shell.component').then((m) => m.AppShellComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/pages/home-page.component').then((m) => m.HomePageComponent),
        title: 'RetailPOS | Overview'
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/products/products-page.component').then((m) => m.ProductsPageComponent),
        title: 'RetailPOS | Products'
      },
      {
        path: 'cart',
        loadComponent: () =>
          import('./features/cart/cart-page.component').then((m) => m.CartPageComponent),
        title: 'RetailPOS | Cart'
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/billing/billing-page.component').then((m) => m.BillingPageComponent),
        title: 'RetailPOS | Orders'
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications-page.component').then(
            (m) => m.NotificationsPageComponent
          ),
        title: 'RetailPOS | Notifications'
      },
      {
        path: 'shift',
        loadComponent: () =>
          import('./features/shift/shift-page.component').then((m) => m.ShiftPageComponent),
        title: 'RetailPOS | Shift Management'
      },
      {
        path: 'admin',
        canActivate: [roleGuard],
        data: { role: 'Admin' },
        loadComponent: () =>
          import('./features/admin/admin-page.component').then((m) => m.AdminPageComponent),
        title: 'RetailPOS | Admin Dashboard'
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
