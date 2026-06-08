import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MemberService } from '../../../core/services/member.service';
import { AttendanceService } from '../../../core/services/attendance.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';

@Component({
  selector: 'app-attendance-check-in',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, MatIconModule],
  templateUrl: './attendance-check-in.component.html',
  styleUrl: './attendance-check-in.component.css',
})
export class AttendanceCheckInComponent implements OnInit {
  private readonly memberSvc = inject(MemberService);
  private readonly attendanceSvc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  members = signal<Member[]>([]);
  saving = signal(false);
  form = this.fb.nonNullable.group({ memberId: [0, Validators.required], notes: [''] });

  ngOnInit(): void {
    this.memberSvc.getPaged(null, { pageNumber: 1, pageSize: 500, sortColumn: 'FullName' }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.members.set(res.data.items);
      },
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const { memberId, notes } = this.form.getRawValue();
    this.attendanceSvc.checkIn(memberId, notes || undefined).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.notify.success('Checked in');
          this.router.navigate(['../']);
        } else this.notify.error(res.message ?? 'Check-in failed');
      },
      error: (e) => {
        this.saving.set(false);
        this.notify.error(e.error?.message ?? 'Check-in failed');
      },
    });
  }
}
