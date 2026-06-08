import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  if (!environment.useCookieAuth) {
    return next(req);
  }

  return next(req.clone({ withCredentials: true }));
};
