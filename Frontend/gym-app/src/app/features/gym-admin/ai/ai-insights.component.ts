import { DatePipe, NgClass } from '@angular/common';
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
import { MatTableModule } from '@angular/material/table';
import { AiService } from '../../../core/services/ai.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { AiAnalytics, AiInsight, LeadScore } from '../../../shared/models/ai.models';

Chart.register(...registerables);

@Component({
  selector: 'app-ai-insights',
  standalone: true,
  imports: [
    DatePipe,
    NgClass,
    RouterLink,
    MatIconModule,
    MatTableModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './ai-insights.component.html',
  styleUrl: './ai-insights.component.css',
})
export class AiInsightsComponent implements OnInit, OnDestroy {
  @ViewChild('leadChart') leadChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly svc = inject(AiService);
  private readonly injector = inject(Injector);

  activeTab = signal<'insights' | 'leads'>('insights');
  insights = signal<AiInsight[]>([]);
  leads = signal<LeadScore[]>([]);
  analytics = signal<AiAnalytics | null>(null);
  leadColumns = ['fullName', 'scoreCategory', 'engagementScore', 'followUpCount'];
  private chart?: Chart;

  ngOnInit(): void {
    this.svc.getAnalytics().subscribe({
      next: (r) => {
        if (r.success && r.data) this.analytics.set(r.data);
      },
    });
    this.svc.getBusinessInsights().subscribe({
      next: (r) => {
        if (r.success && r.data) this.insights.set(r.data.items);
      },
    });
    this.svc.getLeadScoring(1, 50).subscribe({
      next: (r) => {
        if (r.success && r.data) {
          this.leads.set(r.data.items);
          this.scheduleChartRender();
        }
      },
    });
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  leadInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  severityIcon(severity: string): string {
    switch (severity) {
      case 'Critical':
        return 'error';
      case 'Warning':
        return 'warning';
      default:
        return 'info';
    }
  }

  severityBadgeClass(severity: string): string {
    switch (severity) {
      case 'Critical':
        return 'badge-critical';
      case 'Warning':
        return 'badge-warn';
      default:
        return 'badge-info';
    }
  }

  scoreBadgeClass(category: string): string {
    switch (category) {
      case 'Hot':
        return 'score-hot';
      case 'Warm':
        return 'score-warm';
      default:
        return 'score-cold';
    }
  }

  private scheduleChartRender(): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(() => this.renderLeadChart());
    });
  }

  private renderLeadChart(): void {
    const el = this.leadChartRef?.nativeElement;
    const items = this.leads();
    if (!el || !items.length) return;
    this.chart?.destroy();
    const hot = items.filter((l) => l.scoreCategory === 'Hot').length;
    const warm = items.filter((l) => l.scoreCategory === 'Warm').length;
    const cold = items.filter((l) => l.scoreCategory === 'Cold').length;
    this.chart = new Chart(el, {
      type: 'doughnut',
      data: {
        labels: ['Hot', 'Warm', 'Cold'],
        datasets: [{
          data: [hot, warm, cold],
          backgroundColor: ['#f04438', '#ff6600', '#98a2b3'],
          borderWidth: 0,
        }],
      },
      options: this.doughnutOptions(),
    });
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
