import { Injectable, computed, signal } from '@angular/core';
import { SessionUser, UserRole } from '../types/api.models';

const STORAGE_KEY = 'retail-pos.session';

interface JwtPayload {
  email?: string;
  role?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly sessionSignal = signal<SessionUser | null>(this.readStoredSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.sessionSignal());
  readonly userId = computed(() => this.sessionSignal()?.userId ?? 0);
  readonly role = computed<UserRole>(() => this.sessionSignal()?.role ?? 'User');
  readonly isAdmin = computed(() => this.role() === 'Admin');
  readonly token = computed(() => this.sessionSignal()?.token ?? '');
  readonly email = computed(() => this.sessionSignal()?.email ?? '');

  setSession(token: string, email?: string, role?: string): void {
    const payload = this.parseJwt(token);
    const session: SessionUser = {
      token,
      email:
        email ??
        payload.email ??
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
        '',
      role: this.normalizeRole(
        role ??
          payload.role ??
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
          'User',
        email ?? payload.email ?? ''
      ),
      userId: Number(
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ?? 0
      )
    };

    this.sessionSignal.set(session);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  clear(): void {
    this.sessionSignal.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private readStoredSession(): SessionUser | null {
    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as SessionUser;
      if (parsed && parsed.role) {
        parsed.role = this.normalizeRole(parsed.role, parsed.email);
      }
      return parsed?.token ? parsed : null;
    } catch {
      return null;
    }
  }

  private parseJwt(token: string): JwtPayload {
    try {
      const payload = token.split('.')[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const decoded = atob(normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '='));
      return JSON.parse(decoded) as JwtPayload;
    } catch {
      return {};
    }
  }

  private normalizeRole(role: string, email?: string): UserRole {
    // Safety override for the main admin email
    if (email === 'admin@pos.com') {
      return 'Admin';
    }

    if (role === 'Admin' || role === '1') {
      return 'Admin';
    }
    if (role === 'Cashier' || role === '2') {
      return 'Cashier';
    }

    return 'User';
  }
}
