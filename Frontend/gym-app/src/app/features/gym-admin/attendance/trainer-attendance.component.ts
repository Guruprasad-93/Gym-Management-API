import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AttendanceService } from '../../../core/services/attendance.service';
import { TrainerService } from '../../../core/services/trainer.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { TrainerAttendance } from '../../../shared/models/attendance.models';
import { Trainer } from '../../../shared/models/trainer.models';

@Component({
  selector: 'app-trainer-attendance',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, MatTableModule, MatPaginatorModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header title="Trainer Attendance" subtitle="Trainer check-in/out records" />
    @if (auth.hasPermission(permissions.ManageTrainerAttendance)) {
      <div class="actions">
        <mat-form-field appearance="outline"><mat-label>Trainer</mat-label>
          <mat-select [formControl]="trainerControl"><mat-option [value]="null">Select</mat-option>
            @for (t of trainers(); track t.id) { <mat-option [value]="t.id">{{ t.fullName }}</mat-option> }
          </mat-select>
        </mat-form-field>
        <button mat-flat-button color="primary" type="button" (click)="checkIn()">Check In</button>
        <button mat-stroked-button type="button" (click)="checkOut()">Check Out</button>
      </div>
    }
    @if (loading()) { <mat-spinner /> } @else {
      <table mat-table [dataSource]="rows" class="full-width">
        <ng-container matColumnDef="trainerName"><th mat-header-cell *matHeaderCellDef>Trainer</th><td mat-cell *matCellDef="let r">{{ r.trainerName }}</td></ng-container>
        <ng-container matColumnDef="attendanceDate"><th mat-header-cell *matHeaderCellDef>Date</th><td mat-cell *matCellDef="let r">{{ r.attendanceDate }}</td></ng-container>
        <ng-container matColumnDef="statusName"><th mat-header-cell *matHeaderCellDef>Status</th><td mat-cell *matCellDef="let r">{{ r.statusName }}</td></ng-container>
        <ng-container matColumnDef="checkInAt"><th mat-header-cell *matHeaderCellDef>In</th><td mat-cell *matCellDef="let r">{{ r.checkInAt | date:'short' }}</td></ng-container>
        <ng-container matColumnDef="checkOutAt"><th mat-header-cell *matHeaderCellDef>Out</th><td mat-cell *matCellDef="let r">{{ r.checkOutAt ? (r.checkOutAt | date:'short') : '—' }}</td></ng-container>
        <tr mat-header-row *matHeaderRowDef="cols"></tr>
        <tr mat-row *matRowDef="let row; columns: cols"></tr>
      </table>
      <mat-paginator [length]="total()" [pageSize]="10" [pageIndex]="pageIndex" (page)="onPage($event)" />
    }
  `,
  styles: [`.actions { display:flex; gap:1rem; flex-wrap:wrap; align-items:center; margin-bottom:1rem; } .full-width{width:100%}`],
})
export class TrainerAttendanceComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(AttendanceService);
  private readonly trainerSvc = inject(TrainerService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  cols = ['trainerName', 'attendanceDate', 'statusName', 'checkInAt', 'checkOutAt'];
  rows: TrainerAttendance[] = [];
  trainers = signal<Trainer[]>([]);
  trainerControl = this.fb.control<number | null>(null);
  loading = signal(true);
  total = signal(0);
  pageIndex = 0;

  ngOnInit(): void {
    this.trainerSvc.getPaged(null, { pageNumber: 1, pageSize: 200, sortColumn: 'UserName' }).subscribe({
      next: (r) => { if (r.success && r.data) this.trainers.set(r.data.items); },
    });
    this.load();
  }

  onPage(e: PageEvent): void { this.pageIndex = e.pageIndex; this.load(); }

  load(): void {
    this.loading.set(true);
    const from = new Date(); from.setDate(from.getDate() - 30);
    this.svc.getTrainerAttendance(undefined, from.toISOString().slice(0, 10), new Date().toISOString().slice(0, 10), this.pageIndex + 1, 10).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) { this.rows = res.data.items; this.total.set(res.data.totalCount); }
      },
      error: () => { this.loading.set(false); this.notify.error('Failed to load'); },
    });
  }

  checkIn(): void {
    const id = this.trainerControl.value;
    if (!id) return;
    this.svc.trainerCheckIn(id).subscribe({
      next: (r) => { if (r.success) { this.notify.success('Trainer checked in'); this.load(); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Failed'),
    });
  }

  checkOut(): void {
    const id = this.trainerControl.value;
    if (!id) return;
    this.svc.trainerCheckOut(id).subscribe({
      next: (r) => { if (r.success) { this.notify.success('Trainer checked out'); this.load(); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Failed'),
    });
  }
}
