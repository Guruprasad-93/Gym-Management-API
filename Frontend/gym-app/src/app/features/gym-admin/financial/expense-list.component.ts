import { CurrencyPipe, DatePipe } from '@angular/common';

import { Component, OnInit, inject, signal } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatIconModule } from '@angular/material/icon';

import { MatTableModule } from '@angular/material/table';

import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ExpenseService } from '../../../core/services/financial.service';

import { NotificationService } from '../../../core/services/notification.service';

import { AuthService } from '../../../core/services/auth.service';

import { Permissions } from '../../../core/constants/permissions';

import { EXPENSE_PAYMENT_METHODS, Expense, ExpenseCategory } from '../../../shared/models/financial.models';



@Component({

  selector: 'app-expense-list',

  standalone: true,

  imports: [

    CurrencyPipe,

    DatePipe,

    ReactiveFormsModule,

    MatIconModule,

    MatTableModule,

    MatPaginatorModule,

    MatProgressSpinnerModule,

  ],

  templateUrl: './expense-list.component.html',

  styleUrl: './expense-list.component.css',

})

export class ExpenseListComponent implements OnInit {

  private readonly expenseService = inject(ExpenseService);

  private readonly notify = inject(NotificationService);

  private readonly fb = inject(FormBuilder);

  readonly auth = inject(AuthService);

  readonly permissions = Permissions;

  readonly methods = EXPENSE_PAYMENT_METHODS;

  readonly cols = ['date', 'category', 'amount', 'vendor', 'method', 'actions'];



  expenses = signal<Expense[]>([]);

  categories = signal<ExpenseCategory[]>([]);

  loading = signal(true);

  total = signal(0);

  pageIndex = 0;

  pageSize = 10;

  showForm = false;

  editing: Expense | null = null;



  form = this.fb.group({

    categoryId: [null as number | null, Validators.required],

    amount: [0, [Validators.required, Validators.min(0.01)]],

    expenseDate: [new Date().toISOString().slice(0, 10), Validators.required],

    paymentMethod: ['Cash', Validators.required],

    vendorName: [''],

    description: [''],

    attachmentFileId: [null as number | null],

  });



  get pageSummary(): string {

    const total = this.total();

    if (!total) return 'No expenses';

    const start = this.pageIndex * this.pageSize + 1;

    const end = Math.min((this.pageIndex + 1) * this.pageSize, total);

    return `Showing ${start}–${end} of ${total} expenses`;

  }



  ngOnInit(): void {

    this.expenseService.getCategories().subscribe((r) => this.categories.set(r.data ?? []));

    this.load();

  }



  load(): void {

    this.loading.set(true);

    this.expenseService.getPaged({ pageNumber: this.pageIndex + 1, pageSize: this.pageSize }).subscribe({

      next: (r) => {

        this.expenses.set(r.data?.items ?? []);

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



  openForm(row?: Expense): void {

    this.editing = row ?? null;

    if (row) {

      this.form.patchValue({

        categoryId: row.categoryId,

        amount: row.amount,

        expenseDate: row.expenseDate.slice(0, 10),

        paymentMethod: row.paymentMethod,

        vendorName: row.vendorName ?? '',

        description: row.description ?? '',

        attachmentFileId: row.attachmentFileId ?? null,

      });

    } else {

      this.form.reset({

        paymentMethod: 'Cash',

        expenseDate: new Date().toISOString().slice(0, 10),

        amount: 0,

      });

    }

    this.showForm = true;

  }



  save(): void {

    if (this.form.invalid) return;

    const v = this.form.getRawValue();

    const payload = {

      categoryId: v.categoryId!,

      amount: v.amount!,

      expenseDate: v.expenseDate!,

      paymentMethod: v.paymentMethod!,

      vendorName: v.vendorName || undefined,

      description: v.description || undefined,

      attachmentFileId: v.attachmentFileId ?? undefined,

    };

    const req = this.editing

      ? this.expenseService.update(this.editing.id, payload)

      : this.expenseService.create(payload);

    req.subscribe({

      next: () => {

        this.notify.success('Saved');

        this.showForm = false;

        this.load();

      },

      error: (e) => this.notify.error(e?.error?.message ?? 'Save failed'),

    });

  }



  remove(row: Expense): void {

    if (!confirm('Delete this expense?')) return;

    this.expenseService.delete(row.id).subscribe({

      next: () => {

        this.notify.success('Deleted');

        this.load();

      },

      error: () => this.notify.error('Delete failed'),

    });

  }



  exportExcel(): void {

    this.expenseService.exportExcel().subscribe({

      next: (blob) => this.download(blob, 'expenses.xlsx'),

      error: () => this.notify.error('Export failed'),

    });

  }



  private download(blob: Blob, name: string): void {

    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');

    a.href = url;

    a.download = name;

    a.click();

    URL.revokeObjectURL(url);

  }

}

