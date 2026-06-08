import { CurrencyPipe, DatePipe, NgClass } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { PaymentService } from '../../../core/services/payment.service';
import { RazorpayService } from '../../../core/services/razorpay.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Payment } from '../../../shared/models/membership-payment.models';
import { PaymentFormDialogComponent } from './payment-form-dialog.component';

@Component({
  selector: 'app-payment-list',
  standalone: true,
  imports: [
    CurrencyPipe,
    DatePipe,
    NgClass,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './payment-list.component.html',
  styleUrl: './payment-list.component.css',
})
export class PaymentListComponent implements OnInit, AfterViewInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(PaymentService);
  private readonly razorpay = inject(RazorpayService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  displayedColumns = ['member', 'amount', 'method', 'status', 'reference', 'date', 'actions'];
  dataSource = new MatTableDataSource<Payment>([]);
  loading = signal(true);
  searchControl = this.fb.nonNullable.control('');

  get pageSummary(): string {
    const total = this.dataSource.data.length;
    if (!total) return 'No payments';
    const pageIndex = this.paginator?.pageIndex ?? 0;
    const pageSize = this.paginator?.pageSize ?? 10;
    const start = pageIndex * pageSize + 1;
    const end = Math.min((pageIndex + 1) * pageSize, total);
    return `Showing ${start}–${end} of ${total} payments`;
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

  memberInitials(name: string | undefined): string {
    return (name ?? '—')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('') || '?';
  }

  statusBadgeClass(status: string): string {
    switch (status) {
      case 'Completed':
        return 'status-badge--completed';
      case 'Failed':
        return 'status-badge--failed';
      case 'Refunded':
        return 'status-badge--refunded';
      default:
        return 'status-badge--pending';
    }
  }

  load(): void {
    this.loading.set(true);
    this.svc.getAll(this.searchControl.value || undefined).subscribe({
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
      .open(PaymentFormDialogComponent, { width: '480px' })
      .afterClosed()
      .subscribe((ok) => ok && this.load());
  }

  invoice(p: Payment): void {
    this.svc.generateInvoice(p.id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.svc.downloadInvoice(res.data.id).subscribe({
            next: (blob) => {
              const url = URL.createObjectURL(blob);
              const a = document.createElement('a');
              a.href = url;
              a.download = `${res.data!.invoiceNumber}.pdf`;
              a.click();
              URL.revokeObjectURL(url);
            },
          });
        }
      },
      error: (e) => this.notify.error(e.error?.message ?? 'Invoice failed'),
    });
  }

  refund(p: Payment): void {
    const reason = window.prompt('Refund reason (optional):') ?? undefined;
    this.razorpay.refund(p.id, { reason }).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Payment refunded.');
          this.load();
        }
      },
      error: (e) => this.notify.error(e.error?.message ?? 'Refund failed'),
    });
  }
}
