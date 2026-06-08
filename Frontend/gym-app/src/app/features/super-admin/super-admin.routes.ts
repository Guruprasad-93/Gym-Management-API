import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { permissionGuard } from '../../core/guards/permission.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { Permissions } from '../../core/constants/permissions';
import { Roles } from '../../core/constants/roles';

export const SUPER_ADMIN_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./super-admin-layout.component').then((m) => m.SuperAdminLayoutComponent),
    canActivate: [authGuard, roleGuard(Roles.SuperAdmin)],
    children: [
      {
        path: '',
        canActivate: [permissionGuard(Permissions.ViewDashboard)],
        loadComponent: () =>
          import('./dashboard/super-admin-dashboard.component').then(
            (m) => m.SuperAdminDashboardComponent
          ),
      },
      {
        path: 'gyms',
        canActivate: [permissionGuard(Permissions.ViewGyms)],
        loadComponent: () =>
          import('./gyms/gym-list.component').then((m) => m.GymListComponent),
      },
      {
        path: 'gym-admins',
        canActivate: [permissionGuard(Permissions.ViewGymAdmins)],
        loadComponent: () =>
          import('./gym-admins/gym-admin-list.component').then((m) => m.GymAdminListComponent),
      },
      {
        path: 'roles',
        canActivate: [permissionGuard(Permissions.ViewRoles)],
        loadComponent: () =>
          import('./roles/role-list.component').then((m) => m.RoleListComponent),
      },
      {
        path: 'privileges',
        canActivate: [permissionGuard(Permissions.ViewPrivileges)],
        loadComponent: () =>
          import('./privileges/privilege-list.component').then((m) => m.PrivilegeListComponent),
      },
      {
        path: 'role-matrix',
        canActivate: [permissionGuard(Permissions.ViewPermissionMatrix)],
        loadComponent: () =>
          import('./role-matrix/role-matrix.component').then((m) => m.RoleMatrixComponent),
      },
      {
        path: 'audit',
        canActivate: [permissionGuard(Permissions.ViewAuditLogs)],
        loadComponent: () =>
          import('../gym-admin/audit/audit-dashboard.component').then((m) => m.AuditDashboardComponent),
      },
      {
        path: 'white-label',
        canActivate: [permissionGuard(Permissions.ViewPlatformSaas)],
        loadComponent: () =>
          import('./white-label/super-admin-white-label.component').then((m) => m.SuperAdminWhiteLabelComponent),
      },
    ],
  },
];
