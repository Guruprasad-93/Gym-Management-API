import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MembershipPlan } from '../../../shared/models/membership-payment.models';

@Component({
  selector: 'app-membership-plan-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Plan' : 'Add Plan' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline" class="full-width"><mat-label>Plan Name</mat-label><input matInput formControlName="planName" /></mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Duration (months)</mat-label><input matInput type="number" formControlName="durationInMonths" /></mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Price</mat-label><input matInput type="number" formControlName="price" /></mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Description</mat-label><textarea matInput rows="2" formControlName="description"></textarea></mat-form-field>
        @if (isEdit) { <mat-checkbox formControlName="isActive">Active</mat-checkbox> }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid" (click)="save()">Save</button>
    </mat-dialog-actions>
  `,
  styles: [`.form-grid { min-width:320px; } .full-width { width:100%; }`],
})
export class MembershipPlanFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<MembershipPlanFormDialogComponent>);
  readonly data = inject<MembershipPlan | null>(MAT_DIALOG_DATA, { optional: true });
  isEdit = false;
  form = this.fb.nonNullable.group({
    planName: ['', Validators.required],
    durationInMonths: [1, [Validators.required, Validators.min(1)]],
    price: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    isActive: [true],
  });

  ngOnInit(): void {
    this.isEdit = !!this.data;
    if (this.data) this.form.patchValue(this.data);
  }

  save(): void {
    const raw = this.form.getRawValue();
    const req = this.isEdit && this.data
      ? this.svc.updatePlan(this.data.id, {
          planName: raw.planName,
          durationInMonths: raw.durationInMonths,
          price: raw.price,
          description: raw.description || undefined,
          isActive: raw.isActive,
        })
      : this.svc.createPlan({
          planName: raw.planName,
          durationInMonths: raw.durationInMonths,
          price: raw.price,
          description: raw.description || undefined,
        });
    req.subscribe({
      next: (res) => { if (res.success) { this.notify.success('Saved'); this.ref.close(true); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Save failed'),
    });
  }
}
