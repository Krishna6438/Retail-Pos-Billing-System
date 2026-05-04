import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthApiService } from '../../core/services/auth-api.service';
import { AuthStore } from '../../core/store/auth.store';
import { errorMessage } from '../../shared/utils/error-message';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss'
})
export class AuthPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  readonly mode = signal<'login' | 'register'>('login');
  readonly busy = signal(false);
  readonly feedback = signal('');

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  readonly registerForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    storeId: [1, [Validators.required]]
  });

  switchMode(mode: 'login' | 'register'): void {
    this.mode.set(mode);
    this.feedback.set('');
  }

  submitLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.feedback.set('');

    this.authApi
      .login(this.loginForm.getRawValue())
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (response) => {
          this.authStore.setSession(response.token, response.email, response.role);
          this.router.navigateByUrl('/');
        },
        error: (error) => this.feedback.set(errorMessage(error, 'Unable to sign in right now.'))
      });
  }

  submitRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.feedback.set('');

    this.authApi
      .register(this.registerForm.getRawValue())
      .pipe(finalize(() => this.busy.set(false)))
      .subscribe({
        next: (message) => {
          this.feedback.set(message || 'Account created. You can sign in now.');
          this.mode.set('login');
          this.loginForm.patchValue({ email: this.registerForm.controls.email.value });
        },
        error: (error) => this.feedback.set(errorMessage(error, 'Unable to create account.'))
      });
  }
}
