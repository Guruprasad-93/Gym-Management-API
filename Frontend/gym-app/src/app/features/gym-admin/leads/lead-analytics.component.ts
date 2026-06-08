import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Chart, registerables } from 'chart.js';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LeadService } from '../../../core/services/lead.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { LeadAnalytics } from '../../../shared/models/lead.models';

Chart.register(...registerables);

@Component({
  selector: 'app-lead-analytics',
  standalone: true,
  imports: [
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
    StatCardComponent,
  ],
  template: `
    <app-page-header title="Lead Analytics" subtitle="Conversion funnel and pipeline metrics">
      <button mat-stroked-button type="button" routerLink="/gym-admin/leads">Back to leads</button>
      <button mat-stroked-button type="button" (click)="exportPdf()"><mat-icon>picture_as_pdf</mat-icon> PDF</button>
      <button mat-stroked-button type="button" (click)="exportExcel()"><mat-icon>table_chart</mat-icon> Excel</button>
    </app-page-header>

    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else if (analytics()) {
      <div class="stats-grid">
        <app-stat-card label="Total Leads" [value]="analytics()!.dashboard.totalLeads" icon="groups" color="#1565c0" />
        <app-stat-card label="New Today" [value]="analytics()!.dashboard.newLeadsToday" icon="today" color="#0277bd" />
        <app-stat-card label="Conversion Rate" [value]="analytics()!.dashboard.conversionRate + '%'" icon="trending_up" color="#2e7d32" />
        <app-stat-card label="Trial Conversion" [value]="analytics()!.dashboard.trialConversionRate + '%'" icon="fitness_center" color="#6a1b9a" />
        <app-stat-card label="Lost Leads" [value]="analytics()!.dashboard.lostLeads" icon="cancel" color="#c62828" />
        <app-stat-card label="Pending Follow-ups" [value]="analytics()!.dashboard.pendingFollowUps" icon="schedule" color="#e65100" />
      </div>

      <div class="links">
        <a routerLink="/gym-admin/leads/followups">Pending follow-ups</a>
        <a routerLink="/gym-admin/leads/trials">Today's trials</a>
      </div>

      <div class="charts-grid">
        <section class="chart-card">
          <h3>Leads by Source</h3>
          <canvas #sourceChart></canvas>
        </section>
        <section class="chart-card">
          <h3>Leads by Status</h3>
          <canvas #statusChart></canvas>
        </section>
        <section class="chart-card wide">
          <h3>Monthly Conversions</h3>
          <canvas #conversionChart></canvas>
        </section>
        <section class="chart-card wide">
          <h3>Trainer Performance</h3>
          <canvas #trainerChart></canvas>
        </section>
      </div>
    }
  `,
  styles: [
    `
      .stats-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 1rem; margin-bottom: 1rem; }
      .links { display: flex; gap: 1rem; margin-bottom: 1rem; }
      .charts-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }
      .chart-card { background: #fff; border-radius: 8px; padding: 1rem; }
      .chart-card.wide { grid-column: span 2; }
      .center-spinner { margin: 2rem auto; display: block; }
      @media (max-width: 900px) { .charts-grid { grid-template-columns: 1fr; } .chart-card.wide { grid-column: span 1; } }
    `,
  ],
})
export class LeadAnalyticsComponent implements OnInit, AfterViewInit {
  @ViewChild('sourceChart') sourceChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('statusChart') statusChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('conversionChart') conversionChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('trainerChart') trainerChartRef!: ElementRef<HTMLCanvasElement>;

  private readonly leadService = inject(LeadService);
  private readonly notify = inject(NotificationService);
  analytics = signal<LeadAnalytics | null>(null);
  loading = signal(true);
  private charts: Chart[] = [];

  ngOnInit(): void {
    this.leadService.getAnalytics().subscribe({
      next: (res) => {
        this.analytics.set(res.data!);
        this.loading.set(false);
        setTimeout(() => this.renderCharts(), 0);
      },
      error: () => {
        this.notify.error('Failed to load analytics');
        this.loading.set(false);
      },
    });
  }

  ngAfterViewInit(): void {}

  exportPdf(): void {
    this.leadService.exportPdf('conversion').subscribe({
      next: (blob) => this.download(blob, 'leads-conversion.pdf'),
      error: () => this.notify.error('Export failed'),
    });
  }

  exportExcel(): void {
    this.leadService.exportExcel('conversion').subscribe({
      next: (blob) => this.download(blob, 'leads-conversion.xlsx'),
      error: () => this.notify.error('Export failed'),
    });
  }

  private download(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private renderCharts(): void {
    const data = this.analytics();
    if (!data) return;
    this.charts.forEach((c) => c.destroy());
    this.charts = [];

    this.charts.push(new Chart(this.sourceChartRef.nativeElement, {
      type: 'doughnut',
      data: {
        labels: data.leadsBySource.map((x) => x.name),
        datasets: [{ data: data.leadsBySource.map((x) => x.count), backgroundColor: ['#1565c0', '#0277bd', '#00838f', '#6a1b9a', '#4527a0', '#e65100', '#2e7d32', '#c62828'] }],
      },
    }));

    this.charts.push(new Chart(this.statusChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: data.leadsByStatus.map((x) => x.name),
        datasets: [{ label: 'Leads', data: data.leadsByStatus.map((x) => x.count), backgroundColor: '#1565c0' }],
      },
      options: { scales: { y: { beginAtZero: true } } },
    }));

    const months = [...data.monthlyConversions].reverse();
    this.charts.push(new Chart(this.conversionChartRef.nativeElement, {
      type: 'line',
      data: {
        labels: months.map((m) => m.monthLabel),
        datasets: [
          { label: 'New Leads', data: months.map((m) => m.newLeads), borderColor: '#1565c0', tension: 0.3 },
          { label: 'Conversions', data: months.map((m) => m.conversions), borderColor: '#2e7d32', tension: 0.3 },
        ],
      },
    }));

    this.charts.push(new Chart(this.trainerChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels: data.trainerPerformance.map((t) => t.trainerName),
        datasets: [
          { label: 'Total', data: data.trainerPerformance.map((t) => t.totalLeads), backgroundColor: '#90caf9' },
          { label: 'Converted', data: data.trainerPerformance.map((t) => t.convertedLeads), backgroundColor: '#2e7d32' },
        ],
      },
      options: { scales: { y: { beginAtZero: true } } },
    }));
  }
}
