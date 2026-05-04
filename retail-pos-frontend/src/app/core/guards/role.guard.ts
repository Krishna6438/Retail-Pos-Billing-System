import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../store/auth.store';

export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);
  const requiredRole = route.data['role'] as string | undefined;

  if (!requiredRole || authStore.role() === requiredRole) {
    return true;
  }

  return router.createUrlTree(['/']);
};
