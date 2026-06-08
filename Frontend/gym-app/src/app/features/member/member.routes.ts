import { Routes } from '@angular/router';

import { authGuard } from '../../core/guards/auth.guard';

import { permissionGuard } from '../../core/guards/permission.guard';

import { roleGuard } from '../../core/guards/role.guard';

import { Permissions } from '../../core/constants/permissions';

import { Roles } from '../../core/constants/roles';



export const MEMBER_ROUTES: Routes = [

  {

    path: '',

    loadComponent: () =>

      import('./member-layout.component').then((m) => m.MemberLayoutComponent),

    canActivate: [authGuard, roleGuard(Roles.Member)],

    children: [

      {

        path: '',

        redirectTo: 'dashboard',

        pathMatch: 'full',

      },

      {

        path: 'dashboard',

        canActivate: [permissionGuard(Permissions.ViewMemberDashboard)],

        loadComponent: () =>

          import('./dashboard/member-dashboard.component').then((m) => m.MemberDashboardComponent),

      },

      {

        path: 'profile',

        canActivate: [permissionGuard(Permissions.ViewMemberDetails)],

        loadComponent: () =>

          import('./profile/member-profile.component').then((m) => m.MemberProfileComponent),

      },

      {

        path: 'goals',

        canActivate: [permissionGuard(Permissions.ManageMemberGoals)],

        loadComponent: () =>

          import('./goals/member-goals.component').then((m) => m.MemberGoalsComponent),

      },

      {

        path: 'progress',

        canActivate: [permissionGuard(Permissions.TrackMemberProgress)],

        loadComponent: () =>

          import('./progress/member-progress.component').then((m) => m.MemberProgressComponent),

      },

      {

        path: 'workouts',

        canActivate: [permissionGuard(Permissions.TrackMemberProgress)],

        loadComponent: () =>

          import('./workouts/member-workouts.component').then((m) => m.MemberWorkoutsComponent),

      },

      {

        path: 'diets',

        canActivate: [permissionGuard(Permissions.TrackMemberProgress)],

        loadComponent: () =>

          import('./diets/member-diets.component').then((m) => m.MemberDietsComponent),

      },

      {

        path: 'water',

        canActivate: [permissionGuard(Permissions.TrackMemberProgress)],

        loadComponent: () =>

          import('./water/member-water.component').then((m) => m.MemberWaterComponent),

      },

      {

        path: 'referrals',

        canActivate: [permissionGuard(Permissions.ViewMemberDashboard)],

        loadComponent: () =>

          import('./referrals/member-referrals.component').then((m) => m.MemberReferralsComponent),

      },

      {

        path: 'feedback',

        canActivate: [permissionGuard(Permissions.SubmitMemberFeedback)],

        loadComponent: () =>

          import('./feedback/member-feedback.component').then((m) => m.MemberFeedbackComponent),

      },

      {

        path: 'diet',

        canActivate: [permissionGuard(Permissions.ViewMemberDiet)],

        loadComponent: () =>

          import('../gym-admin/diet/member-diet-view.component').then((m) => m.MemberDietViewComponent),

      },

      {

        path: 'workout',

        canActivate: [permissionGuard(Permissions.ViewMemberWorkout)],

        loadComponent: () =>

          import('../gym-admin/workout/member-workout-view.component').then((m) => m.MemberWorkoutViewComponent),

      },

      {

        path: 'checkout',

        canActivate: [permissionGuard(Permissions.InitiateOnlinePayment)],

        loadComponent: () =>

          import('./checkout/member-checkout.component').then((m) => m.MemberCheckoutComponent),

      },

      {

        path: 'bookings',

        canActivate: [permissionGuard(Permissions.ViewBookings)],

        loadComponent: () =>

          import('./bookings/member-bookings.component').then((m) => m.MemberBookingsComponent),

      },

      {

        path: 'bookings/history',

        canActivate: [permissionGuard(Permissions.ViewBookings)],

        loadComponent: () =>

          import('./bookings/member-booking-history.component').then((m) => m.MemberBookingHistoryComponent),

      },

    ],

  },

];


