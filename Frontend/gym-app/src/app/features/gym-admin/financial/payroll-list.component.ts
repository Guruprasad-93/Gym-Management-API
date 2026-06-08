import { CurrencyPipe, DatePipe, NgClass } from '@angular/common';

import { Component, OnInit, inject, signal } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatIconModule } from '@angular/material/icon';

import { MatTableModule } from '@angular/material/table';

import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { PayrollService } from '../../../core/services/financial.service';

import { NotificationService } from '../../../core/services/notification.service';

import { AuthService } from '../../../core/services/auth.service';

import { Permissions } from '../../../core/constants/permissions';

import { Payroll } from '../../../shared/models/financial.models';



@Component({

  selector: 'app-payroll-list',

  standalone: true,

  imports: [

    CurrencyPipe,

    DatePipe,

    NgClass,

    ReactiveFormsModule,

    MatIconModule,

    MatTableModule,

    MatPaginatorModule,

    MatProgressSpinnerModule,

  ],

  templateUrl: './payroll-list.component.html',

  styleUrl: './payroll-list.component.css',

})

export class PayrollListComponent implements OnInit {

  private readonly payrollService = inject(PayrollService);

  private readonly notify = inject(NotificationService);

  private readonly fb = inject(FormBuilder);

  readonly auth = inject(AuthService);

  readonly permissions = Permissions;

  readonly cols = ['employee', 'month', 'net', 'status', 'actions'];



  payrolls = signal<Payroll[]>([]);

  loading = signal(true);

  total = signal(0);

  pageIndex = 0;

  pageSize = 10;

  showGenerate = false;



  genForm = this.fb.group({

    salaryMonth: [new Date().toISOString().slice(0, 7), Validators.required],

    defaultTrainerBaseSalary: [15000, Validators.required],

    defaultStaffBaseSalary: [25000, Validators.required],

  });



  get pageSummary(): string {

    const total = this.total();

    if (!total) return 'No payroll records';

    const start = this.pageIndex * this.pageSize + 1;

    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);

    return `Showing ${start}–${end} of ${total} records`;

  }



  ngOnInit(): void {

    this.load();

  }



  employeeInitials(name: string): string {

    return (name ?? '')

      .split(/\s+/)

      .filter(Boolean)

      .slice(0, 2)

      .map((part) => part[0]?.toUpperCase() ?? '')

      .join('');

  }



  statusBadgeClass(status: string): string {

    switch (status) {

      case 'Paid':

        return 'status-badge--paid';

      case 'Approved':

        return 'status-badge--approved';

      default:

        return 'status-badge--draft';

    }

  }



  load(): void {

    this.loading.set(true);

    this.payrollService.getPaged({ pageNumber: this.pageIndex + 1, pageSize: this.pageSize }).subscribe({

      next: (r) => {

        this.payrolls.set(r.data?.items ?? []);

        this.total.set(r.data?.totalCount ?? 0);

        this.loading.set(false);

      },

      error: () => this.loading.set(false),

    });

  }



  onPage(e: PageEvent): void {

    this.pageIndex = e.pageIndex;

    this.pageSize = e.pageSize;

    this.load();

  }



  generate(): void {

    const v = this.genForm.getRawValue();

    this.payrollService

      .generate({

        salaryMonth: `${v.salaryMonth}-01`,

        defaultTrainerBaseSalary: v.defaultTrainerBaseSalary!,

        defaultStaffBaseSalary: v.defaultStaffBaseSalary!,

      })

      .subscribe({

        next: (r) => {

          this.notify.success(`Generated ${r.data?.generatedCount ?? 0} records`);

          this.showGenerate = false;

          this.load();

        },

        error: (e) => this.notify.error(e?.error?.message ?? 'Generate failed'),

      });

  }



  approve(row: Payroll): void {

    this.payrollService.approve(row.id).subscribe({

      next: () => {

        this.notify.success('Approved');

        this.load();

      },

      error: () => this.notify.error('Approve failed'),

    });

  }



  pay(row: Payroll): void {

    this.payrollService.pay(row.id).subscribe({

      next: () => {

        this.notify.success('Marked paid');

        this.load();

      },

      error: () => this.notify.error('Pay failed'),

    });

  }



  exportPdf(): void {

    this.payrollService.exportPdf().subscribe({

      next: (blob) => {

        const a = document.createElement('a');

        a.href = URL.createObjectURL(blob);

        a.download = 'payroll.pdf';

        a.click();

      },

      error: () => this.notify.error('Export failed'),

    });

  }

}

