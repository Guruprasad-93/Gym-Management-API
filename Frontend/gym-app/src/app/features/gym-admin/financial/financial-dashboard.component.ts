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

import { FinancialService } from '../../../core/services/financial.service';

import { NotificationService } from '../../../core/services/notification.service';

import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';

import { FinancialDashboard } from '../../../shared/models/financial.models';



Chart.register(...registerables);



@Component({

  selector: 'app-financial-dashboard',

  standalone: true,

  imports: [

    RouterLink,

    MatIconModule,

    MatProgressSpinnerModule,

    SaasKpiCardComponent,

    SaasChartCardComponent,

  ],

  templateUrl: './financial-dashboard.component.html',

  styleUrl: './financial-dashboard.component.css',

})

export class FinancialDashboardComponent implements OnInit, OnDestroy {

  @ViewChild('profitChart') profitChartRef?: ElementRef<HTMLCanvasElement>;

  @ViewChild('expenseChart') expenseChartRef?: ElementRef<HTMLCanvasElement>;

  @ViewChild('payrollChart') payrollChartRef?: ElementRef<HTMLCanvasElement>;

  @ViewChild('commissionChart') commissionChartRef?: ElementRef<HTMLCanvasElement>;



  private readonly financialService = inject(FinancialService);

  private readonly notify = inject(NotificationService);

  private readonly injector = inject(Injector);



  dash = signal<FinancialDashboard | null>(null);

  loading = signal(true);

  private charts: Chart[] = [];



  ngOnInit(): void {

    this.financialService.getDashboard().subscribe({

      next: (r) => {

        this.dash.set(r.data!);

        this.loading.set(false);

        this.scheduleChartRender();

      },

      error: () => {

        this.notify.error('Failed to load dashboard');

        this.loading.set(false);

      },

    });

  }



  ngOnDestroy(): void {

    this.destroyCharts();

  }



  exportPdf(): void {

    this.financialService.exportPdf().subscribe({

      next: (b) => this.download(b, 'profit-loss.pdf'),

      error: () => this.notify.error('Export failed'),

    });

  }



  exportExcel(): void {

    this.financialService.exportExcel().subscribe({

      next: (b) => this.download(b, 'profit-loss.xlsx'),

      error: () => this.notify.error('Export failed'),

    });

  }



  private download(blob: Blob, name: string): void {

    const a = document.createElement('a');

    a.href = URL.createObjectURL(blob);

    a.download = name;

    a.click();

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

    const d = this.dash();

    if (!d) return;

    this.destroyCharts();



    if (this.profitChartRef?.nativeElement && d.monthlyProfitTrend.length) {

      const months = d.monthlyProfitTrend.map((m) => m.monthLabel);

      this.charts.push(

        new Chart(this.profitChartRef.nativeElement, {

          type: 'line',

          data: {

            labels: months,

            datasets: [

              {

                label: 'Revenue',

                data: d.monthlyProfitTrend.map((m) => m.revenue),

                borderColor: '#12b76a',

                backgroundColor: 'rgba(18, 183, 106, 0.08)',

                tension: 0.35,

                pointRadius: 3,

              },

              {

                label: 'Expenses',

                data: d.monthlyProfitTrend.map((m) => m.expenses),

                borderColor: '#f04438',

                backgroundColor: 'rgba(240, 68, 56, 0.08)',

                tension: 0.35,

                pointRadius: 3,

              },

              {

                label: 'Profit',

                data: d.monthlyProfitTrend.map((m) => m.profit),

                borderColor: '#ff6600',

                backgroundColor: 'rgba(255, 102, 0, 0.08)',

                fill: true,

                tension: 0.35,

                pointRadius: 3,

              },

            ],

          },

          options: this.lineOptions(),

        }),

      );

    }



    if (this.expenseChartRef?.nativeElement && d.expenseBreakdown.length) {

      this.charts.push(

        new Chart(this.expenseChartRef.nativeElement, {

          type: 'doughnut',

          data: {

            labels: d.expenseBreakdown.map((x) => x.name),

            datasets: [{

              data: d.expenseBreakdown.map((x) => x.amount),

              backgroundColor: ['#ff6600', '#2e90fa', '#12b76a', '#f79009', '#7a5af8', '#f04438'],

              borderWidth: 0,

            }],

          },

          options: this.doughnutOptions(),

        }),

      );

    }



    if (this.payrollChartRef?.nativeElement && d.payrollCostTrend.length) {

      this.charts.push(

        new Chart(this.payrollChartRef.nativeElement, {

          type: 'bar',

          data: {

            labels: d.payrollCostTrend.map((x) => x.monthLabel),

            datasets: [{

              label: 'Payroll',

              data: d.payrollCostTrend.map((x) => x.value),

              backgroundColor: '#2e90fa',

              borderRadius: 8,

              borderSkipped: false,

            }],

          },

          options: this.barOptions(),

        }),

      );

    }



    if (this.commissionChartRef?.nativeElement && d.commissionTrend.length) {

      this.charts.push(

        new Chart(this.commissionChartRef.nativeElement, {

          type: 'bar',

          data: {

            labels: d.commissionTrend.map((x) => x.monthLabel),

            datasets: [{

              label: 'Commissions',

              data: d.commissionTrend.map((x) => x.value),

              backgroundColor: '#7a5af8',

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

