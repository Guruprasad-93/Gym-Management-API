import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime } from 'rxjs';
import { AttendanceService } from '../../../core/services/attendance.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { MemberAttendance, AttendanceStatus } from '../../../shared/models/attendance.models';
import { untilDestroyed } from '../../../core/utils/destroy-ref.util';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Attendance List" subtitle="Search and filter attendance records" />

    <div class="filters">
      <mat-form-field appearance="outline">
        <mat-label>Search</mat-label>
        <input matInput [formControl]="searchControl" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Status</mat-label>
        <mat-select [formControl]="statusControl">
          <mat-option [value]="null">All</mat-option>
          @for (s of statuses; track s.attendanceStatusId) {
            <mat-option [value]="s.attendanceStatusId">{{ s.name }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>From</mat-label>
        <input matInput type="date" [formControl]="fromControl" />
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>To</mat-label>
        <input matInput type="date" [formControl]="toControl" />
      </mat-form-field>
    </div>

    @if (loading()) { <mat-spinner /> } @else {
      <table mat-table [dataSource]="rows" class="full-width">
        <ng-container matColumnDef="memberName"><th mat-header-cell *matHeaderCellDef>Member</th><td mat-cell *matCellDef="let r">{{ r.memberName }}</td></ng-container>
        <ng-container matColumnDef="attendanceDate"><th mat-header-cell *matHeaderCellDef>Date</th><td mat-cell *matCellDef="let r">{{ r.attendanceDate }}</td></ng-container>
        <ng-container matColumnDef="statusName"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r">{{ r.statusName }}</td></ng-container>
        <ng-container matColumnDef="checkInAt"><th mat-header-cell *matHeaderCellDef>Check In</th><td mat-cell *matCellDef="let r">{{ r.checkInAt | date: 'short' }}</td></ng-container>
        <ng-container matColumnDef="checkOutAt"><th mat-header-cell *matHeaderCellDef>Check Out</th><td mat-cell *matCellDef="let r">{{ r.checkOutAt ? (r.checkOutAt | date: 'short') : '—' }}</td></ng-container>
        <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let r">
            <a mat-icon-button [routerLink]="['../members', r.memberId, 'history']"><mat-icon>history</mat-icon></a>
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="cols"></tr>
        <tr mat-row *matRowDef="let row; columns: cols"></tr>
      </table>
      <mat-paginator [length]="total()" [pageSize]="pageSize" [pageIndex]="pageIndex" [pageSizeOptions]="[10,25,50]" (page)="onPage($event)" />
    }
  `,
  styles: [`.filters { display:flex; flex-wrap:wrap; gap:1rem; } .full-width { width:100%; }`],
})
export class AttendanceListComponent implements OnInit {
  private readonly svc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  cols = ['memberName', 'attendanceDate', 'statusName', 'checkInAt', 'checkOutAt', 'actions'];
  rows: MemberAttendance[] = [];
  statuses: AttendanceStatus[] = [];
  loading = signal(true);
  total = signal(0);
  pageIndex = 0;
  pageSize = 10;

  searchControl = this.fb.nonNullable.control('');
  statusControl = this.fb.control<number | null>(null);
  fromControl = this.fb.nonNullable.control(this.isoDate(-30));
  toControl = this.fb.nonNullable.control(this.isoDate(0));

  ngOnInit(): void {
    this.svc.getStatuses().pipe(untilDestroyed()).subscribe((r) => { if (r.success && r.data) this.statuses = r.data; });
    this.searchControl.valueChanges.pipe(debounceTime(350), untilDestroyed()).subscribe(() => { this.pageIndex = 0; this.load(); });
    const reload = () => {
      this.pageIndex = 0;
      this.load();
    };
    this.statusControl.valueChanges.pipe(untilDestroyed()).subscribe(reload);
    this.fromControl.valueChanges.pipe(untilDestroyed()).subscribe(reload);
    this.toControl.valueChanges.pipe(untilDestroyed()).subscribe(reload);
    this.load();
  }

  onPage(e: PageEvent): void { this.pageIndex = e.pageIndex; this.pageSize = e.pageSize; this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getPaged({
      search: this.searchControl.value || undefined,
      statusId: this.statusControl.value ?? undefined,
      fromDate: this.fromControl.value,
      toDate: this.toControl.value,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
    }).pipe(untilDestroyed()).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) { this.rows = res.data.items; this.total.set(res.data.totalCount); }
      },
      error: () => { this.loading.set(false); this.notify.error('Failed to load attendance'); },
    });
  }

  private isoDate(offsetDays: number): string {
    const d = new Date(); d.setDate(d.getDate() + offsetDays);
    return d.toISOString().slice(0, 10);
  }
}
