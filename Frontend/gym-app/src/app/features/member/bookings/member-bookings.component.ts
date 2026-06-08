import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BookingService } from '../../../core/services/booking.service';
import { AvailableSlot } from '../../../shared/models/booking.models';

@Component({
  selector: 'app-member-bookings',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, MatSnackBarModule],
  templateUrl: './member-bookings.component.html',
  styleUrl: './member-bookings.component.css',
})
export class MemberBookingsComponent implements OnInit {
  private readonly svc = inject(BookingService);
  private readonly snack = inject(MatSnackBar);

  slots: AvailableSlot[] = [];
  fromDate = new Date().toISOString().slice(0, 10);
  toDate = new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 10);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.svc.getAvailableSlots(this.fromDate, this.toDate).subscribe({
      next: (r) => {
        if (r.success && r.data) this.slots = r.data;
      },
    });
  }

  book(slot: AvailableSlot): void {
    this.svc.book(slot.classScheduleId, slot.bookingDate).subscribe({
      next: () => {
        this.snack.open('Booking confirmed!', 'OK', { duration: 2500 });
        this.load();
      },
      error: (e) => this.snack.open(e.error?.message ?? 'Booking failed.', 'Dismiss', { duration: 4000 }),
    });
  }

  waitlist(slot: AvailableSlot): void {
    this.svc.joinWaitlist(slot.classScheduleId, slot.bookingDate).subscribe({
      next: () => {
        this.snack.open('Added to waitlist.', 'OK', { duration: 2500 });
        this.load();
      },
      error: () => this.snack.open('Waitlist failed.', 'Dismiss', { duration: 4000 }),
    });
  }
}
