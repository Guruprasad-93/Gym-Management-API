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
import { AttendanceAnalytics } from '../../../shared/models/analytics.models';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics-attendance',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './analytics-attendance.component.html',
  styleUrl: './analytics-attendance.component.css',
})
export class AnalyticsAttendanceComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly analytics = inject(AnalyticsService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  @ViewChild('weeklyChart') weeklyChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('monthlyChart') monthlyChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('activeChart') activeChartRef?: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  exporting = signal(false);
  data = signal<AttendanceAnalytics | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.analytics.getAttendance().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.data.set(res.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load attendance analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  weeklyTotal(att: AttendanceAnalytics): number {
    return att.weeklyTrend.reduce((sum, d) => sum + d.count, 0);
  }

  monthlyTotal(att: AttendanceAnalytics): number {
    return att.monthlyTrend.reduce((sum, m) => sum + m.count, 0);
  }

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  exportPdf(): void {
    this.exporting.set(true);
    this.analytics.exportPdf('attendance').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `attendance-analytics-${Date.now()}.pdf`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.analytics.exportExcel('attendance').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `attendance-analytics-${Date.now()}.xlsx`);
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
    const att = this.data();
    if (!att) return;
    this.destroyCharts();

    if (this.weeklyChartRef?.nativeElement && att.weeklyTrend.length) {
      this.charts.push(new Chart(this.weeklyChartRef.nativeElement, {
        type: 'line',
        data: {
          labels: att.weeklyTrend.map((x) => x.dayLabel),
          datasets: [{
            label: 'Check-ins',
            data: att.weeklyTrend.map((x) => x.count),
            borderColor: '#ff6600',
            backgroundColor: 'rgba(255, 102, 0, 0.08)',
            fill: true,
            tension: 0.35,
            pointRadius: 3,
            pointBackgroundColor: '#ff6600',
          }],
        },
        options: this.lineOptions(),
      }));
    }

    if (this.monthlyChartRef?.nativeElement && att.monthlyTrend.length) {
      this.charts.push(new Chart(this.monthlyChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: att.monthlyTrend.map((x) => x.name),
          datasets: [{
            label: 'Check-ins',
            data: att.monthlyTrend.map((x) => x.count),
            backgroundColor: '#2e90fa',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.barOptions(),
      }));
    }

    const topMembers = att.mostActiveMembers.slice(0, 6);
    if (this.activeChartRef?.nativeElement && topMembers.length) {
      this.charts.push(new Chart(this.activeChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: topMembers.map((x) => x.memberName),
          datasets: [{
            label: 'Visits',
            data: topMembers.map((x) => x.attendanceCount),
            backgroundColor: '#12b76a',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: {
          ...this.barOptions(),
          indexAxis: 'y',
        },
      }));
    }
  }

  private lineOptions(): ChartConfiguration<'line'>['options'] {
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
}
