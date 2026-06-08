import { CurrencyPipe, DatePipe } from '@angular/common';
import {
  Component,
  OnInit,
  OnDestroy,
  ElementRef,
  ViewChild,
  inject,
  signal,
  Injector,
  runInInjectionContext,
  afterNextRender,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { AnalyticsDashboard } from '../../../shared/models/analytics.models';

Chart.register(...registerables);

interface QuickAction {
  label: string;
  description: string;
  icon: string;
  route: string;
  color: string;
}

@Component({
  selector: 'app-gym-admin-dashboard',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './gym-admin-dashboard.component.html',
  styleUrl: './gym-admin-dashboard.component.css',
})
export class GymAdminDashboardComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly analytics = inject(AnalyticsService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  @ViewChild('revenueChart') revenueChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('growthChart') growthChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('attendanceChart') attendanceChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('planChart') planChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('paymentChart') paymentChartRef?: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  exporting = signal(false);
  dashboard = signal<AnalyticsDashboard | null>(null);
  private charts: Chart[] = [];

  readonly quickActions: QuickAction[] = [
    { label: 'Revenue Analytics', description: 'Track income and payment trends', icon: 'payments', route: '/gym-admin/analytics/revenue', color: '#ff6600' },
    { label: 'Member Analytics', description: 'Growth, plans, and retention', icon: 'groups', route: '/gym-admin/analytics/members', color: '#2e90fa' },
    { label: 'Attendance Analytics', description: 'Check-ins and member activity', icon: 'how_to_reg', route: '/gym-admin/analytics/attendance', color: '#12b76a' },
    { label: 'Trainer Analytics', description: 'Trainer performance overview', icon: 'sports', route: '/gym-admin/analytics/trainers', color: '#7a5af8' },
  ];

  get activeMemberRate(): string {
    const dash = this.dashboard();
    if (!dash?.overview.totalMembers) return '';
    const pct = Math.round((dash.overview.activeMembers / dash.overview.totalMembers) * 100);
    return `${pct}% active`;
  }

  ngOnInit(): void {
    if (!this.auth.hasPermission(Permissions.ViewAnalytics)) {
      this.loading.set(false);
      return;
    }
    this.analytics.getDashboard().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.dashboard.set(res.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load dashboard');
      },
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  workoutCompletionHint(dash: AnalyticsDashboard): string {
    return `${dash.workouts.completionPercentage}% complete`;
  }

  dietComplianceHint(dash: AnalyticsDashboard): string {
    return `${dash.diets.compliancePercentage}% compliance`;
  }

  exportPdf(): void {
    this.exporting.set(true);
    this.analytics.exportPdf('dashboard').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `business-dashboard-${Date.now()}.pdf`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.analytics.exportExcel('dashboard').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `business-dashboard-${Date.now()}.xlsx`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderCharts());
    });
  }

  private destroyCharts(): void {
    this.charts.forEach((c) => c.destroy());
    this.charts = [];
  }

  private renderCharts(): void {
    const d = this.dashboard();
    if (!d) return;
    this.destroyCharts();

    if (this.revenueChartRef?.nativeElement && d.revenue.revenueTrend.length) {
      this.charts.push(new Chart(this.revenueChartRef.nativeElement, {
        type: 'line',
        data: {
          labels: d.revenue.revenueTrend.map((x) => x.monthLabel),
          datasets: [{
            label: 'Revenue',
            data: d.revenue.revenueTrend.map((x) => x.value),
            borderColor: '#ff6600',
            backgroundColor: 'rgba(255, 102, 0, 0.08)',
            fill: true,
            tension: 0.35,
            pointRadius: 3,
            pointBackgroundColor: '#ff6600',
          }],
        },
        options: this.lineChartOptions(true),
      }));
    }

    if (this.growthChartRef?.nativeElement && d.membership.growthTrend.length) {
      this.charts.push(new Chart(this.growthChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: d.membership.growthTrend.map((x) => x.monthLabel),
          datasets: [{
            label: 'New Members',
            data: d.membership.growthTrend.map((x) => x.newMembers),
            backgroundColor: '#2e90fa',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.barChartOptions(),
      }));
    }

    if (this.attendanceChartRef?.nativeElement && d.attendance.weeklyTrend.length) {
      this.charts.push(new Chart(this.attendanceChartRef.nativeElement, {
        type: 'line',
        data: {
          labels: d.attendance.weeklyTrend.map((x) => x.dayLabel),
          datasets: [{
            label: 'Check-ins',
            data: d.attendance.weeklyTrend.map((x) => x.count),
            borderColor: '#12b76a',
            backgroundColor: 'rgba(18, 183, 106, 0.08)',
            fill: true,
            tension: 0.35,
            pointRadius: 3,
            pointBackgroundColor: '#12b76a',
          }],
        },
        options: this.lineChartOptions(false),
      }));
    }

    if (this.planChartRef?.nativeElement && d.membership.planDistribution.length) {
      this.charts.push(new Chart(this.planChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: d.membership.planDistribution.map((x) => x.name),
          datasets: [{
            data: d.membership.planDistribution.map((x) => x.count),
            backgroundColor: ['#ff6600', '#2e90fa', '#12b76a', '#f79009', '#7a5af8'],
            borderWidth: 0,
          }],
        },
        options: this.doughnutOptions(),
      }));
    }

    if (this.paymentChartRef?.nativeElement && d.revenue.revenueByPaymentMethod.length) {
      this.charts.push(new Chart(this.paymentChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: d.revenue.revenueByPaymentMethod.map((x) => x.name),
          datasets: [{
            data: d.revenue.revenueByPaymentMethod.map((x) => x.value),
            backgroundColor: ['#ff6600', '#2e90fa', '#12b76a', '#f79009', '#7a5af8'],
            borderWidth: 0,
          }],
        },
        options: this.doughnutOptions(),
      }));
    }
  }

  private lineChartOptions(currency: boolean): ChartConfiguration<'line'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: currency ? {
          callbacks: {
            label: (ctx) => {
              const value = typeof ctx.parsed.y === 'number' ? ctx.parsed.y : 0;
              return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'INR' }).format(value);
            },
          },
        } : undefined,
      },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085', font: { size: 10 }, maxRotation: 0 }, border: { display: false } },
        y: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: {
            color: '#667085',
            font: { size: 10 },
            maxTicksLimit: 5,
            callback: currency
              ? (value) => new Intl.NumberFormat(undefined, { notation: 'compact', compactDisplay: 'short' }).format(Number(value))
              : undefined,
          },
          border: { display: false },
        },
      },
    };
  }

  private barChartOptions(): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085', font: { size: 10 }, maxRotation: 0 }, border: { display: false } },
        y: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: { color: '#667085', font: { size: 10 }, maxTicksLimit: 5 },
          border: { display: false },
        },
      },
    };
  }

  private doughnutOptions(): ChartConfiguration<'doughnut'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '62%',
      plugins: {
        legend: {
          position: 'bottom',
          labels: { usePointStyle: true, boxWidth: 6, padding: 6, color: '#667085', font: { size: 10 } },
        },
      },
    };
  }
}
