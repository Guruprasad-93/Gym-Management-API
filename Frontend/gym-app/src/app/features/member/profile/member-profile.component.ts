import { CurrencyPipe, DatePipe } from '@angular/common';

import { Component, OnInit, inject, signal } from '@angular/core';

import { RouterLink } from '@angular/router';

import { MatIconModule } from '@angular/material/icon';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { forkJoin } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';

import { MemberService } from '../../../core/services/member.service';

import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';

import { NotificationService } from '../../../core/services/notification.service';

import { MemberDetails } from '../../../shared/models/member.models';

import { ProfilePhotoManagerComponent } from '../../../shared/components/profile-photo-manager/profile-photo-manager.component';

import { MemberFilesGalleryComponent } from '../../../shared/components/member-files-gallery/member-files-gallery.component';

import { FileCategories } from '../../../shared/models/file.models';

import {

  MemberFeedback,

  MemberProgressEntry,

  MemberSelfServiceDashboard,

  WorkoutTracking,

} from '../../../shared/models/member-self-service.models';



interface ProfileTab {

  id: 'overview' | 'membership' | 'payments' | 'measurements' | 'documents';

  label: string;

}



@Component({

  selector: 'app-member-profile',

  standalone: true,

  imports: [

    CurrencyPipe,

    DatePipe,

    RouterLink,

    MatIconModule,

    MatProgressSpinnerModule,

    ProfilePhotoManagerComponent,

    MemberFilesGalleryComponent,

  ],

  templateUrl: './member-profile.component.html',

  styleUrl: './member-profile.component.css',

})

export class MemberProfileComponent implements OnInit {

  readonly auth = inject(AuthService);

  private readonly memberService = inject(MemberService);

  private readonly selfService = inject(MemberSelfServiceService);

  private readonly notify = inject(NotificationService);



  readonly fileCategories = FileCategories;

  readonly tabs: ProfileTab[] = [

    { id: 'overview', label: 'Overview' },

    { id: 'membership', label: 'Membership' },

    { id: 'payments', label: 'Payments' },

    { id: 'measurements', label: 'Measurements' },

    { id: 'documents', label: 'Documents' },

  ];



  profile: MemberDetails | null = null;

  dashboard: MemberSelfServiceDashboard | null = null;

  progressEntries = signal<MemberProgressEntry[]>([]);

  feedbackItems = signal<MemberFeedback[]>([]);

  workoutStats = { total: 0, thisMonth: 0, avgCompletion: 0, latestPlan: '—' };



  loading = true;

  activeTab = signal<ProfileTab['id']>('overview');



  ngOnInit(): void {

    this.memberService.getMe().subscribe({

      next: (meRes) => {

        if (!meRes.success || !meRes.data) {

          this.loading = false;

          this.notify.error('Member profile not found');

          return;

        }



        forkJoin({

          details: this.memberService.getDetails(meRes.data.id),

          dashboard: this.selfService.getDashboard(),

          progress: this.selfService.getProgressTrends(),

          workouts: this.selfService.getWorkouts(),

          feedback: this.selfService.getFeedback(),

        }).subscribe({

          next: ({ details, dashboard, progress, workouts, feedback }) => {

            this.loading = false;

            if (details.success && details.data) this.profile = details.data;

            if (dashboard.success && dashboard.data) this.dashboard = dashboard.data;

            if (progress.success && progress.data) this.progressEntries.set(progress.data.entries ?? []);

            if (feedback.success && feedback.data) this.feedbackItems.set(feedback.data);

            if (workouts.success && workouts.data) this.computeWorkoutStats(workouts.data);

          },

          error: () => {

            this.loading = false;

            this.notify.error('Failed to load profile');

          },

        });

      },

      error: () => {

        this.loading = false;

        this.notify.error('Failed to load member profile');

      },

    });

  }



  memberCode(): string {

    return this.profile ? `MEM${this.profile.id.toString().padStart(5, '0')}` : '—';

  }



  dobLabel(): string {
    if (!this.profile?.dateOfBirth && this.profile?.age == null) return '—';
    const dob = this.profile?.dateOfBirth
      ? new Date(this.profile.dateOfBirth).toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
      : null;
    const age = this.profile?.age;
    if (dob && age != null) return `${dob} (${age} years)`;
    if (dob) return dob;
    if (age != null) return `${age} years`;
    return '—';
  }



  daysRemaining(): number | null {

    if (!this.profile?.membershipEndDate) return null;

    const end = new Date(this.profile.membershipEndDate);

    const today = new Date();

    today.setHours(0, 0, 0, 0);

    end.setHours(0, 0, 0, 0);

    return Math.max(0, Math.ceil((end.getTime() - today.getTime()) / 86400000));

  }



  recentPayments() {

    return (this.profile?.paymentHistory ?? []).slice(0, 5);

  }



  latestProgress(): MemberProgressEntry | null {

    return this.progressEntries()[0] ?? null;

  }



  latestFeedback(): MemberFeedback | null {

    return this.feedbackItems()[0] ?? null;

  }



  latestPaymentAmount(): number {

    return this.profile?.paymentHistory[0]?.amount ?? 0;

  }



  latestPaymentMethod(): string {

    return this.profile?.paymentHistory[0]?.paymentMethod ?? '—';

  }



  activeMembershipStart(): string | null {

    return this.dashboard?.activeMembership?.startDate ?? this.profile?.joinDate ?? null;

  }



  membershipDuration(): string {

    const start = this.activeMembershipStart();

    const end = this.profile?.membershipEndDate;

    if (!start || !end) return '—';

    const months = Math.round((new Date(end).getTime() - new Date(start).getTime()) / (86400000 * 30));

    return months > 0 ? `${months} months` : '—';

  }



  weightLabel(): string {

    const w = this.latestProgress()?.weight ?? this.profile?.weight;

    return w != null ? `${w} kg` : '—';

  }



  bmiLabel(): string {

    const bmi = this.latestProgress()?.bmi;

    if (bmi != null) return `${bmi}`;

    const h = this.profile?.height;

    const w = this.profile?.weight;

    if (h && w) {

      const m = h / 100;

      return (w / (m * m)).toFixed(1);

    }

    return '—';

  }



  formatGoalType(goalType: string): string {

    return goalType.replace(/([a-z])([A-Z])/g, '$1 $2');

  }



  private computeWorkoutStats(workouts: WorkoutTracking[]): void {

    const now = new Date();

    const month = now.getMonth();

    const year = now.getFullYear();

    const thisMonth = workouts.filter((w) => {

      const d = new Date(w.workoutDate);

      return d.getMonth() === month && d.getFullYear() === year;

    });



    this.workoutStats = {

      total: workouts.length,

      thisMonth: thisMonth.length,

      avgCompletion: workouts.length

        ? Math.round(workouts.reduce((sum, w) => sum + w.completionPercentage, 0) / workouts.length)

        : 0,

      latestPlan: workouts[0]?.workoutPlanName ?? '—',

    };

  }

}

