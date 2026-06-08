import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AttendanceService } from '../../../core/services/attendance.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MemberAttendance } from '../../../shared/models/attendance.models';

@Component({
  selector: 'app-attendance-check-out',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, MatIconModule],
  templateUrl: './attendance-check-out.component.html',
  styleUrl: './attendance-check-out.component.css',
})
export class AttendanceCheckOutComponent implements OnInit {
  private readonly svc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  openSessions = signal<MemberAttendance[]>([]);
  saving = signal(false);
  form = this.fb.nonNullable.group({ memberId: [0, Validators.required], notes: [''] });

  ngOnInit(): void {
    this.svc.getToday().subscribe({
      next: (res) => {
        if (res.success && res.data)
          this.openSessions.set(res.data.filter((r) => !r.checkOutAt && r.statusCode === 'CHECKED_IN'));
      },
    });
  }

  submit(): void {
    const { memberId, notes } = this.form.getRawValue();
    this.saving.set(true);
    this.svc.checkOut(memberId, notes || undefined).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.notify.success('Checked out');
          this.router.navigate(['../']);
        } else this.notify.error(res.message ?? 'Check-out failed');
      },
      error: (e) => {
        this.saving.set(false);
        this.notify.error(e.error?.message ?? 'Check-out failed');
      },
    });
  }
}
