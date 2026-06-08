import { Component, OnInit, ViewChild, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RoleService } from '../../../core/services/role.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Role } from '../../../shared/models/role.models';
import { RoleFormDialogComponent } from './role-form-dialog.component';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.css',
})
export class RoleListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly roleService = inject(RoleService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['roleName', 'description', 'system', 'actions'];
  dataSource = new MatTableDataSource<Role>([]);
  searchControl = this.fb.nonNullable.control('');
  typeControl = this.fb.nonNullable.control<'all' | 'system' | 'custom'>('all');

  get pageSummary(): string {
    const total = this.dataSource.filteredData.length;
    if (!total || !this.paginator) return total ? `${total} roles` : 'No roles';

    const start = this.paginator.pageIndex * this.paginator.pageSize + 1;
    const end = Math.min((this.paginator.pageIndex + 1) * this.paginator.pageSize, total);
    return `Showing ${start}-${end} of ${total} roles`;
  }

  ngOnInit(): void {
    this.dataSource.filterPredicate = (data, filter) => {
      const parsed = JSON.parse(filter) as { text: string; type: 'all' | 'system' | 'custom' };
      const haystack = `${data.roleName} ${data.description ?? ''}`.toLowerCase();
      const matchesText = !parsed.text || haystack.includes(parsed.text);
      const matchesType =
        parsed.type === 'all' ||
        (parsed.type === 'system' && data.isSystemRole) ||
        (parsed.type === 'custom' && !data.isSystemRole);
      return matchesText && matchesType;
    };

    this.load();
    this.searchControl.valueChanges.subscribe(() => this.applyFilter());
    this.typeControl.valueChanges.subscribe(() => this.applyFilter());
  }

  load(): void {
    this.roleService.getAll().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.dataSource.data = res.data;
          setTimeout(() => {
            this.dataSource.paginator = this.paginator;
            this.dataSource.sort = this.sort;
            this.paginator?.page.subscribe(() => this.cdr.markForCheck());
          });
        }
      },
      error: () => this.notify.error('Failed to load roles'),
    });
  }

  openForm(role?: Role): void {
    const ref = this.dialog.open(RoleFormDialogComponent, { width: '420px', data: role ?? null });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  deleteRole(role: Role): void {
    if (!confirm(`Delete role "${role.roleName}"?`)) return;
    this.roleService.delete(role.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Role deleted');
          this.load();
        }
      },
      error: (err) => this.notify.error(err.error?.message ?? 'Delete failed'),
    });
  }

  onPageChange(): void {
    this.cdr.markForCheck();
  }

  private applyFilter(): void {
    this.dataSource.filter = JSON.stringify({
      text: this.searchControl.value.trim().toLowerCase(),
      type: this.typeControl.value,
    });
    this.paginator?.firstPage();
    this.cdr.markForCheck();
  }
}
