import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

function extractMessage(error: HttpErrorResponse): string {
  const body = error.error as { message?: string; errors?: string[] } | null;
  if (body?.message) return body.message;
  if (body?.errors?.length) return body.errors.join(', ');
  if (error.status === 0) return 'Unable to reach the server. Check your connection.';
  if (error.status === 403) return 'You do not have permission to perform this action.';
  if (error.status === 404) return 'The requested resource was not found.';
  if (error.status === 429) return 'Too many requests. Please wait and try again.';
  if (error.status >= 500) return 'A server error occurred. Please try again later.';
  return error.message || 'An unexpected error occurred.';
}

export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const injector = inject(Injector);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const skipToast =
        req.url.includes('/auth/login') ||
        req.url.includes('/auth/validate') ||
        req.url.includes('/auth/session') ||
        req.url.includes('/public/white-label/login-branding');

      if (error.status === 401 && !req.url.includes('/auth/')) {
        injector.get(AuthService).clearSession();
        router.navigate(['/auth/login']);
        if (!skipToast) injector.get(NotificationService).error('Your session has expired. Please sign in again.');
      } else if (!skipToast && error.status !== 401) {
        injector.get(NotificationService).error(extractMessage(error));
      }

      return throwError(() => error);
    })
  );
};
