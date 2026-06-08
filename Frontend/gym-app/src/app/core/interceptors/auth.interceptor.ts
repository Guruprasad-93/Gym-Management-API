import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshCompleted = new BehaviorSubject<boolean>(false);

function isAuthEndpoint(url: string): boolean {
  return (
    url.includes('/auth/login') ||
    url.includes('/auth/refresh') ||
    url.includes('/auth/forgot-password') ||
    url.includes('/auth/reset-password') ||
    url.includes('/auth/csrf')
  );
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Injector).get(AuthService);

  const authReq =
    !environment.useCookieAuth && auth.getToken() && !isAuthEndpoint(req.url)
      ? req.clone({ setHeaders: { Authorization: `Bearer ${auth.getToken()}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint(req.url)) {
        return throwError(() => error);
      }

      if (!isRefreshing) {
        isRefreshing = true;
        refreshCompleted.next(false);

        return auth.refreshToken().pipe(
          switchMap((response) => {
            isRefreshing = false;
            if (response.success && response.data) {
              refreshCompleted.next(true);
              const retryReq =
                !environment.useCookieAuth && response.data.token
                  ? req.clone({ setHeaders: { Authorization: `Bearer ${response.data.token}` } })
                  : req;
              return next(retryReq);
            }
            auth.logout();
            return throwError(() => error);
          }),
          catchError((refreshError) => {
            isRefreshing = false;
            auth.logout();
            return throwError(() => refreshError);
          })
        );
      }

      return refreshCompleted.pipe(
        filter((done) => done),
        take(1),
        switchMap(() => next(req))
      );
    })
  );
};
