import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { BranchService } from '../../../core/services/branch.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Branch, BranchTarget } from '../../../shared/models/branch.models';

@Component({
  selector: 'app-branch-targets-page',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './branch-targets.component.html',
  styleUrl: './branch-targets.component.css',
})
export class BranchTargetsComponent implements OnInit {
  readonly Math = Math;

  private readonly service = inject(BranchService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  branches = signal<Branch[]>([]);
  targets = signal<BranchTarget[]>([]);
  form = this.fb.group({
    branchId: [0, [Validators.required, Validators.min(1)]],
    targetMonth: [new Date().toISOString().slice(0, 7), Validators.required],
    revenueTarget: [100000, Validators.required],
    newMembersTarget: [20, Validators.required],
    leadConversionsTarget: [10, Validators.required],
  });

  ngOnInit(): void {
    this.service.getList().subscribe({
      next: (r) => {
        if (r.success && r.data) this.branches.set(r.data);
      },
    });
    this.load();
  }

  load(): void {
    this.service.getTargets().subscribe({
      next: (r) => {
        if (r.success && r.data) this.targets.set(r.data);
      },
    });
  }

  save(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    this.service
      .upsertTarget({
        branchId: v.branchId!,
        targetMonth: `${v.targetMonth}-01`,
        revenueTarget: v.revenueTarget!,
        newMembersTarget: v.newMembersTarget!,
        leadConversionsTarget: v.leadConversionsTarget!,
      })
      .subscribe({
        next: () => {
          this.notify.success('Target saved');
          this.load();
        },
        error: () => this.notify.error('Failed to save target'),
      });
  }
}
