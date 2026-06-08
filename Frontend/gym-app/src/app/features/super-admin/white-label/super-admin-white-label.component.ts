import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Chart } from 'chart.js/auto';
import { WhiteLabelService } from '../../../core/services/white-label.service';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import {
  WhiteLabelCustomerSummary,
  WhiteLabelPlatformDashboard,
} from '../../../shared/models/white-label.models';

@Component({
  selector: 'app-super-admin-white-label',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatProgressSpinnerModule,
    SaasKpiCardComponent,
    SaasChartCardComponent,
  ],
  templateUrl: './super-admin-white-label.component.html',
  styleUrl: './super-admin-white-label.component.css',
})
export class SuperAdminWhiteLabelComponent implements OnInit, AfterViewInit {
  @ViewChild('adoptionChart') adoptionChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private readonly svc = inject(WhiteLabelService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  private chart?: Chart;

  loading = signal(true);
  dashboard: WhiteLabelPlatformDashboard | null = null;

  displayedColumns = ['brandName', 'subDomain', 'customDomain', 'subscriptionStatus', 'currentPeriodEnd'];
  dataSource = new MatTableDataSource<WhiteLabelCustomerSummary>([]);
  searchControl = this.fb.nonNullable.control('');
  statusControl = this.fb.nonNullable.control('all');
  statusOptions: string[] = [];

  get pageSummary(): string {
    const total = this.dataSource.filteredData.length;
    if (!total || !this.paginator) return total ? `${total} customers` : 'No customers';

    const start = this.paginator.pageIndex * this.paginator.pageSize + 1;
    const end = Math.min((this.paginator.pageIndex + 1) * this.paginator.pageSize, total);
    return `Showing ${start}-${end} of ${total} customers`;
  }

  ngOnInit(): void {
    this.dataSource.filterPredicate = (data, filter) => {
      const parsed = JSON.parse(filter) as { text: string; status: string };
      const haystack = `${data.brandName} ${data.subDomain ?? ''} ${data.customDomain ?? ''} ${data.subscriptionStatus ?? ''}`.toLowerCase();
      const matchesText = !parsed.text || haystack.includes(parsed.text);
      const status = (data.subscriptionStatus ?? 'N/A').toLowerCase();
      const matchesStatus = parsed.status === 'all' || status === parsed.status.toLowerCase();
      return matchesText && matchesStatus;
    };

    this.searchControl.valueChanges.subscribe(() => this.applyFilter());
    this.statusControl.valueChanges.subscribe(() => this.applyFilter());

    this.svc.getPlatformDashboard().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.dashboard = res.data;
          this.dataSource.data = res.data.premiumCustomers ?? [];
          this.statusOptions = [
            ...new Set(
              res.data.premiumCustomers
                .map((c) => c.subscriptionStatus ?? 'N/A')
                .filter(Boolean),
            ),
          ].sort();
          setTimeout(() => {
            this.dataSource.paginator = this.paginator;
            this.dataSource.sort = this.sort;
            this.paginator?.page.subscribe(() => this.cdr.markForCheck());
            this.renderChart();
          });
        }
        this.cdr.markForCheck();
      },
      error: () => this.loading.set(false),
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.renderChart(), 0);
  }

  brandInitials(name: string): string {
    if (!name?.trim()) return 'WL';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  statusBadgeClass(status?: string | null): string {
    const normalized = (status ?? 'N/A').toLowerCase();
    if (normalized === 'active') return 'status-badge status-badge--active';
    if (normalized === 'trial') return 'status-badge status-badge--trial';
    if (normalized === 'expired' || normalized === 'cancelled' || normalized === 'canceled') {
      return 'status-badge status-badge--expired';
    }
    return 'status-badge status-badge--muted';
  }

  onPageChange(): void {
    this.cdr.markForCheck();
  }

  private applyFilter(): void {
    this.dataSource.filter = JSON.stringify({
      text: this.searchControl.value.trim().toLowerCase(),
      status: this.statusControl.value,
    });
    this.paginator?.firstPage();
    this.cdr.markForCheck();
  }

  private renderChart(): void {
    const el = this.adoptionChartRef?.nativeElement;
    if (!el || !this.dashboard?.adoptionTrend.length) return;

    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'line',
      data: {
        labels: this.dashboard.adoptionTrend.map((p) => p.adoptionDate),
        datasets: [
          {
            label: 'Enabled gyms',
            data: this.dashboard.adoptionTrend.map((p) => p.enabledCount),
            borderColor: '#ff6600',
            backgroundColor: 'rgba(255, 102, 0, 0.12)',
            fill: true,
            tension: 0.35,
            pointRadius: 3,
            pointHoverRadius: 5,
            pointBackgroundColor: '#ff6600',
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#667085', font: { size: 11 } },
          },
          y: {
            beginAtZero: true,
            grid: { color: '#f2f4f7' },
            ticks: { color: '#667085', font: { size: 11 }, precision: 0 },
          },
        },
      },
    });
  }
}
