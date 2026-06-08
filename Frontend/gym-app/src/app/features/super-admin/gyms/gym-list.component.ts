import { Component, OnInit, ViewChild, inject, signal, ChangeDetectorRef } from '@angular/core';

import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';

import { MatSort, MatSortModule } from '@angular/material/sort';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { MatButtonModule } from '@angular/material/button';

import { MatIconModule } from '@angular/material/icon';

import { GymService } from '../../../core/services/gym.service';

import { NotificationService } from '../../../core/services/notification.service';

import { AuthService } from '../../../core/services/auth.service';

import { Permissions } from '../../../core/constants/permissions';

import { Gym } from '../../../shared/models/gym.models';

import { RouterModule } from '@angular/router';

import { GymFormDialogComponent } from './gym-form-dialog.component';



@Component({

  selector: 'app-gym-list',

  standalone: true,

  imports: [

    ReactiveFormsModule,

    MatTableModule,

    MatPaginatorModule,

    MatSortModule,

    MatDialogModule,

    MatButtonModule,

    MatIconModule,

    RouterModule,

  ],

  templateUrl: './gym-list.component.html',

  styleUrl: './gym-list.component.css',

})

export class GymListComponent implements OnInit {

  readonly auth = inject(AuthService);

  readonly permissions = Permissions;

  private readonly gymService = inject(GymService);

  private readonly notify = inject(NotificationService);

  private readonly dialog = inject(MatDialog);

  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);



  @ViewChild(MatPaginator) paginator!: MatPaginator;

  @ViewChild(MatSort) sort!: MatSort;



  displayedColumns = ['name', 'email', 'phone', 'status', 'actions'];

  dataSource = new MatTableDataSource<Gym>([]);

  searchControl = this.fb.nonNullable.control('');

  statusControl = this.fb.nonNullable.control<'all' | 'active' | 'inactive'>('all');

  private gyms = signal<Gym[]>([]);



  canCreate = () => this.auth.hasPermission(Permissions.CreateGym);

  canUpdate = () => this.auth.hasPermission(Permissions.UpdateGym);

  canDelete = () => this.auth.hasPermission(Permissions.DeleteGym);

  canActivate = () => this.auth.hasPermission(Permissions.ActivateGym);

  canDeactivate = () => this.auth.hasPermission(Permissions.DeactivateGym);



  get pageSummary(): string {

    const total = this.dataSource.filteredData.length;

    if (!total || !this.paginator) return total ? `${total} gyms` : 'No gyms';



    const start = this.paginator.pageIndex * this.paginator.pageSize + 1;

    const end = Math.min((this.paginator.pageIndex + 1) * this.paginator.pageSize, total);

    return `Showing ${start}-${end} of ${total} gyms`;

  }



  ngOnInit(): void {

    this.loadGyms();

    this.dataSource.filterPredicate = (data, filter) => {

      const parsed = JSON.parse(filter) as { text: string; status: 'all' | 'active' | 'inactive' };

      const matchesText =

        !parsed.text ||

        [data.name, data.email, data.phone, data.address]

          .filter(Boolean)

          .some((field) => field!.toLowerCase().includes(parsed.text));

      const matchesStatus =

        parsed.status === 'all' ||

        (parsed.status === 'active' && data.isActive) ||

        (parsed.status === 'inactive' && !data.isActive);

      return matchesText && matchesStatus;

    };



    this.searchControl.valueChanges.subscribe(() => this.applyFilter());

    this.statusControl.valueChanges.subscribe(() => this.applyFilter());

  }



  gymInitials(name: string): string {

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

        if (res.success && res.data) {

          this.gyms.set(res.data);

          this.dataSource.data = res.data;

          setTimeout(() => {

            this.dataSource.paginator = this.paginator;

            this.dataSource.sort = this.sort;

            this.paginator?.page.subscribe(() => this.cdr.markForCheck());

          });

        }

      },

      error: () => this.notify.error('Failed to load gyms'),

    });

  }



  openForm(gym?: Gym): void {

    const ref = this.dialog.open(GymFormDialogComponent, {

      width: '480px',

      data: gym ?? null,

    });

    ref.afterClosed().subscribe((saved) => {

      if (saved) this.loadGyms();

    });

  }



  toggleActive(gym: Gym, active: boolean): void {

    const req = active ? this.gymService.activate(gym.id) : this.gymService.deactivate(gym.id);

    req.subscribe({

      next: (res) => {

        if (res.success) {

          this.notify.success(active ? 'Gym activated' : 'Gym deactivated');

          this.loadGyms();

        }

      },

      error: () => this.notify.error('Action failed'),

    });

  }



  deleteGym(gym: Gym): void {

    if (!confirm(`Delete gym "${gym.name}"?`)) return;

    this.gymService.delete(gym.id).subscribe({

      next: (res) => {

        if (res.success) {

          this.notify.success('Gym deleted');

          this.loadGyms();

        }

      },

      error: () => this.notify.error('Delete failed'),

    });

  }



  private applyFilter(): void {

    this.dataSource.filter = JSON.stringify({

      text: this.searchControl.value.trim().toLowerCase(),

      status: this.statusControl.value,

    });

    this.paginator?.firstPage();
    this.cdr.markForCheck();
  }

  onPageChange(): void {
    this.cdr.markForCheck();
  }
}
