import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  { path: 'login', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'register',
    loadComponent: () => import('./register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'super-admin',
    loadChildren: () =>
      import('./features/super-admin/super-admin.routes').then((m) => m.SUPER_ADMIN_ROUTES),
  },
  {
    path: 'gym-admin',
    loadChildren: () =>
      import('./features/gym-admin/gym-admin.routes').then((m) => m.GYM_ADMIN_ROUTES),
  },
  {
    path: 'trainer',
    loadChildren: () =>
      import('./features/trainer/trainer.routes').then((m) => m.TRAINER_ROUTES),
  },
  {
    path: 'member',
    loadChildren: () =>
      import('./features/member/member.routes').then((m) => m.MEMBER_ROUTES),
  },
  {
    path: 'website/:gymSlug',
    loadComponent: () =>
      import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteShellComponent),
    children: [
      { path: '', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteHomeComponent) },
      { path: 'about', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteAboutComponent) },
      { path: 'plans', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsitePlansComponent) },
      { path: 'trainers', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteTrainersComponent) },
      { path: 'gallery', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteGalleryPageComponent) },
      { path: 'contact', loadComponent: () => import('./features/public-website/public-website.component').then((m) => m.PublicWebsiteContactComponent) },
    ],
  },
  { path: '**', redirectTo: 'auth/login' },
];
