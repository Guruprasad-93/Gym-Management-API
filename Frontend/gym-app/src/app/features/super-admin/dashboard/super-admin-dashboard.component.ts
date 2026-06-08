import { CommonModule } from '@angular/common';
import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { forkJoin } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DashboardService, DashboardStats } from '../../../core/services/dashboard.service';
import { SaasSubscriptionService } from '../../../core/services/saas-subscription.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasPlatformDashboard } from '../../../shared/models/saas.models';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';

Chart.register(...registerables);

interface QuickAction {
  label: string;
  description: string;
  icon: string;
  route: string;
  color: string;
}

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './super-admin-dashboard.component.html',
  styleUrl: './super-admin-dashboard.component.css',
})
export class SuperAdminDashboardComponent implements OnInit, AfterViewInit {
  readonly auth = inject(AuthService);
  private readonly dashboard = inject(DashboardService);
  private readonly saasService = inject(SaasSubscriptionService);
  private readonly notify = inject(NotificationService);

  @ViewChild('overviewChartCanvas') overviewChartCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('subscriptionChartCanvas') subscriptionChartCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('revenueChartCanvas') revenueChartCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('membershipChartCanvas') membershipChartCanvas?: ElementRef<HTMLCanvasElement>;

  stats: DashboardStats | null = null;
  saas: SaasPlatformDashboard | null = null;
  loading = true;

  readonly quickActions: QuickAction[] = [
    { label: 'Manage Gyms', description: 'View and configure gym tenants', icon: 'fitness_center', route: '/super-admin/gyms', color: '#ff6600' },
    { label: 'Gym Admins', description: 'Manage platform administrators', icon: 'manage_accounts', route: '/super-admin/gym-admins', color: '#2e90fa' },
    { label: 'Roles & Access', description: 'Configure roles and permissions', icon: 'admin_panel_settings', route: '/super-admin/roles', color: '#7a5af8' },
    { label: 'White Label', description: 'Platform branding and SaaS settings', icon: 'palette', route: '/super-admin/white-label', color: '#12b76a' },
    { label: 'Audit Logs', description: 'Review platform activity history', icon: 'history', route: '/super-admin/audit', color: '#f79009' },
    { label: 'Role Matrix', description: 'Permission matrix overview', icon: 'grid_on', route: '/super-admin/role-matrix', color: '#06aed4' },
  ];

  private charts: Chart[] = [];

  get activeGymRate(): string {
    if (!this.stats?.totalGyms) return '';
    const pct = Math.round((this.stats.activeGyms / this.stats.totalGyms) * 100);
    return `${pct}% active`;
  }

  get activeMemberRate(): string {
    if (!this.stats?.totalMembers) return '';
    const pct = Math.round((this.stats.activeMembers / this.stats.totalMembers) * 100);
    return `${pct}% active`;
  }

  get trainerAssignmentRate(): string {
    if (!this.stats?.totalMembers) return '';
    const pct = Math.round((this.stats.membersWithTrainer / this.stats.totalMembers) * 100);
    return `${pct}% assigned`;
  }

  ngOnInit(): void {
    forkJoin({
      stats: this.dashboard.getStats(),
      saas: this.saasService.getPlatformDashboard(),
    }).subscribe({
      next: ({ stats, saas }) => {
        this.loading = false;
        if (stats.success && stats.data) {
          this.stats = stats.data;
          setTimeout(() => this.renderCharts());
        }
        if (saas.success && saas.data) this.saas = saas.data;
        setTimeout(() => this.renderCharts());
      },
      error: () => {
        this.loading = false;
        this.notify.error('Failed to load dashboard');
      },
    });
  }

  ngAfterViewInit(): void {
    if (this.stats) this.renderCharts();
  }

  private renderCharts(): void {
    if (!this.stats) return;
    this.destroyCharts();
    this.renderOverviewChart();
    this.renderSubscriptionChart();
    this.renderRevenueChart();
    this.renderMembershipChart();
  }

