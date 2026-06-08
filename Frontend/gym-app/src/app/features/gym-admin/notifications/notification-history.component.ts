import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { GymNotificationService } from '../../../core/services/gym-notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NOTIFICATION_STATUSES, NOTIFICATION_TYPES, NotificationLog } from '../../../shared/models/notification.models';

@Component({
  selector: 'app-notification-history',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Notification History" subtitle="Delivery logs and status">
      <button mat-stroked-button routerLink="..">Dashboard</button>
    </app-page-header>

    <form class="filters" [formGroup]="filterForm">
      <mat-form-field appearance="outline"><mat-label>Search</mat-label><input matInput formControlName="search" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Type</mat-label>
        <mat-select formControlName="notificationType"><mat-option value="">All</mat-option>@for (t of types; track t) { <mat-option [value]="t">{{ t }}</mat-option> }</mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline"><mat-label>Status</mat-label>
        <mat-select formControlName="status"><mat-option value="">All</mat-option>@for (s of statuses; track s) { <mat-option [value]="s">{{ s }}</mat-option> }</mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline"><mat-label>From</mat-label><input matInput type="date" formControlName="fromDate" /></mat-form-field>
      <mat-form-field appearance="outline"><mat-label>To</mat-label><input matInput type="date" formControlName="toDate" /></mat-form-field>
    </form>

    @if (loading()) { <mat-spinner class="center-spinner" /> }
    @else {
      <div class="table-card">
        <table mat-table [dataSource]="rows">
          <ng-container matColumnDef="createdAt"><th mat-header-cell *matHeaderCellDef>Date</th><td mat-cell *matCellDef="let r">{{ r.createdAt | date:'short' }}</td></ng-container>
          <ng-container matColumnDef="type"><th mat-header-cell *matHeaderCellDef>Type</th><td mat-cell *matCellDef="let r">{{ r.notificationType }}</td></ng-container>
          <ng-container matColumnDef="phone"><th mat-header-cell *matHeaderCellDef>Phone</th><td mat-cell *matCellDef="let r">{{ r.recipientPhone }}</td></ng-container>
          <ng-container matColumnDef="template"><th mat-header-cell *matHeaderCellDef>Template</th><td mat-cell *matCellDef="let r">{{ r.whatsAppTemplateName }}</td></ng-container>
          <ng-container matColumnDef="status"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r" [class.failed]="r.status === 'Failed'">{{ r.status }}</td></ng-container>
          <ng-container matColumnDef="error"><th mat-header-cell *matHeaderCellDef>Error</th><td mat-cell *matCellDef="let r">{{ r.errorMessage || '—' }}</td></ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols"></tr>
        </table>
        <mat-paginator [length]="totalCount" [pageSize]="pageSize" [pageIndex]="pageIndex" [pageSizeOptions]="[10,20,50]" (page)="onPage($event)" />
      </div>
    }
  `,
  styles: [
    `
      .filters { display: flex; flex-wrap: wrap; gap: 0.75rem; margin-bottom: 1rem; }
      .table-card { background: #fff; border-radius: 8px; overflow: auto; }
      table { width: 100%; }
      .failed { color: #c62828; }
      .center-spinner { margin: 2rem auto; display: block; }
    `,
  ],
})
export class NotificationHistoryComponent implements OnInit {
  private readonly svc = inject(GymNotificationService);
  private readonly fb = inject(FormBuilder);
  readonly types = NOTIFICATION_TYPES;
  readonly statuses = NOTIFICATION_STATUSES;

  loading = signal(true);
  rows: NotificationLog[] = [];
  totalCount = 0;
  pageIndex = 0;
  pageSize = 20;
  cols = ['createdAt', 'type', 'phone', 'template', 'status', 'error'];

  filterForm = this.fb.nonNullable.group({
    search: '',
    notificationType: '',
    status: '',
    fromDate: '',
    toDate: '',
  });

  ngOnInit(): void {
    this.load();
    this.filterForm.valueChanges.pipe(debounceTime(350), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.load();
    });
  }

  onPage(e: PageEvent): void {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    const f = this.filterForm.getRawValue();
    this.svc.searchHistory({
      search: f.search || undefined,
      notificationType: f.notificationType || undefined,
      status: f.status || undefined,
      fromDate: f.fromDate || undefined,
      toDate: f.toDate || undefined,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.rows = res.data.items;
          this.totalCount = res.data.totalCount;
        }
      },
      error: () => this.loading.set(false),
    });
  }
}
