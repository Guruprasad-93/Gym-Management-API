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
import { forkJoin } from 'rxjs';
import { PaymentService } from '../../../core/services/payment.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { MonthlyRevenue, RevenueDashboard } from '../../../shared/models/membership-payment.models';

Chart.register(...registerables);

@Component({
  selector: 'app-revenue-dashboard',
  standalone: true,
  imports: [
    CurrencyPipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './revenue-dashboard.component.html',
  styleUrl: './revenue-dashboard.component.css',
})
export class RevenueDashboardComponent implements OnInit, OnDestroy {
  @ViewChild('trendChart') trendChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly svc = inject(PaymentService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  loading = signal(true);
  dashboard = signal<RevenueDashboard | null>(null);
  monthly = signal<MonthlyRevenue[]>([]);
  private charts: Chart[] = [];

  ngOnInit(): void {
    forkJoin({
      dashboard: this.svc.getRevenueDashboard(),
      monthly: this.svc.getMonthlyRevenue(12),
    }).subscribe({
      next: ({ dashboard, monthly }) => {
        this.loading.set(false);
        if (dashboard.success && dashboard.data) this.dashboard.set(dashboard.data);
        if (monthly.success && monthly.data) {
          this.monthly.set(monthly.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load revenue');
      },
    });
  }

  ngOnDestroy(): void {
    this.charts.forEach((c) => c.destroy());
    this.charts = [];
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderChart());
    });
  }

  private renderChart(): void {
    const data = this.monthly();
    if (!this.trendChartRef?.nativeElement || !data.length) return;
    this.charts.forEach((c) => c.destroy());
    this.charts = [];
    this.charts.push(
      new Chart(this.trendChartRef.nativeElement, {
        type: 'bar',
        data: {
          labels: data.map((x) => x.monthLabel),
          datasets: [{
            label: 'Revenue',
            data: data.map((x) => x.revenue),
            backgroundColor: '#ff6600',
            borderRadius: 8,
            borderSkipped: false,
          }],
        },
        options: this.barOptions(),
      }),
    );
  }

  private barOptions(): ChartConfiguration<'bar'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          callbacks: {
            label: (ctx) => {
              const value = typeof ctx.parsed.y === 'number' ? ctx.parsed.y : 0;
              return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'INR' }).format(value);
            },
          },
        },
      },
      scales: {
        x: { grid: { display: false }, ticks: { color: '#667085' }, border: { display: false } },
        y: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: {
            color: '#667085',
            callback: (value) =>
              new Intl.NumberFormat(undefined, { notation: 'compact', compactDisplay: 'short' }).format(Number(value)),
          },
          border: { display: false },
        },
      },
    };
  }
}