  private destroyCharts(): void {
    this.charts.forEach((chart) => chart.destroy());
    this.charts = [];
  }

  private renderOverviewChart(): void {
    if (!this.overviewChartCanvas?.nativeElement || !this.stats) return;

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels: ['Gyms', 'Active Gyms', 'Members', 'Trainers', 'Expired Memberships'],
        datasets: [
          {
            label: 'Counts',
            data: [
              this.stats.totalGyms,
              this.stats.activeGyms,
              this.stats.totalMembers,
              this.stats.totalTrainers,
              this.stats.expiredMemberships,
            ],
            backgroundColor: ['#ff6600', '#12b76a', '#2e90fa', '#06aed4', '#f04438'],
            borderRadius: 8,
            borderSkipped: false,
          },
        ],
      },
      options: this.baseChartOptions(false),
    };

    this.charts.push(new Chart(this.overviewChartCanvas.nativeElement, config));
  }

  private renderSubscriptionChart(): void {
    if (!this.subscriptionChartCanvas?.nativeElement || !this.saas) return;

    const config: ChartConfiguration<'doughnut'> = {
      type: 'doughnut',
      data: {
        labels: ['Active', 'Trial', 'Expired'],
        datasets: [
          {
            data: [
              this.saas.activeSubscriptions,
              this.saas.trialSubscriptions,
              this.saas.expiredSubscriptions,
            ],
            backgroundColor: ['#12b76a', '#f79009', '#f04438'],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: '68%',
        plugins: {
          legend: {
            position: 'right',
            labels: { usePointStyle: true, boxWidth: 8, padding: 16, color: '#667085' },
          },
        },
      },
    };

    this.charts.push(new Chart(this.subscriptionChartCanvas.nativeElement, config));
  }

  private renderRevenueChart(): void {
    if (!this.revenueChartCanvas?.nativeElement || !this.stats) return;

    const labels = ['Total Revenue', 'Monthly Revenue'];
    const values = [this.stats.totalRevenue, this.stats.monthlyRevenue];
    if (this.saas) {
      labels.push('MRR', 'ARR');
      values.push(this.saas.monthlyRecurringRevenue, this.saas.annualRecurringRevenue);
    }

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'Revenue',
            data: values,
            backgroundColor: ['#7a5af8', '#ff6600', '#2e90fa', '#12b76a'],
            borderRadius: 8,
            borderSkipped: false,
          },
        ],
      },
      options: this.baseChartOptions(true),
    };

    this.charts.push(new Chart(this.revenueChartCanvas.nativeElement, config));
  }

  private renderMembershipChart(): void {
    if (!this.membershipChartCanvas?.nativeElement || !this.stats) return;

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels: ['Active', 'Pending Renewals', 'Expired'],
        datasets: [
          {
            label: 'Memberships',
            data: [
              this.stats.activeMemberships,
              this.stats.pendingRenewals,
              this.stats.expiredMemberships,
            ],
            backgroundColor: ['#12b76a', '#f79009', '#f04438'],
            borderRadius: 8,
            borderSkipped: false,
          },
        ],
      },
      options: this.baseChartOptions(false),
    };

    this.charts.push(new Chart(this.membershipChartCanvas.nativeElement, config));
  }

  private baseChartOptions(currency: boolean): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: currency
          ? {
              callbacks: {
                label: (ctx) => {
                  const value = typeof ctx.parsed.y === 'number' ? ctx.parsed.y : 0;
                  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value);
                },
              },
            }
          : undefined,
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: { color: '#667085' },
          border: { display: false },
        },
        y: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: {
            color: '#667085',
            callback: currency
              ? (value) => new Intl.NumberFormat(undefined, { notation: 'compact', compactDisplay: 'short' }).format(Number(value))
              : undefined,
          },
          border: { display: false },
        },
      },
    };
  }
}
