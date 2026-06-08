import { Routes } from '@angular/router';
import { changePasswordGuard } from '../../core/guards/change-password.guard';

export const AUTH_ROUTES: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('../../login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'forgot-password',
    loadComponent: () =>
      import('./forgot-password/forgot-password-page.component').then(
        (m) => m.ForgotPasswordPageComponent
      ),
  },
  {
    path: 'reset-password',
    loadComponent: () =>
      import('./reset-password/reset-password-page.component').then(
        (m) => m.ResetPasswordPageComponent
      ),
  },
  {
    path: 'change-password',
    canActivate: [changePasswordGuard],
    loadComponent: () =>
      import('./change-password/change-password-page.component').then(
        (m) => m.ChangePasswordPageComponent
      ),
  },
];
