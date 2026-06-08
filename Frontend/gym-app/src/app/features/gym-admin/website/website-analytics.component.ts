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
import { WebsiteService } from '../../../core/services/website.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { WebsiteAnalyticsOverview } from '../../../shared/models/website.models';

Chart.register(...registerables);

@Component({
  selector: 'app-website-analytics',
  standalone: true,
  imports: [
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './website-analytics.component.html',
  styleUrl: './website-analytics.component.css',
})
export class WebsiteAnalyticsComponent implements OnInit, OnDestroy {
  @ViewChild('dailyChart') dailyChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly service = inject(WebsiteService);
  private readonly notify = inject(NotificationService);
  private readonly injector = inject(Injector);

  loading = signal(true);
  analytics = signal<WebsiteAnalyticsOverview | null>(null);
  private chart?: Chart;

  ngOnInit(): void {
    this.service.getAnalytics(30).subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) {
          this.analytics.set(r.data);
          this.scheduleChartRender();
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load analytics');
      },
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderChart());
    });
  }

  private renderChart(): void {
    const data = this.analytics()?.dailyLeads ?? [];
    if (!this.dailyChartRef?.nativeElement || !data.length) return;
    this.chart?.destroy();
    this.chart = new Chart(this.dailyChartRef.nativeElement, {
      type: 'line',
      data: {
        labels: data.map((d) => d.leadDate),
        datasets: [{
          label: 'Daily Leads',
          data: data.map((d) => d.leadCount),
          borderColor: '#ff6600',
          backgroundColor: 'rgba(255, 102, 0, 0.08)',
          fill: true,
          tension: 0.35,
          pointRadius: 3,
        }],
      },
      options: this.lineOptions(),
    });
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
}
