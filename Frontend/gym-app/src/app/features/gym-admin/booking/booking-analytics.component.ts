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
import { BookingService } from '../../../core/services/booking.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { BookingAnalytics } from '../../../shared/models/booking.models';

Chart.register(...registerables);

@Component({
  selector: 'app-booking-analytics',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './booking-analytics.component.html',
  styleUrl: './booking-analytics.component.css',
})
export class BookingAnalyticsComponent implements OnInit, OnDestroy {
  @ViewChild('trendChart') trendRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('classChart') classRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('peakChart') peakRef?: ElementRef<HTMLCanvasElement>;

  private readonly svc = inject(BookingService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  loading = signal(true);
  analytics = signal<BookingAnalytics | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.svc.getAnalytics().subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) {
          this.analytics.set(r.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load booking analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.charts.forEach((c) => c.destroy());
    this.charts = [];
  }

  export(format: 'pdf' | 'excel'): void {
    this.svc.exportReport(format, 'bookings').subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `booking-report.${format === 'excel' ? 'xlsx' : 'pdf'}`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.notify.error('Export failed'),
    });
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderCharts());
    });
  }

  private renderCharts(): void {
    const a = this.analytics();
    if (!a) return;
    this.charts.forEach((c) => c.destroy());
    this.charts = [];

    if (this.trendRef?.nativeElement && a.bookingTrend.length) {
      this.charts.push(
        new Chart(this.trendRef.nativeElement, {
          type: 'line',
          data: {
            labels: a.bookingTrend.map((x) => x.label),
            datasets: [{
              label: 'Bookings',
              data: a.bookingTrend.map((x) => x.bookingCount),
              borderColor: '#ff6600',
              backgroundColor: 'rgba(255, 102, 0, 0.08)',
              fill: true,
              tension: 0.35,
              pointRadius: 3,
            }],
          },
          options: this.lineOptions(),
        }),
      );
    }

    if (this.classRef?.nativeElement && a.popularClasses.length) {
      this.charts.push(
        new Chart(this.classRef.nativeElement, {
          type: 'bar',
          data: {
            labels: a.popularClasses.map((x) => x.label),
            datasets: [{
              data: a.popularClasses.map((x) => x.bookingCount),
              backgroundColor: '#12b76a',
              borderRadius: 8,
              borderSkipped: false,
            }],
          },
          options: this.barOptions(),
        }),
      );
    }

    if (this.peakRef?.nativeElement && a.peakHours.length) {
      this.charts.push(
        new Chart(this.peakRef.nativeElement, {
          type: 'bar',
          data: {
            labels: a.peakHours.map((x) => x.label),
            datasets: [{
              data: a.peakHours.map((x) => x.bookingCount),
              backgroundColor: '#2e90fa',
              borderRadius: 8,
              borderSkipped: false,
            }],
          },
          options: this.barOptions(),
        }),
      );
    }
  }

  private lineOptions(): ChartConfiguration<'line'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085' }, border: { display: false } },
        y: { beginAtZero: true, grid: { color: '#f2f4f7' }, ticks: { color: '#667085' }, border: { display: false } },
      },
    };
  }

  private barOptions(): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085', maxRotation: 45 }, border: { display: false } },
        y: { beginAtZero: true, grid: { color: '#f2f4f7' }, ticks: { color: '#667085' }, border: { display: false } },
      },
    };
  }
}
