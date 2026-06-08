import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { Membership } from '../../../shared/models/membership-payment.models';

@Component({
  selector: 'app-expired-membership-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTableModule, MatButtonModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header title="Expired Memberships" subtitle="Memberships past end date">
      <button mat-button routerLink="/gym-admin/memberships">Back</button>
    </app-page-header>
    @if (loading()) { <mat-spinner /> } @else {
      <table mat-table [dataSource]="items()" class="table-card">
        <ng-container matColumnDef="member"><th mat-header-cell *matHeaderCellDef>Member</th><td mat-cell *matCellDef="let r">{{ r.memberName }}</td></ng-container>
        <ng-container matColumnDef="plan"><th mat-header-cell *matHeaderCellDef>Plan</th><td mat-cell *matCellDef="let r">{{ r.planName }}</td></ng-container>
        <ng-container matColumnDef="endDate"><th mat-header-cell *matHeaderCellDef>Expired</th><td mat-cell *matCellDef="let r">{{ r.endDate }}</td></ng-container>
        <tr mat-header-row *matHeaderRowDef="cols"></tr><tr mat-row *matRowDef="let row; columns: cols"></tr>
      </table>
    }
  `,
  styles: [`table { width:100%; background:#fff; border-radius:8px; }`],
})
export class ExpiredMembershipListComponent implements OnInit {
  private readonly svc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  loading = signal(true);
  items = signal<Membership[]>([]);
  cols = ['member', 'plan', 'endDate'];

  ngOnInit(): void {
    this.svc.getExpired().subscribe({
      next: (res) => { this.loading.set(false); if (res.success && res.data) this.items.set(res.data); },
      error: () => { this.loading.set(false); this.notify.error('Load failed'); },
    });
  }
}
