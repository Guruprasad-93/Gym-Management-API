import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { permissionGuard } from '../../core/guards/permission.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { Permissions } from '../../core/constants/permissions';
import { Roles } from '../../core/constants/roles';

export const TRAINER_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./trainer-layout.component').then((m) => m.TrainerLayoutComponent),
    canActivate: [authGuard, roleGuard(Roles.Trainer)],
    children: [
      {
        path: '',
        canActivate: [permissionGuard(Permissions.ViewDashboard)],
        loadComponent: () =>
          import('./dashboard/trainer-dashboard.component').then((m) => m.TrainerDashboardComponent),
      },
      {
        path: 'members',
        canActivate: [permissionGuard(Permissions.ViewMembers)],
        loadComponent: () =>
          import('./members/trainer-members.component').then((m) => m.TrainerMembersComponent),
      },
      {
        path: 'leads',
        canActivate: [permissionGuard(Permissions.ViewLeads)],
        loadComponent: () =>
          import('../gym-admin/leads/lead-list.component').then((m) => m.LeadListComponent),
      },
      {
        path: 'attendance',
        canActivate: [permissionGuard(Permissions.ViewAttendance)],
        loadComponent: () =>
          import('../gym-admin/attendance/attendance-dashboard.component').then((m) => m.AttendanceDashboardComponent),
      },
      {
        path: 'attendance/check-in',
        canActivate: [permissionGuard(Permissions.ManageAttendance)],
        loadComponent: () =>
          import('../gym-admin/attendance/attendance-check-in.component').then((m) => m.AttendanceCheckInComponent),
      },
      {
        path: 'attendance/check-out',
        canActivate: [permissionGuard(Permissions.ManageAttendance)],
        loadComponent: () =>
          import('../gym-admin/attendance/attendance-check-out.component').then((m) => m.AttendanceCheckOutComponent),
      },
      {
        path: 'workout-plans',
        canActivate: [permissionGuard(Permissions.ViewWorkoutPlans)],
        loadComponent: () =>
          import('../gym-admin/workout/workout-plan-list.component').then((m) => m.WorkoutPlanListComponent),
      },
      {
        path: 'members/:memberId/workout',
        canActivate: [permissionGuard(Permissions.ViewMemberWorkout)],
        loadComponent: () =>
          import('../gym-admin/workout/member-workout-view.component').then((m) => m.MemberWorkoutViewComponent),
      },
      {
        path: 'ai-recommendations',
        canActivate: [permissionGuard(Permissions.ViewAiRecommendations)],
        loadComponent: () =>
          import('./ai-recommendations/trainer-ai-recommendations.component').then((m) => m.TrainerAiRecommendationsComponent),
      },
      {
        path: 'schedule',
        canActivate: [permissionGuard(Permissions.ViewBookings)],
        loadComponent: () =>
          import('./bookings/trainer-schedule.component').then((m) => m.TrainerScheduleComponent),
      },
      {
        path: 'bookings',
        canActivate: [permissionGuard(Permissions.ViewBookings)],
        loadComponent: () =>
          import('./bookings/trainer-bookings.component').then((m) => m.TrainerBookingsComponent),
      },
    ],
  },
];
