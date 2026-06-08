import { DatePipe } from '@angular/common';
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
import { AiService } from '../../../core/services/ai.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { AiDashboard } from '../../../shared/models/ai.models';

Chart.register(...registerables);

@Component({
  selector: 'app-ai-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './ai-dashboard.component.html',
  styleUrl: './ai-dashboard.component.css',
})
export class AiDashboardComponent implements OnInit, OnDestroy {
  @ViewChild('churnChart') churnChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('renewalChart') renewalChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly svc = inject(AiService);
  private readonly injector = inject(Injector);

  loading = signal(true);
  dashboard = signal<AiDashboard | null>(null);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.svc.getDashboard().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.dashboard.set(res.data);
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

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderCharts());
    });
  }

  private renderCharts(): void {
    const d = this.dashboard();
    if (!d) return;
    this.charts.forEach((c) => c.destroy());
    this.charts = [];

    if (this.churnChartRef?.nativeElement && d.churnRiskDistribution.length) {
      this.charts.push(
        new Chart(this.churnChartRef.nativeElement, {
          type: 'doughnut',
          data: {
            labels: d.churnRiskDistribution.map((x) => x.label),
            datasets: [{
              data: d.churnRiskDistribution.map((x) => x.count),
              backgroundColor: ['#f04438', '#f79009', '#12b76a'],
              borderWidth: 0,
            }],
          },
          options: this.doughnutOptions(),
        }),
      );
    }

    if (this.renewalChartRef?.nativeElement && d.renewalProbabilityDistribution.length) {
      this.charts.push(
        new Chart(this.renewalChartRef.nativeElement, {
          type: 'bar',
          data: {
            labels: d.renewalProbabilityDistribution.map((x) => x.label),
            datasets: [{
              label: 'Members',
              data: d.renewalProbabilityDistribution.map((x) => x.count),
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

  private barOptions(): ChartConfiguration<'bar'>['options'] {
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
