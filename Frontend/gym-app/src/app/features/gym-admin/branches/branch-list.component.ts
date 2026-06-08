import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { BranchService } from '../../../core/services/branch.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Branch } from '../../../shared/models/branch.models';

@Component({
  selector: 'app-branch-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatIconModule, MatTableModule],
  templateUrl: './branch-list.component.html',
  styleUrl: './branch-list.component.css',
})
export class BranchListComponent implements OnInit {
  private readonly service = inject(BranchService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  branches = signal<Branch[]>([]);
  cols = ['name', 'code', 'members', 'manager', 'status'];
  form = this.fb.group({
    branchName: ['', Validators.required],
    branchCode: [''],
    city: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.getPaged().subscribe({
      next: (r) => {
        if (r.success && r.data) this.branches.set(r.data.items);
      },
      error: () => this.notify.error('Failed to load branches'),
    });
  }

  create(): void {
    if (this.form.invalid) return;
    this.service.create(this.form.getRawValue() as never).subscribe({
      next: () => {
        this.notify.success('Branch created');
        this.form.reset();
        this.load();
      },
      error: () => this.notify.error('Failed to create branch'),
    });
  }
}
