import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BookingService } from '../../../core/services/booking.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ClassSchedule, DAY_NAMES } from '../../../shared/models/booking.models';

@Component({
  selector: 'app-schedule-list',
  standalone: true,
  imports: [RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './schedule-list.component.html',
  styleUrl: './schedule-list.component.css',
})
export class ScheduleListComponent implements OnInit {
  private readonly svc = inject(BookingService);
  private readonly notify = inject(NotificationService);

  readonly dayNames = DAY_NAMES;
  loading = signal(true);
  schedules = signal<ClassSchedule[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getSchedules().subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) this.schedules.set(r.data.items);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load schedules');
      },
    });
  }

  remove(id: number): void {
    if (!confirm('Cancel this class schedule?')) return;
    this.svc.deleteSchedule(id).subscribe({
      next: () => {
        this.notify.success('Schedule cancelled');
        this.load();
      },
      error: () => this.notify.error('Failed to cancel schedule'),
    });
  }
}
