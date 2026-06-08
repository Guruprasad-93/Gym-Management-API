import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard =
  (...roles: string[]): CanActivateFn =>
  () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.isAuthenticated() && roles.some((r) => auth.hasRole(r))) {
      return true;
    }

    return router.createUrlTree(['/auth/login']);
  };
