import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { DietService } from '../../../core/services/diet.service';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';
import { DietPlanListItem } from '../../../shared/models/diet.models';

export interface AssignDietPlanDialogData {
  dietPlanId?: number;
  planName?: string;
  memberId?: number;
}

@Component({
  selector: 'app-assign-diet-plan-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.planName ? 'Assign "' + data.planName + '"' : 'Assign diet plan' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        @if (!data.dietPlanId) {
          <mat-form-field appearance="outline">
            <mat-label>Diet plan</mat-label>
            <mat-select formControlName="dietPlanId" required>
              @for (p of plans; track p.dietPlanId) {
                <mat-option [value]="p.dietPlanId">{{ p.planName }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }
        @if (!data.memberId) {
          <mat-form-field appearance="outline">
            <mat-label>Member</mat-label>
            <mat-select formControlName="memberId" required>
              @for (m of members; track m.id) {
                <mat-option [value]="m.id">{{ m.fullName }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }
        <mat-form-field appearance="outline">
          <mat-label>Start date</mat-label>
          <input matInput type="date" formControlName="startDate" required />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>End date (optional)</mat-label>
          <input matInput type="date" formControlName="endDate" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Notes</mat-label>
          <textarea matInput rows="2" formControlName="notes"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" type="button" [disabled]="form.invalid || saving" (click)="submit()">Assign</button>
    </mat-dialog-actions>
  `,
  styles: [`.form { display: flex; flex-direction: column; min-width: 320px; } .full { width: 100%; }`],
})
export class AssignDietPlanDialogComponent {
  readonly data = inject<AssignDietPlanDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<AssignDietPlanDialogComponent>);
  private readonly fb = inject(FormBuilder);
  private readonly diet = inject(DietService);
  private readonly membersSvc = inject(MemberService);
  private readonly notify = inject(NotificationService);

  members: Member[] = [];
  plans: DietPlanListItem[] = [];
  saving = false;

  form = this.fb.group({
    dietPlanId: [this.data.dietPlanId ?? (null as number | null), Validators.required],
    memberId: [this.data.memberId ?? (null as number | null), Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    endDate: [''],
    notes: [''],
  });

  constructor() {
    if (!this.data.dietPlanId) {
      this.diet.getPlans(false).subscribe({
        next: (res) => { if (res.success && res.data) this.plans = res.data; },
      });
    }
    if (!this.data.memberId) {
      this.membersSvc.getPaged(null, { pageNumber: 1, pageSize: 200 }, false).subscribe({
        next: (res) => {
          if (res.success && res.data) this.members = res.data.items;
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const v = this.form.getRawValue();
    this.diet
      .assign({
        memberId: v.memberId!,
        dietPlanId: v.dietPlanId!,
        startDate: v.startDate!,
        endDate: v.endDate || undefined,
        notes: v.notes || undefined,
        deactivatePrevious: true,
      })
      .subscribe({
        next: (res) => {
          this.saving = false;
          if (res.success) {
            this.notify.success('Diet plan assigned');
            this.ref.close(true);
          }
        },
        error: () => {
          this.saving = false;
          this.notify.error('Assignment failed');
        },
      });
  }
}
