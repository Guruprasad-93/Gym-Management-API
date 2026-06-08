import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BookingService } from '../../../core/services/booking.service';
import { SlotBooking } from '../../../shared/models/booking.models';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

@Component({
  selector: 'app-trainer-bookings',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './trainer-bookings.component.html',
  styleUrl: './trainer-bookings.component.css',
})
export class TrainerBookingsComponent implements OnInit {
  private readonly svc = inject(BookingService);

  bookings: SlotBooking[] = [];
  loading = true;
  search = '';
  statusFilter = 'all';

  get filteredBookings(): SlotBooking[] {
    const q = this.search.trim().toLowerCase();
    return this.bookings.filter((b) => {
      const matchesStatus = this.statusFilter === 'all' || b.status === this.statusFilter;
      const matchesSearch =
        !q ||
        b.memberName.toLowerCase().includes(q) ||
        b.className.toLowerCase().includes(q) ||
        b.branchName.toLowerCase().includes(q);
      return matchesStatus && matchesSearch;
    });
  }

  get confirmedCount(): number {
    return this.bookings.filter((b) => b.status === 'Confirmed').length;
  }

  get waitlistedCount(): number {
    return this.bookings.filter((b) => b.status === 'Waitlisted').length;
  }

  ngOnInit(): void {
    this.svc.getBookings(1, 100).subscribe({
      next: (r) => {
        this.loading = false;
        if (r.success && r.data) this.bookings = r.data.items;
      },
      error: () => {
        this.loading = false;
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
