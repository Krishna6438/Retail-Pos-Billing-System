import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(error: unknown, fallback = 'Something went wrong.'): string {
  if (error instanceof HttpErrorResponse) {
    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    if (error.error?.message) {
      return error.error.message as string;
    }
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}
