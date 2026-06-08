import { DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuditService } from '../../../core/services/audit.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import {
  AUDIT_ACTIONS,
  AUDIT_ENTITIES,
  AuditDashboard,
  AuditLog,
  AuditSearchQuery,
} from '../../../shared/models/audit.models';

const ENTITY_ACCENTS = ['#2e90fa', '#7a5af8', '#12b76a'];

@Component({
  selector: 'app-audit-dashboard',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    SaasKpiCardComponent,
  ],
  templateUrl: './audit-dashboard.component.html',
  styleUrl: './audit-dashboard.component.css',
})
export class AuditDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly audit = inject(AuditService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly entities = AUDIT_ENTITIES;
  readonly actions = AUDIT_ACTIONS;
  readonly displayedColumns = ['createdDate', 'user', 'entity', 'action', 'ip', 'details'];
  dataSource = new MatTableDataSource<AuditLog>([]);

  loading = signal(true);
  exporting = signal(false);
  totalCount = signal(0);
  pageIndex = 0;
  pageSize = 20;
  dashboard: AuditDashboard | null = null;

  filterForm = this.fb.group({
    search: [''],
    fromDate: [''],
    toDate: [''],
    entityName: [''],
    actionType: [''],
    entityId: [''],
  });

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No audit logs';

    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}-${end} of ${total} logs`;
  }

  ngOnInit(): void {
    this.loadDashboard();
    this.loadLogs();
    this.filterForm.valueChanges.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.loadDashboard();
      this.loadLogs();
    });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadLogs();
    this.cdr.markForCheck();
  }

  exportPdf(): void {
    this.download('pdf');
  }

  exportExcel(): void {
    this.download('excel');
  }

  entityAccent(index: number): string {
    return ENTITY_ACCENTS[index % ENTITY_ACCENTS.length];
  }

  userInitials(name?: string): string {
    if (!name?.trim()) return 'SY';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  actionBadgeClass(action: string): string {
    const normalized = action.toLowerCase();
    if (normalized === 'create' || normalized === 'renew') return 'action-badge action-badge--create';
    if (normalized === 'update' || normalized === 'mark' || normalized === 'cancel') return 'action-badge action-badge--update';
    if (normalized === 'delete') return 'action-badge action-badge--delete';
    if (normalized === 'login' || normalized === 'logout') return 'action-badge action-badge--auth';
    if (normalized === 'checkin' || normalized === 'checkout') return 'action-badge action-badge--attendance';
    return 'action-badge action-badge--default';
  }

  detailTooltip(row: AuditLog): string {
    const parts: string[] = [];
    if (row.oldValueJson) parts.push('Old: ' + row.oldValueJson.slice(0, 200));
    if (row.newValueJson) parts.push('New: ' + row.newValueJson.slice(0, 200));
    return parts.join('\n') || 'No change payload';
  }

  private loadDashboard(): void {
    const q = this.buildQuery();
    this.audit.getDashboard(q.fromDate, q.toDate).subscribe({
      next: (res) => {
        if (res.success && res.data) this.dashboard = res.data;
      },
      error: () => this.notify.error('Failed to load audit summary'),
    });
  }

  private loadLogs(): void {
    this.loading.set(true);
    const query = this.buildQuery();
    this.audit.search(query).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.dataSource.data = res.data.items;
          this.totalCount.set(res.data.totalCount);
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load audit logs');
        this.cdr.markForCheck();
      },
    });
  }

  private buildQuery(): AuditSearchQuery {
    const v = this.filterForm.getRawValue();
    return {
      search: v.search || undefined,
      fromDate: v.fromDate ? new Date(v.fromDate).toISOString() : undefined,
      toDate: v.toDate ? new Date(v.toDate).toISOString() : undefined,
      entityName: v.entityName || undefined,
      actionType: v.actionType || undefined,
      entityId: v.entityId || undefined,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
    };
  }

  private download(format: 'pdf' | 'excel'): void {
    this.exporting.set(true);
    const query = this.buildQuery();
    const req = format === 'pdf' ? this.audit.downloadPdf(query) : this.audit.downloadExcel(query);
    req.subscribe({
      next: (blob) => {
        this.exporting.set(false);
        const ext = format === 'pdf' ? 'pdf' : 'xlsx';
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `audit-logs.${ext}`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => {
        this.exporting.set(false);
        this.notify.error('Export failed');
      },
    });
  }
}
