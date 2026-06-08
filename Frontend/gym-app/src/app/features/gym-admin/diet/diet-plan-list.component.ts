import { Component, OnInit, inject, signal } from '@angular/core';

import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { RouterModule } from '@angular/router';

import { MatTableModule } from '@angular/material/table';

import { MatIconModule } from '@angular/material/icon';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DietService } from '../../../core/services/diet.service';

import { NotificationService } from '../../../core/services/notification.service';

import { AuthService } from '../../../core/services/auth.service';

import { Permissions } from '../../../core/constants/permissions';

import { DietCategory, DietPlanListItem } from '../../../shared/models/diet.models';

import { AssignDietPlanDialogComponent } from './assign-diet-plan-dialog.component';



@Component({

  selector: 'app-diet-plan-list',

  standalone: true,

  imports: [

    ReactiveFormsModule,

    RouterModule,

    MatTableModule,

    MatIconModule,

    MatProgressSpinnerModule,

    MatDialogModule,

  ],

  templateUrl: './diet-plan-list.component.html',

  styleUrl: './diet-plan-list.component.css',

})

export class DietPlanListComponent implements OnInit {

  readonly auth = inject(AuthService);

  readonly permissions = Permissions;

  private readonly svc = inject(DietService);

  private readonly notify = inject(NotificationService);

  private readonly dialog = inject(MatDialog);



  loading = signal(true);

  plans = signal<DietPlanListItem[]>([]);

  categories = signal<DietCategory[]>([]);

  cols = ['planName', 'items', 'calories', 'status', 'actions'];



  searchControl = new FormControl('');

  categoryControl = new FormControl('');

  includeInactiveControl = new FormControl(false);



  ngOnInit(): void {

    this.svc.getCategories().subscribe({

      next: (res) => {

        if (res.success && res.data) this.categories.set(res.data);

      },

    });

    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => this.load());

    this.categoryControl.valueChanges.subscribe(() => this.load());

    this.includeInactiveControl.valueChanges.subscribe(() => this.load());

    this.load();

  }



  load(): void {

    this.loading.set(true);

    const cat = this.categoryControl.value;

    this.svc

      .getPlans(!!this.includeInactiveControl.value, cat ? Number(cat) : undefined, this.searchControl.value || undefined)

      .subscribe({

        next: (res) => {

          this.loading.set(false);

          if (res.success && res.data) this.plans.set(res.data);

        },

        error: () => {

          this.loading.set(false);

          this.notify.error('Failed to load diet plans');

        },

      });

  }



  openAssign(plan: DietPlanListItem): void {

    this.dialog

      .open(AssignDietPlanDialogComponent, { width: '480px', data: { dietPlanId: plan.dietPlanId, planName: plan.planName } })

      .afterClosed()

      .subscribe((ok) => ok && this.load());

  }



  clone(plan: DietPlanListItem): void {

    this.svc.clone(plan.dietPlanId).subscribe({

      next: (res) => {

        if (res.success) {

          this.notify.success('Plan cloned');

          this.load();

        }

      },

      error: () => this.notify.error('Clone failed'),

    });

  }



  remove(plan: DietPlanListItem): void {

    if (!confirm(`Delete diet plan "${plan.planName}"?`)) return;

    this.svc.delete(plan.dietPlanId).subscribe({

      next: (res) => {

        if (res.success) {

          this.notify.success('Plan deleted');

          this.load();

        }

      },

      error: (err) => this.notify.error(err?.error?.message || 'Delete failed'),

    });

  }



  exportPdf(plan: DietPlanListItem): void {

    this.svc.downloadPdf(plan.dietPlanId).subscribe({

      next: (blob) => this.saveBlob(blob, `diet-plan-${plan.dietPlanId}.pdf`),

      error: () => this.notify.error('PDF export failed'),

    });

  }



  private saveBlob(blob: Blob, name: string): void {

    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');

    a.href = url;

    a.download = name;

    a.click();

    URL.revokeObjectURL(url);

  }

}

