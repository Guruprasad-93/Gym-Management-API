import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BookingService } from '../../../core/services/booking.service';
import { SlotBooking } from '../../../shared/models/booking.models';

@Component({
  selector: 'app-member-booking-history',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './member-booking-history.component.html',
  styleUrl: './member-booking-history.component.css',
})
export class MemberBookingHistoryComponent implements OnInit {
  private readonly svc = inject(BookingService);
  bookings: SlotBooking[] = [];

  ngOnInit(): void {
    this.svc.getBookings(1, 50).subscribe({
      next: (r) => {
        if (r.success && r.data) this.bookings = r.data.items;
      },
    });
  }
}
