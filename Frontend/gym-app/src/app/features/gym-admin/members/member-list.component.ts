import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Member } from '../../../shared/models/member.models';
import { MemberFormDialogComponent } from './member-form-dialog.component';
import { AssignTrainerDialogComponent } from './assign-trainer-dialog.component';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css',
})
export class MemberListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly memberService = inject(MemberService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);

  displayedColumns = ['fullName', 'phone', 'membership', 'trainer', 'joinDate', 'status', 'actions'];
  dataSource = new MatTableDataSource<Member>([]);
  loading = signal(false);
  totalCount = signal(0);

  pageIndex = 0;
  pageSize = 10;
  sortColumn = 'FullName';
  sortDirection: 'asc' | 'desc' = 'asc';

  searchControl = this.fb.nonNullable.control('');
  includeInactiveControl = this.fb.nonNullable.control(false);

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No members';
    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}–${end} of ${total} members`;
  }

  ngOnInit(): void {
    this.load();
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.load();
      });
    this.includeInactiveControl.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.load();
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

  load(): void {
    this.loading.set(true);
    this.memberService
      .getPaged(
        null,
        {
          pageNumber: this.pageIndex + 1,
          pageSize: this.pageSize,
          search: this.searchControl.value || undefined,
          sortColumn: this.sortColumn,
          sortDirection: this.sortDirection,
        },
        this.includeInactiveControl.value,
      )
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) {
            this.dataSource.data = res.data.items;
            this.totalCount.set(res.data.totalCount);
          }
        },
        error: () => {
          this.loading.set(false);
          this.notify.error('Failed to load members');
        },
      });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  onSort(sort: Sort): void {
    if (!sort.active || !sort.direction) return;
    const map: Record<string, string> = { fullName: 'FullName', phone: 'Phone', joinDate: 'JoinDate' };
    this.sortColumn = map[sort.active] ?? 'FullName';
    this.sortDirection = sort.direction === 'desc' ? 'desc' : 'asc';
    this.load();
  }

  openCreate(): void {
    const ref = this.dialog.open(MemberFormDialogComponent, { width: '520px' });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  openEdit(member: Member): void {
    const ref = this.dialog.open(MemberFormDialogComponent, { width: '520px', data: member });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  openAssignTrainer(member: Member): void {
    const ref = this.dialog.open(AssignTrainerDialogComponent, { width: '420px', data: member });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  remove(member: Member): void {
    if (!confirm(`Delete member "${member.fullName}"? This soft-deletes the record.`)) return;
    this.memberService.delete(member.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Member deleted');
          this.load();
        }
      },
      error: () => this.notify.error('Delete failed'),
    });
  }
}
