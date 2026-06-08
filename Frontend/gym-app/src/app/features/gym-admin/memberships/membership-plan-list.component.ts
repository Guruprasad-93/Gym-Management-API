import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { MembershipPlan } from '../../../shared/models/membership-payment.models';
import { MembershipPlanFormDialogComponent } from './membership-plan-form-dialog.component';

@Component({
  selector: 'app-membership-plan-list',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatTableModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './membership-plan-list.component.html',
  styleUrl: './membership-plan-list.component.css',
})
export class MembershipPlanListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  loading = signal(true);
  plans = signal<MembershipPlan[]>([]);
  cols = ['planName', 'duration', 'price', 'status', 'actions'];

  get pageSummary(): string {
    const count = this.plans().length;
    if (!count) return 'No plans';
    return `${count} plan${count === 1 ? '' : 's'} configured`;
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getPlans(true).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.plans.set(res.data);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load plans');
      },
    });
  }

  openCreate(): void {
    this.dialog
      .open(MembershipPlanFormDialogComponent, { width: '440px' })
      .afterClosed()
      .subscribe((ok) => ok && this.load());
  }

  openEdit(p: MembershipPlan): void {
    this.dialog
      .open(MembershipPlanFormDialogComponent, { width: '440px', data: p })
      .afterClosed()
      .subscribe((ok) => ok && this.load());
  }

  remove(p: MembershipPlan): void {
    if (!confirm(`Deactivate plan "${p.planName}"?`)) return;
    this.svc.deletePlan(p.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Plan deactivated');
          this.load();
        }
      },
      error: () => this.notify.error('Delete failed'),
    });
  }
}
