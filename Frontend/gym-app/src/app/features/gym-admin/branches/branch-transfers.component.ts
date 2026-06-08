import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { BranchService } from '../../../core/services/branch.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Branch, BranchTransfer } from '../../../shared/models/branch.models';

@Component({
  selector: 'app-branch-transfers',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, RouterLink, MatIconModule],
  templateUrl: './branch-transfers.component.html',
  styleUrl: './branch-transfers.component.css',
})
export class BranchTransfersComponent implements OnInit {
  private readonly service = inject(BranchService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  branches = signal<Branch[]>([]);
  history = signal<BranchTransfer[]>([]);
  form = this.fb.group({
    type: ['Member', Validators.required],
    entityId: [0, [Validators.required, Validators.min(1)]],
    toBranchId: [0, [Validators.required, Validators.min(1)]],
  });

  ngOnInit(): void {
    this.service.getList().subscribe({
      next: (r) => {
        if (r.success && r.data) this.branches.set(r.data);
      },
    });
    this.loadHistory();
  }

  loadHistory(): void {
    this.service.getTransfers().subscribe({
      next: (r) => {
        if (r.success && r.data) this.history.set(r.data.items);
      },
    });
  }

  transfer(): void {
    if (this.form.invalid) return;
    const v = this.form.getRawValue();
    const req =
      v.type === 'Trainer'
        ? this.service.transferTrainer({ trainerId: v.entityId!, toBranchId: v.toBranchId! })
        : this.service.transferMember({ memberId: v.entityId!, toBranchId: v.toBranchId! });
    req.subscribe({
      next: () => {
        this.notify.success('Transfer completed');
        this.loadHistory();
      },
      error: () => this.notify.error('Transfer failed'),
    });
  }
}
