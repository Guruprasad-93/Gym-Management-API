import { DatePipe, NgClass } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Membership } from '../../../shared/models/membership-payment.models';
import { CreateMembershipDialogComponent } from './create-membership-dialog.component';
import { RenewMembershipDialogComponent } from './renew-membership-dialog.component';

@Component({
  selector: 'app-membership-list',
  standalone: true,
  imports: [
    DatePipe,
    NgClass,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './membership-list.component.html',
  styleUrl: './membership-list.component.css',
})
export class MembershipListComponent implements OnInit, AfterViewInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns = ['member', 'plan', 'dates', 'status', 'actions'];
  dataSource = new MatTableDataSource<Membership>([]);
  loading = signal(true);
  searchControl = this.fb.nonNullable.control('');

  get pageSummary(): string {
    const total = this.dataSource.data.length;
    if (!total) return 'No memberships';
    const pageIndex = this.paginator?.pageIndex ?? 0;
    const pageSize = this.paginator?.pageSize ?? 10;
    const start = pageIndex * pageSize + 1;
    const end = Math.min((pageIndex + 1) * pageSize, total);
    return `Showing ${start}–${end} of ${total} memberships`;
  }

  ngOnInit(): void {
    this.load();
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => {
        this.paginator?.firstPage();
        this.load();
      });
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
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
      case 'Active':
        return 'status-badge--active';
      case 'Expired':
        return 'status-badge--expired';
      case 'Cancelled':
        return 'status-badge--cancelled';
      default:
        return 'status-badge--muted';
    }
  }

  load(): void {
    this.loading.set(true);
    this.svc.getAll(this.searchControl.value || undefined, true).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.dataSource.data = res.data;
          this.cdr.detectChanges();
          if (this.paginator) {
            this.dataSource.paginator = this.paginator;
          }
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Load failed');
      },
    });
  }

  openCreate(): void {
    this.dialog
      .open(CreateMembershipDialogComponent, { width: '480px' })
      .afterClosed()
      .subscribe((ok) => ok && this.load());
  }

  renew(m: Membership): void {
    this.dialog
      .open(RenewMembershipDialogComponent, { width: '400px', data: m })
      .afterClosed()
      .subscribe((ok) => ok && this.load());
  }

  cancel(m: Membership): void {
    if (!confirm(`Cancel membership for ${m.memberName}?`)) return;
    this.svc.cancel(m.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Cancelled');
          this.load();
        }
      },
      error: () => this.notify.error('Cancel failed'),
    });
  }
}
