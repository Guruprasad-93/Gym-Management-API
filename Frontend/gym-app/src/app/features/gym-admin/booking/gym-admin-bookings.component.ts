import { DatePipe, NgClass } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { BookingService } from '../../../core/services/booking.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SlotBooking } from '../../../shared/models/booking.models';

@Component({
  selector: 'app-gym-admin-bookings',
  standalone: true,
  imports: [DatePipe, NgClass, RouterLink, MatIconModule, MatProgressSpinnerModule, MatTableModule],
  templateUrl: './gym-admin-bookings.component.html',
  styleUrl: './gym-admin-bookings.component.css',
})
export class GymAdminBookingsComponent implements OnInit {
  private readonly svc = inject(BookingService);
  private readonly notify = inject(NotificationService);

  loading = signal(true);
  bookings = signal<SlotBooking[]>([]);
  cols = ['member', 'class', 'schedule', 'trainer', 'status'];

  ngOnInit(): void {
    this.svc.getBookings(1, 100).subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) this.bookings.set(r.data.items);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load bookings');
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

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Confirmed':
      case 'CheckedIn':
        return 'status-badge--confirmed';
      case 'Cancelled':
        return 'status-badge--cancelled';
      case 'NoShow':
        return 'status-badge--noshow';
      default:
        return 'status-badge--muted';
    }
  }
}
