import { Component, OnInit, ViewChild, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PrivilegeService } from '../../../core/services/privilege.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Privilege } from '../../../shared/models/role.models';
import { PrivilegeFormDialogComponent } from './privilege-form-dialog.component';

@Component({
  selector: 'app-privilege-list',
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
  templateUrl: './privilege-list.component.html',
  styleUrl: './privilege-list.component.css',
})
export class PrivilegeListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly privilegeService = inject(PrivilegeService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['privilegeName', 'category', 'description', 'actions'];
  dataSource = new MatTableDataSource<Privilege>([]);
  searchControl = this.fb.nonNullable.control('');
  categoryControl = this.fb.nonNullable.control('all');
  categories: string[] = [];

  get pageSummary(): string {
    const total = this.dataSource.filteredData.length;
    if (!total || !this.paginator) return total ? `${total} privileges` : 'No privileges';

    const start = this.paginator.pageIndex * this.paginator.pageSize + 1;
    const end = Math.min((this.paginator.pageIndex + 1) * this.paginator.pageSize, total);
    return `Showing ${start}-${end} of ${total} privileges`;
  }

  ngOnInit(): void {
    this.dataSource.filterPredicate = (data, filter) => {
      const parsed = JSON.parse(filter) as { text: string; category: string };
      const haystack = `${data.privilegeName} ${data.category} ${data.description ?? ''}`.toLowerCase();
      const matchesText = !parsed.text || haystack.includes(parsed.text);
      const matchesCategory = parsed.category === 'all' || data.category === parsed.category;
      return matchesText && matchesCategory;
    };

    this.load();
    this.searchControl.valueChanges.subscribe(() => this.applyFilter());
    this.categoryControl.valueChanges.subscribe(() => this.applyFilter());
  }

  load(): void {
    this.privilegeService.getAll().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.dataSource.data = res.data;
          this.categories = [...new Set(res.data.map((p) => p.category).filter(Boolean))].sort();
          setTimeout(() => {
            this.dataSource.paginator = this.paginator;
            this.dataSource.sort = this.sort;
            this.paginator?.page.subscribe(() => this.cdr.markForCheck());
          });
        }
      },
      error: () => this.notify.error('Failed to load privileges'),
    });
  }

  openForm(): void {
    const ref = this.dialog.open(PrivilegeFormDialogComponent, { width: '420px' });
    ref.afterClosed().subscribe((ok) => ok && this.load());
  }

  remove(p: Privilege): void {
    if (!confirm(`Delete privilege "${p.privilegeName}"?`)) return;
    this.privilegeService.delete(p.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Deleted');
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
      category: this.categoryControl.value,
    });
    this.paginator?.firstPage();
    this.cdr.markForCheck();
  }
}
