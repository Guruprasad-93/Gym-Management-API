import { CurrencyPipe } from '@angular/common';
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
import { RevenueAnalytics } from '../../../shared/models/analytics.models';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics-revenue',
  standalone: true,
  imports: [
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './analytics-revenue.component.html',
  styleUrl: './analytics-revenue.component.css',
})
export class AnalyticsRevenueComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly analytics = inject(AnalyticsService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  @ViewChild('trendChart') trendChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('planChart') planChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('methodChart') methodChartRef?: ElementRef<HTMLCanvasElement>;

  loading = signal(true);
  exporting = signal(false);
  data = signal<RevenueAnalytics | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.analytics.getRevenue().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.data.set(res.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load revenue analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.destroyCharts();
  }

  exportPdf(): void {
    this.exporting.set(true);
    this.analytics.exportPdf('revenue').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `revenue-analytics-${Date.now()}.pdf`);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }

  exportExcel(): void {
    this.exporting.set(true);
    this.analytics.exportExcel('revenue').subscribe({
      next: (blob) => {
        this.exporting.set(false);
        this.analytics.downloadBlob(blob, `revenue-analytics-${Date.now()}.xlsx`);
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
    const rev = this.data();
    if (!rev) return;
    this.destroyCharts();

    if (this.trendChartRef?.nativeElement && rev.revenueTrend.length) {
      this.charts.push(new Chart(this.trendChartRef.nativeElement, {
        type: 'line',
        data: {
          labels: rev.revenueTrend.map((x) => x.monthLabel),
          datasets: [{
            label: 'Revenue',
            data: rev.revenueTrend.map((x) => x.value),
            borderColor: '#ff6600',
            backgroundColor: 'rgba(255, 102, 0, 0.08)',
            fill: true,
            tension: 0.35,
            pointRadius: 3,
            pointBackgroundColor: '#ff6600',
          }],
        },
        options: this.lineOptions(true),
      }));
    }

    if (this.planChartRef?.nativeElement && rev.revenueByPlan.length) {
      this.charts.push(new Chart(this.planChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: rev.revenueByPlan.map((x) => x.name),
          datasets: [{
            label: 'Revenue',
            data: rev.revenueByPlan.map((x) => x.value),
            backgroundColor: '#2e90fa',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.barOptions(true),
      }));
    }

    if (this.methodChartRef?.nativeElement && rev.revenueByPaymentMethod.length) {
      this.charts.push(new Chart(this.methodChartRef.nativeElement, {
        type: 'doughnut',
        data: {
          labels: rev.revenueByPaymentMethod.map((x) => x.name),
          datasets: [{
            data: rev.revenueByPaymentMethod.map((x) => x.value),
            backgroundColor: ['#ff6600', '#2e90fa', '#12b76a', '#f79009', '#7a5af8'],
            borderWidth: 0,
          }],
        },
        options: this.doughnutOptions(),
      }));
    }
  }

  private lineOptions(currency: boolean): ChartConfiguration<'line'>['options'] {
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
        x: { grid: { display: false }, ticks: { color: '#667085' }, border: { display: false } },
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

  private barOptions(currency: boolean): ChartConfiguration<'bar'>['options'] {
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
        x: { grid: { display: false }, ticks: { color: '#667085', maxRotation: 45 }, border: { display: false } },
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
        tooltip: {
          callbacks: {
            label: (ctx) => {
              const value = typeof ctx.parsed === 'number' ? ctx.parsed : 0;
              return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'INR' }).format(value);
            },
          },
        },
      },
    };
  }
}
