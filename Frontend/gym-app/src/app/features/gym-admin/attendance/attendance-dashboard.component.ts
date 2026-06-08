import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { AttendanceDashboard, MemberAttendance } from '../../../shared/models/attendance.models';

@Component({
  selector: 'app-attendance-dashboard',
  standalone: true,
  imports: [DatePipe, RouterModule, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './attendance-dashboard.component.html',
  styleUrl: './attendance-dashboard.component.css',
})
export class AttendanceDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);
  private readonly router = inject(Router);

  loading = signal(true);
  today = signal<MemberAttendance[]>([]);
  stats: AttendanceDashboard | null = null;

  get showFullListLink(): boolean {
    return !this.router.url.includes('/trainer/');
  }

  ngOnInit(): void {
    this.svc.getDashboard().subscribe({
      next: (res) => {
        if (res.success && res.data) this.stats = res.data;
        this.svc.getToday().subscribe({
          next: (t) => {
            this.loading.set(false);
            if (t.success && t.data) this.today.set(t.data);
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load attendance dashboard');
      },
    });
  }

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }
}
