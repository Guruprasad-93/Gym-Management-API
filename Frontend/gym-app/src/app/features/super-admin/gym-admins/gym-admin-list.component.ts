import { Component, OnInit, ViewChild, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { GymAdminService } from '../../../core/services/gym-admin.service';
import { GymService } from '../../../core/services/gym.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { GymAdmin } from '../../../shared/models/gym-admin.models';
import { Gym } from '../../../shared/models/gym.models';
import { GymAdminFormDialogComponent } from './gym-admin-form-dialog.component';
import { TemporaryPasswordDialogComponent } from './temporary-password-dialog.component';

@Component({
  selector: 'app-gym-admin-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    RouterModule,
  ],
  templateUrl: './gym-admin-list.component.html',
  styleUrl: './gym-admin-list.component.css',
})
export class GymAdminListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly gymAdminService = inject(GymAdminService);
  private readonly gymService = inject(GymService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['name', 'email', 'gymName', 'status', 'actions'];
  dataSource = new MatTableDataSource<GymAdmin>([]);
  gyms = signal<Gym[]>([]);
  loading = signal(false);
  totalCount = signal(0);

  pageIndex = 0;
  pageSize = 10;
  sortColumn = 'Name';
  sortDirection: 'asc' | 'desc' = 'asc';

  gymFilter = this.fb.nonNullable.control('');
  searchControl = this.fb.nonNullable.control('');

  get pageSummary(): string {
    const total = this.totalCount();
    if (!total) return 'No gym administrators';

    const start = this.pageIndex * this.pageSize + 1;
    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);
    return `Showing ${start}-${end} of ${total} administrators`;
  }

  ngOnInit(): void {
    const gymId = this.route.snapshot.queryParamMap.get('gymId');
    if (gymId) this.gymFilter.setValue(gymId);

    this.loadGyms();
    this.loadAdmins();

    this.gymFilter.valueChanges.subscribe(() => {
      this.pageIndex = 0;
      this.loadAdmins();
    });

    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadAdmins();
      });
  }

  adminInitials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  loadGyms(): void {
    this.gymService.getAll().subscribe({
      next: (res) => {
        if (res.success && res.data) this.gyms.set(res.data);
      },
    });
  }

  loadAdmins(): void {
    this.loading.set(true);
    const gymId = this.gymFilter.value || null;
    this.gymAdminService
      .getAll(gymId, {
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
        search: this.searchControl.value || undefined,
        sortColumn: this.sortColumn,
        sortDirection: this.sortDirection,
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
          this.notify.error('Failed to load gym admins');
        },
      });
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadAdmins();
  }

  onSort(sort: Sort): void {
    this.sortColumn = sort.active === 'email' ? 'Email' : sort.active === 'name' ? 'Name' : 'Name';
    this.sortDirection = sort.direction === 'desc' ? 'desc' : 'asc';
    this.loadAdmins();
  }

  openCreate(): void {
    const presetGymId = this.gymFilter.value || undefined;
    const ref = this.dialog.open(GymAdminFormDialogComponent, {
      width: '480px',
      data: presetGymId ? { presetGymId } : null,
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.loadAdmins();
      if (result.temporaryPassword) {
        this.dialog.open(TemporaryPasswordDialogComponent, {
          width: '440px',
          data: {
            title: 'Temporary Password Created',
            email: result.admin.email,
            temporaryPassword: result.temporaryPassword,
            message: result.message,
          },
        });
      } else {
        this.notify.success(result.message ?? 'Gym admin created');
      }
    });
  }

  openEdit(admin: GymAdmin): void {
    const ref = this.dialog.open(GymAdminFormDialogComponent, { width: '480px', data: admin });
    ref.afterClosed().subscribe((result) => {
      if (result) this.loadAdmins();
    });
  }

  resendPassword(admin: GymAdmin): void {
    if (!confirm(`Generate a new temporary password for ${admin.email}?`)) return;
    this.gymAdminService.resendTemporaryPassword(admin.userId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.loadAdmins();
          this.dialog.open(TemporaryPasswordDialogComponent, {
            width: '440px',
            data: {
              title: 'New Temporary Password',
              email: res.data.email,
              temporaryPassword: res.data.temporaryPassword,
              message: res.data.message,
            },
          });
        }
      },
      error: (err) => this.notify.error(err.error?.message ?? 'Failed to reset password'),
    });
  }

  deactivate(admin: GymAdmin): void {
    if (!confirm(`Deactivate gym admin "${admin.name}"?`)) return;
    this.gymAdminService.deactivate(admin.userId).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Gym admin deactivated');
          this.loadAdmins();
        }
      },
      error: () => this.notify.error('Deactivation failed'),
    });
  }

  activate(admin: GymAdmin): void {
    this.gymAdminService.activate(admin.userId).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Gym admin activated');
          this.loadAdmins();
        }
      },
      error: () => this.notify.error('Activation failed'),
    });
  }
}
