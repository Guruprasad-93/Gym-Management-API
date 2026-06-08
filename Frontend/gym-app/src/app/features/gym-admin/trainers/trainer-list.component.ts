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
import { TrainerService } from '../../../core/services/trainer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Trainer } from '../../../shared/models/trainer.models';
import { TrainerFormDialogComponent } from './trainer-form-dialog.component';
import { AssignMembersDialogComponent } from './assign-members-dialog.component';

@Component({
  selector: 'app-trainer-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './trainer-list.component.html',
  styleUrl: './trainer-list.component.css',
})
export class TrainerListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);

  displayedColumns = ['fullName', 'specialization', 'members', 'status', 'actions'];
  dataSource = new MatTableDataSource<Trainer>([]);
  loading = signal(false);
  totalCount = signal(0);

  pageIndex = 0;
  pageSize = 10;
  sortColumn = 'UserName';
  sortDirection: 'asc' | 'desc' = 'asc';

  searchControl = this.fb.nonNullable.control('');
  includeInactiveControl = this.fb.nonNullable.control(false);

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No trainers';
    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}–${end} of ${total} trainers`;
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

  trainerInitials(name: string | undefined): string {
    return (name ?? '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  load(): void {
    this.loading.set(true);
    this.trainerService
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
          this.notify.error('Failed to load trainers');
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
    const map: Record<string, string> = {
      fullName: 'UserName',
      specialization: 'Specialization',
    };
    this.sortColumn = map[sort.active] ?? 'UserName';
    this.sortDirection = sort.direction === 'desc' ? 'desc' : 'asc';
    this.load();
  }

  openCreate(): void {
    const ref = this.dialog.open(TrainerFormDialogComponent, { width: '480px' });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  openEdit(trainer: Trainer): void {
    const ref = this.dialog.open(TrainerFormDialogComponent, { width: '480px', data: trainer });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  openAssign(trainer: Trainer): void {
    const ref = this.dialog.open(AssignMembersDialogComponent, {
      width: '560px',
      data: trainer,
    });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  deactivate(trainer: Trainer): void {
    if (!confirm(`Deactivate trainer "${trainer.fullName ?? trainer.id}"? Assigned members will be unassigned.`)) return;
    this.trainerService.delete(trainer.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Trainer deactivated');
          this.load();
        }
      },
      error: () => this.notify.error('Deactivation failed'),
    });
  }
}
