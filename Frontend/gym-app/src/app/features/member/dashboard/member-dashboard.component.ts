import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { MemberSelfServiceDashboard } from '../../../shared/models/member-self-service.models';

interface QuickAction {
  label: string;
  description: string;
  icon: string;
  route: string;
  color: string;
}

@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
  ],
  templateUrl: './member-dashboard.component.html',
  styleUrl: './member-dashboard.component.css',
})
export class MemberDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);

  readonly quickActions: QuickAction[] = [
    { label: 'Goals', description: 'Set and track fitness goals', icon: 'flag', route: '/member/goals', color: '#ff6600' },
    { label: 'Progress', description: 'Body metrics and photos', icon: 'trending_up', route: '/member/progress', color: '#2e90fa' },
    { label: 'Workouts', description: 'Log daily workout completion', icon: 'fitness_center', route: '/member/workouts', color: '#7a5af8' },
    { label: 'Diet', description: 'Track meals and compliance', icon: 'restaurant', route: '/member/diets', color: '#12b76a' },
    { label: 'Water', description: 'Monitor hydration intake', icon: 'water_drop', route: '/member/water', color: '#06aed4' },
    { label: 'Referrals', description: 'Invite friends and earn points', icon: 'card_giftcard', route: '/member/referrals', color: '#f79009' },
  ];

  loading = signal(true);
  dashboard = signal<MemberSelfServiceDashboard | null>(null);
  qrImage = signal<string | null>(null);

  ngOnInit(): void {
    this.service.getDashboard().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.dashboard.set(res.data);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load dashboard');
      },
    });

    this.service.getQrCode().subscribe({
      next: (res) => {
        if (res.success && res.data) this.qrImage.set(res.data.qrCodeBase64);
      },
    });
  }

  referralHint(data: MemberSelfServiceDashboard): string {
    const converted = data.referralStats.convertedReferrals;
    const total = data.referralStats.totalReferrals;
    return total ? `${converted}/${total} converted` : 'No referrals yet';
  }

  formatGoalType(goalType: string): string {
    return goalType.replace(/([a-z])([A-Z])/g, '$1 $2');
  }
}
