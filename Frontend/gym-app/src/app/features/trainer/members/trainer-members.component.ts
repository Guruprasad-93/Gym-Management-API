import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';

@Component({
  selector: 'app-trainer-members',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatIconModule,
    RouterModule,
  ],
  templateUrl: './trainer-members.component.html',
  styleUrl: './trainer-members.component.css',
})
export class TrainerMembersComponent implements OnInit {
  private readonly memberService = inject(MemberService);
  private readonly notify = inject(NotificationService);

  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  readonly displayedColumns = ['fullName', 'email', 'phone', 'membershipStatus', 'actions'];
  dataSource = new MatTableDataSource<Member>([]);
  loading = signal(true);
  totalCount = signal(0);

  pageIndex = 0;
  pageSize = 10;

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No members';
    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}–${end} of ${total} members`;
  }

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.load();
      });

    this.load();
  }

  memberInitials(name: string): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.memberService
      .getPaged(null, {
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
        search: this.searchControl.value || undefined,
        sortColumn: 'FullName',
        sortDirection: 'asc',
      })
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
}
