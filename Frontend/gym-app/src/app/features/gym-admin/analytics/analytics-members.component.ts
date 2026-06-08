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
import { MembershipAnalytics } from '../../../shared/models/analytics.models';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics-members',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './analytics-members.component.html',
  styleUrl: './analytics-members.component.css',
})
export class AnalyticsMembersComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly analytics = inject(AnalyticsService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  @ViewChild('growthChart') growthChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('planChart') planChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('statusChart') statusChartRef?: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  exporting = signal(false);
  data = signal<MembershipAnalytics | null>(null);
  private charts: Chart[] = [];

  get activeRate(): string {
    const mem = this.data();
    if (!mem?.activeMembers) return '';
    const total = mem.activeMembers + mem.expiredMembers;
    if (!total) return '';
    return `${Math.round((mem.activeMembers / total) * 100)}% active`;
  }

  ngOnInit(): void {
    this.analytics.getMembers().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.data.set(res.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load member analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  planShare(count: number, activeTotal: number): string {
    if (!activeTotal) return '—';
    return `${Math.round((count / activeTotal) * 100)}%`;
  }

  exportPdf(): void {
    this.exporting.set(true);
    this.analytics.exportPdf('members').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `member-analytics-${Date.now()}.pdf`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.analytics.exportExcel('members').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `member-analytics-${Date.now()}.xlsx`);
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
    const mem = this.data();
    if (!mem) return;
    this.destroyCharts();

    if (this.growthChartRef?.nativeElement && mem.growthTrend.length) {
      this.charts.push(new Chart(this.growthChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: mem.growthTrend.map((x) => x.monthLabel),
          datasets: [{
            label: 'New Members',
            data: mem.growthTrend.map((x) => x.newMembers),
            backgroundColor: '#ff6600',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.barOptions(),
      }));
    }

    if (this.planChartRef?.nativeElement && mem.planDistribution.length) {
      this.charts.push(new Chart(this.planChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: mem.planDistribution.map((x) => x.name),
          datasets: [{
            data: mem.planDistribution.map((x) => x.count),
            backgroundColor: ['#ff6600', '#2e90fa', '#12b76a', '#f79009', '#7a5af8'],
            borderWidth: 0,
          }],
        },
        options: this.doughnutOptions(),
      }));
    }

    if (this.statusChartRef?.nativeElement) {
      this.charts.push(new Chart(this.statusChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: ['Active', 'Expiring (7d)', 'Expired'],
          datasets: [{
            data: [mem.activeMembers, mem.expiringIn7Days, mem.expiredMembers],
            backgroundColor: ['#12b76a', '#f79009', '#f04438'],
            borderWidth: 0,
          }],
        },
        options: this.doughnutOptions(),
      }));
    }
  }

  private barOptions(): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085' }, border: { display: false } },
        y: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: { color: '#667085', stepSize: 1 },
          border: { display: false },
        },
      },
    };
  }

  private doughnutOptions(): ChartConfiguration<'doughnut'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '65%',
      plugins: {
        legend: {
          position: 'right',
          labels: { usePointStyle: true, boxWidth: 8, padding: 10, color: '#667085' },
        },
      },
    };
  }
}
