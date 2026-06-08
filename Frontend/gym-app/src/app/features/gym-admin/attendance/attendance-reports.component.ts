import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { DailyAttendanceReport, MonthlyAttendanceReport } from '../../../shared/models/attendance.models';

@Component({
  selector: 'app-attendance-reports',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './attendance-reports.component.html',
  styleUrl: './attendance-reports.component.css',
})
export class AttendanceReportsComponent {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  canExport = this.auth.hasPermission(Permissions.ExportAttendanceReports);
  private readonly svc = inject(AttendanceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  reportTab = signal<'daily' | 'monthly'>('daily');
  dailyDate = this.fb.nonNullable.control(new Date().toISOString().slice(0, 10));
  year = this.fb.nonNullable.control(new Date().getFullYear());
  month = this.fb.nonNullable.control(new Date().getMonth() + 1);

  dailyReport = signal<DailyAttendanceReport | null>(null);
  monthlyReport = signal<MonthlyAttendanceReport | null>(null);
  loadingDaily = signal(false);
  loadingMonthly = signal(false);
  dailyLoaded = signal(false);
  monthlyLoaded = signal(false);

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  loadDaily(): void {
    this.loadingDaily.set(true);
    this.svc.getDailyReport(this.dailyDate.value).subscribe({
      next: (r) => {
        this.loadingDaily.set(false);
        this.dailyLoaded.set(true);
        if (r.success && r.data) {
          this.dailyReport.set(r.data);
        } else {
          this.dailyReport.set(null);
        }
      },
      error: () => {
        this.loadingDaily.set(false);
        this.notify.error('Failed to load daily report');
      },
    });
  }

  loadMonthly(): void {
    this.loadingMonthly.set(true);
    this.svc.getMonthlyReport(this.year.value, this.month.value).subscribe({
      next: (r) => {
        this.loadingMonthly.set(false);
        this.monthlyLoaded.set(true);
        if (r.success && r.data) {
          this.monthlyReport.set(r.data);
        } else {
          this.monthlyReport.set(null);
        }
      },
      error: () => {
        this.loadingMonthly.set(false);
        this.notify.error('Failed to load monthly report');
      },
    });
  }

  exportDaily(type: 'pdf' | 'excel'): void {
    const obs =
      type === 'pdf'
        ? this.svc.downloadDailyPdf(this.dailyDate.value)
        : this.svc.downloadDailyExcel(this.dailyDate.value);
    obs.subscribe({
      next: (b) => this.save(b, `daily.${type === 'pdf' ? 'pdf' : 'xlsx'}`),
      error: () => this.notify.error('Export failed'),
    });
  }

  exportMonthly(type: 'pdf' | 'excel'): void {
    const obs =
      type === 'pdf'
        ? this.svc.downloadMonthlyPdf(this.year.value, this.month.value)
        : this.svc.downloadMonthlyExcel(this.year.value, this.month.value);
    obs.subscribe({
      next: (b) => this.save(b, `monthly.${type === 'pdf' ? 'pdf' : 'xlsx'}`),
      error: () => this.notify.error('Export failed'),
    });
  }

  private save(blob: Blob, name: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = name;
    a.click();
    URL.revokeObjectURL(url);
  }
}
