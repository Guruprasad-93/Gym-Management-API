import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { MemberAttendance } from '../../../shared/models/attendance.models';

@Component({
  selector: 'app-member-attendance-history',
  standalone: true,
  imports: [DatePipe, MatTableModule, MatPaginatorModule, MatButtonModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header title="Member Attendance History" [subtitle]="'Member #' + memberId" />
    @if (auth.hasPermission(permissions.ExportAttendanceReports)) {
      <button mat-stroked-button type="button" (click)="exportExcel()">Export Excel</button>
    }
    @if (loading()) { <mat-spinner /> } @else {
      <table mat-table [dataSource]="rows" class="full-width">
        <ng-container matColumnDef="attendanceDate"><th mat-header-cell *matHeaderCellDef>Date</th><td mat-cell *matCellDef="let r">{{ r.attendanceDate }}</td></ng-container>
        <ng-container matColumnDef="statusName"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r">{{ r.statusName }}</td></ng-container>
        <ng-container matColumnDef="checkInAt"><th mat-header-cell *matHeaderCellDef>Check In</th><td mat-cell *matCellDef="let r">{{ r.checkInAt | date:'short' }}</td></ng-container>
        <ng-container matColumnDef="checkOutAt"><th mat-header-cell *matHeaderCellDef>Check Out</th><td mat-cell *matCellDef="let r">{{ r.checkOutAt ? (r.checkOutAt | date:'short') : '—' }}</td></ng-container>
        <tr mat-header-row *matHeaderRowDef="cols"></tr>
        <tr mat-row *matRowDef="let row; columns: cols"></tr>
      </table>
      <mat-paginator [length]="total()" [pageSize]="pageSize" [pageIndex]="pageIndex" (page)="onPage($event)" />
    }
  `,
  styles: [`.full-width { width:100%; }`],
})
export class MemberAttendanceHistoryComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly route = inject(ActivatedRoute);
  private readonly svc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);

  memberId = Number(this.route.snapshot.paramMap.get('id'));
  cols = ['attendanceDate', 'statusName', 'checkInAt', 'checkOutAt'];
  rows: MemberAttendance[] = [];
  loading = signal(true);
  total = signal(0);
  pageIndex = 0;
  pageSize = 10;

  ngOnInit(): void { this.load(); }
  onPage(e: PageEvent): void { this.pageIndex = e.pageIndex; this.pageSize = e.pageSize; this.load(); }

  load(): void {
    this.loading.set(true);
    this.svc.getMemberHistory(this.memberId, { pageNumber: this.pageIndex + 1, pageSize: this.pageSize }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) { this.rows = res.data.items; this.total.set(res.data.totalCount); }
      },
      error: () => { this.loading.set(false); this.notify.error('Failed to load history'); },
    });
  }

  exportExcel(): void {
    this.svc.downloadMemberHistoryExcel(this.memberId, { pageNumber: 1, pageSize: 5000 }).subscribe({
      next: (blob) => this.downloadBlob(blob, `member-${this.memberId}-attendance.xlsx`),
      error: () => this.notify.error('Export failed'),
    });
  }

  private downloadBlob(blob: Blob, name: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = name; a.click();
    URL.revokeObjectURL(url);
  }
}
