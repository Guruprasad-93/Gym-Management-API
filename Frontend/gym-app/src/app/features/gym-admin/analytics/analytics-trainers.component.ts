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
import { TrainerAnalytics } from '../../../shared/models/analytics.models';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics-trainers',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './analytics-trainers.component.html',
  styleUrl: './analytics-trainers.component.css',
})
export class AnalyticsTrainersComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly analytics = inject(AnalyticsService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  @ViewChild('assignedChart') assignedChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('activityChart') activityChartRef?: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  exporting = signal(false);
  data = signal<TrainerAnalytics | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.analytics.getTrainers().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.data.set(res.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load trainer analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  avgMembersPerTrainer(tr: TrainerAnalytics): number | string {
    if (!tr.activeTrainers) return '—';
    return Math.round((tr.assignedMembers / tr.activeTrainers) * 10) / 10;
  }

  todayAttendanceTotal(tr: TrainerAnalytics): number {
    return tr.performance.reduce((sum, t) => sum + t.todayAttendance, 0);
  }

  trainerInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  exportPdf(): void {
    this.exporting.set(true);
    this.analytics.exportPdf('trainers').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `trainer-analytics-${Date.now()}.pdf`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.analytics.exportExcel('trainers').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `trainer-analytics-${Date.now()}.xlsx`);
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
    const tr = this.data();
    if (!tr?.performance.length) return;
    this.destroyCharts();

    const labels = tr.performance.map((x) => x.trainerName);

    if (this.assignedChartRef?.nativeElement) {
      this.charts.push(new Chart(this.assignedChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels,
          datasets: [{
            label: 'Assigned Members',
            data: tr.performance.map((x) => x.assignedMembers),
            backgroundColor: '#ff6600',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.horizontalBarOptions(),
      }));
    }

    if (this.activityChartRef?.nativeElement) {
      this.charts.push(new Chart(this.activityChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels,
          datasets: [{
            label: "Today's Check-ins",
            data: tr.performance.map((x) => x.todayAttendance),
            backgroundColor: '#12b76a',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.horizontalBarOptions(),
      }));
    }
  }

  private horizontalBarOptions(): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      indexAxis: 'y',
      plugins: { legend: { display: false } },
      scales: {
        x: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: { color: '#667085', stepSize: 1 },
          border: { display: false },
        },
        y: {
          grid: { display: false },
          ticks: { color: '#667085' },
          border: { display: false },
        },
      },
    };
  }
}
