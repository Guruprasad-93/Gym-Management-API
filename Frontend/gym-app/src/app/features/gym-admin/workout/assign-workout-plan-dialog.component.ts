import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { WorkoutService } from '../../../core/services/workout.service';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';
import { WorkoutPlanListItem } from '../../../shared/models/workout.models';

@Component({
  selector: 'app-assign-workout-plan-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data.planName ? 'Assign "' + data.planName + '"' : 'Assign workout plan' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        @if (!data.workoutPlanId) {
          <mat-form-field appearance="outline" class="full">
            <mat-label>Plan</mat-label>
            <mat-select formControlName="workoutPlanId" required>
              @for (p of plans; track p.workoutPlanId) { <mat-option [value]="p.workoutPlanId">{{ p.planName }}</mat-option> }
            </mat-select>
          </mat-form-field>
        }
        @if (!data.memberId) {
          <mat-form-field appearance="outline" class="full">
            <mat-label>Member</mat-label>
            <mat-select formControlName="memberId" required>
              @for (m of members; track m.id) { <mat-option [value]="m.id">{{ m.fullName }}</mat-option> }
            </mat-select>
          </mat-form-field>
        }
        <mat-form-field appearance="outline"><mat-label>Start</mat-label><input matInput type="date" formControlName="startDate" required /></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>End (optional)</mat-label><input matInput type="date" formControlName="endDate" /></mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || saving" (click)="submit()">Assign</button>
    </mat-dialog-actions>
  `,
  styles: [`.form { display: flex; flex-direction: column; min-width: 320px; } .full { width: 100%; }`],
})
export class AssignWorkoutPlanDialogComponent {
  readonly data = inject<{ workoutPlanId?: number; planName?: string; memberId?: number }>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<AssignWorkoutPlanDialogComponent>);
  private readonly fb = inject(FormBuilder);
  private readonly workout = inject(WorkoutService);
  private readonly membersSvc = inject(MemberService);
  private readonly notify = inject(NotificationService);
  members: Member[] = [];
  plans: WorkoutPlanListItem[] = [];
  saving = false;

  form = this.fb.group({
    workoutPlanId: [this.data.workoutPlanId ?? (null as number | null), Validators.required],
    memberId: [this.data.memberId ?? (null as number | null), Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    endDate: [''],
  });

  constructor() {
    if (!this.data.workoutPlanId) {
      this.workout.getPlans(false).subscribe({ next: (r) => r.success && r.data && (this.plans = r.data) });
    }
    if (!this.data.memberId) {
      this.membersSvc.getPaged(null, { pageNumber: 1, pageSize: 200 }).subscribe({
        next: (r) => { if (r.success && r.data) this.members = r.data.items; },
      });
    }
  }

  submit(): void {
    const v = this.form.getRawValue();
    this.saving = true;
    this.workout.assign({
      memberId: v.memberId!,
      workoutPlanId: v.workoutPlanId!,
      startDate: v.startDate!,
      endDate: v.endDate || undefined,
    }).subscribe({
      next: () => { this.saving = false; this.notify.success('Assigned'); this.ref.close(true); },
      error: () => { this.saving = false; this.notify.error('Assign failed'); },
    });
  }
}
