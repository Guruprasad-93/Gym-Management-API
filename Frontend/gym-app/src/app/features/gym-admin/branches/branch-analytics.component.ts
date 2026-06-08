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
import { BranchService } from '../../../core/services/branch.service';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { BranchAnalytics } from '../../../shared/models/branch.models';

Chart.register(...registerables);

@Component({
  selector: 'app-branch-analytics',
  standalone: true,
  imports: [CurrencyPipe, RouterLink, MatIconModule, MatProgressSpinnerModule, SaasChartCardComponent],
  templateUrl: './branch-analytics.component.html',
  styleUrl: './branch-analytics.component.css',
})
export class BranchAnalyticsComponent implements OnInit, OnDestroy {
  @ViewChild('rankChart') rankChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('monthChart') monthChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly service = inject(BranchService);
  private readonly injector = inject(Injector);

  loading = signal(true);
  analytics = signal<BranchAnalytics | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.service.getAnalytics().subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) {
          this.analytics.set(r.data);
          this.scheduleChartRender();
        }
      },
      error: () => this.loading.set(false),
    });
  }

  ngOnDestroy(): void {
    this.charts.forEach((c) => c.destroy());
    this.charts = [];
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderCharts());
    });
  }

  private renderCharts(): void {
    const data = this.analytics();
    if (!data) return;
    this.charts.forEach((c) => c.destroy());
    this.charts = [];

    if (this.rankChartRef?.nativeElement && data.rankings.length) {
      this.charts.push(
        new Chart(this.rankChartRef.nativeElement, {
          type: 'bar',
          data: {
            labels: data.rankings.map((r) => r.branchName),
            datasets: [{
              label: 'Revenue',
              data: data.rankings.map((r) => r.totalRevenue),
              backgroundColor: '#ff6600',
              borderRadius: 8,
              borderSkipped: false,
            }],
          },
          options: {
            ...this.barOptions(),
            indexAxis: 'y',
          },
        }),
      );
    }

    if (this.monthChartRef?.nativeElement && data.monthlyRevenue.length) {
      const branches = [...new Set(data.monthlyRevenue.map((m) => m.branchName))];
      const labels = [...new Set(data.monthlyRevenue.map((m) => `${m.year}-${String(m.month).padStart(2, '0')}`))];
      const colors = ['#ff6600', '#2e90fa', '#12b76a', '#7a5af8', '#f79009'];
      this.charts.push(
        new Chart(this.monthChartRef.nativeElement, {
          type: 'line',
          data: {
            labels,
            datasets: branches.map((name, i) => ({
              label: name,
              data: labels.map((lbl) => {
                const [y, m] = lbl.split('-').map(Number);
                return data.monthlyRevenue.find((r) => r.branchName === name && r.year === y && r.month === m)?.revenue ?? 0;
              }),
              borderColor: colors[i % colors.length],
              backgroundColor: `${colors[i % colors.length]}14`,
              tension: 0.35,
              pointRadius: 3,
            })),
          },
          options: this.lineOptions(),
        }),
      );
    }
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
              const value = typeof ctx.parsed.x === 'number' ? ctx.parsed.x : typeof ctx.parsed.y === 'number' ? ctx.parsed.y : 0;
              return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'INR' }).format(value);
            },
          },
        },
      },
      scales: {
        x: {
          beginAtZero: true,
          grid: { color: '#f2f4f7' },
          ticks: {
            color: '#667085',
            callback: (value) =>
              new Intl.NumberFormat(undefined, { notation: 'compact', compactDisplay: 'short' }).format(Number(value)),
          },
          border: { display: false },
        },
        y: { grid: { display: false }, ticks: { color: '#667085' }, border: { display: false } },
      },
    };
  }

  private lineOptions(): ChartConfiguration<'line'>['options'] {
    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: { usePointStyle: true, boxWidth: 8, color: '#667085' },
        },
        tooltip: {
          callbacks: {
            label: (ctx) => {
              const value = typeof ctx.parsed.y === 'number' ? ctx.parsed.y : 0;
              return `${ctx.dataset.label}: ${new Intl.NumberFormat(undefined, { style: 'currency', currency: 'INR' }).format(value)}`;
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
