import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DAY_NAMES, TrainerScheduleItem } from '../../../shared/models/booking.models';
import { BookingService } from '../../../core/services/booking.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

@Component({
  selector: 'app-trainer-schedule',
  standalone: true,
  imports: [FormsModule, RouterLink, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './trainer-schedule.component.html',
  styleUrl: './trainer-schedule.component.css',
})
export class TrainerScheduleComponent implements OnInit {
  private readonly svc = inject(BookingService);

  schedule: TrainerScheduleItem[] = [];
  loading = true;
  search = '';
  fromDate = new Date().toISOString().slice(0, 10);
  toDate = new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10);
  readonly DAY_NAMES = DAY_NAMES;

  get filteredSchedule(): TrainerScheduleItem[] {
    const q = this.search.trim().toLowerCase();
    if (!q) return this.schedule;
    return this.schedule.filter(
      (s) => s.className.toLowerCase().includes(q) || s.branchName.toLowerCase().includes(q),
    );
  }

  get totalBookings(): number {
    return this.schedule.reduce((sum, s) => sum + s.bookingCount, 0);
  }

  get avgFill(): number {
    if (!this.schedule.length) return 0;
    const total = this.schedule.reduce((sum, s) => sum + this.fillPercent(s), 0);
    return Math.round(total / this.schedule.length);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.svc.getTrainerSchedule(this.fromDate, this.toDate).subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success && r.data) this.schedule = r.data;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  fillPercent(s: TrainerScheduleItem): number {
    if (!s.capacity) return 0;
    return Math.min(100, Math.round((s.bookingCount / s.capacity) * 100));
  }
}
