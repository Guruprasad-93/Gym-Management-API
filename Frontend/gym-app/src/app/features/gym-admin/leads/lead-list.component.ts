import { DatePipe, NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { LeadService } from '../../../core/services/lead.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import {
  KANBAN_STATUSES,
  LEAD_SOURCES,
  LEAD_STATUS_LABELS,
  Lead,
} from '../../../shared/models/lead.models';

@Component({
  selector: 'app-lead-list',
  standalone: true,
  imports: [
    DatePipe,
    NgClass,
    ReactiveFormsModule,
    RouterModule,
    DragDropModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './lead-list.component.html',
  styleUrl: './lead-list.component.css',
})
export class LeadListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly leadService = inject(LeadService);
  private readonly notify = inject(NotificationService);
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;

  readonly statuses = KANBAN_STATUSES;
  readonly sources = LEAD_SOURCES;
  readonly displayedColumns = ['fullName', 'mobileNumber', 'leadSource', 'status', 'trainer', 'createdDate', 'actions'];
  readonly kanbanDropIds = KANBAN_STATUSES.map((_, i) => `kanban-${i}`);

  searchControl = this.fb.nonNullable.control('');
  statusControl = this.fb.nonNullable.control('');
  sourceControl = this.fb.nonNullable.control('');

  dataSource = new MatTableDataSource<Lead>([]);
  loading = signal(true);
  totalCount = signal(0);
  viewMode = signal<'list' | 'kanban'>('list');
  pageIndex = 0;
  pageSize = 10;

  kanbanColumns = KANBAN_STATUSES.map((status, i) => ({
    status,
    dropId: `kanban-${i}`,
    leads: [] as Lead[],
  }));

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No leads';
    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}–${end} of ${total} leads`;
  }

  ngOnInit(): void {
    this.load();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
    this.statusControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
    this.sourceControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
  }

  statusLabel(status: string): string {
    return LEAD_STATUS_LABELS[status] ?? status;
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'New':
        return 'status-badge--new';
      case 'Contacted':
        return 'status-badge--contact';
      case 'TrialScheduled':
      case 'TrialCompleted':
        return 'status-badge--trial';
      case 'FollowUpPending':
        return 'status-badge--followup';
      case 'Converted':
        return 'status-badge--converted';
      case 'Lost':
        return 'status-badge--lost';
      default:
        return 'status-badge--muted';
    }
  }

  leadInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.leadService
      .getPaged({
        search: this.searchControl.value || undefined,
        status: this.statusControl.value || undefined,
        leadSource: this.sourceControl.value || undefined,
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (res) => {
          const items = res.data?.items ?? [];
          this.dataSource.data = items;
          this.totalCount.set(res.data?.totalCount ?? 0);
          this.refreshKanban(items);
          this.loading.set(false);
        },
        error: (err) => {
          this.notify.error(err?.error?.message ?? 'Failed to load leads');
          this.loading.set(false);
        },
      });

    if (!this.auth.hasPermission(Permissions.ManageLeads)) return;
    this.leadService.getPaged({ pageNumber: 1, pageSize: 500 }).subscribe({
      next: (res) => this.refreshKanban(res.data?.items ?? []),
    });
  }

  refreshKanban(leads: Lead[]): void {
    for (const col of this.kanbanColumns) {
      col.leads = leads.filter((l) => l.status === col.status);
    }
  }

  onDrop(event: CdkDragDrop<Lead[]>, targetStatus: string): void {
    if (!this.auth.hasPermission(Permissions.ManageLeads)) return;
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
      return;
    }
    const lead = event.previousContainer.data[event.previousIndex];
    transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    this.leadService.updateStatus(lead.id, targetStatus).subscribe({
      error: () => {
        this.notify.error('Failed to update lead status');
        this.load();
      },
    });
  }
}
