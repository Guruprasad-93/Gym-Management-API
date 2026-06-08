import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { WorkoutService } from '../../../core/services/workout.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Exercise, ExerciseCategory, DIFFICULTIES } from '../../../shared/models/workout.models';

@Component({
  selector: 'app-exercise-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatCheckboxModule],
  template: `
    <h2 mat-dialog-title>{{ data.exercise ? 'Edit' : 'New' }} Exercise</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form">
        <mat-form-field appearance="outline" class="full"><mat-label>Name</mat-label><input matInput formControlName="exerciseName" required /></mat-form-field>
        <mat-form-field appearance="outline" class="full">
          <mat-label>Category</mat-label>
          <mat-select formControlName="exerciseCategoryId">
            <mat-option [value]="null">None</mat-option>
            @for (c of data.categories; track c.exerciseCategoryId) {
              <mat-option [value]="c.exerciseCategoryId">{{ c.categoryName }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline"><mat-label>Muscle group</mat-label><input matInput formControlName="muscleGroup" /></mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Difficulty</mat-label>
          <mat-select formControlName="difficulty">
            <mat-option value="">—</mat-option>
            @for (d of difficulties; track d) { <mat-option [value]="d">{{ d }}</mat-option> }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full"><mat-label>Instructions</mat-label><textarea matInput rows="3" formControlName="instructions"></textarea></mat-form-field>
        <mat-checkbox formControlName="isActive">Active</mat-checkbox>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" [disabled]="form.invalid || saving" (click)="save()">Save</button>
    </mat-dialog-actions>
  `,
  styles: [`.form { display: flex; flex-direction: column; min-width: 400px; } .full { width: 100%; }`],
})
export class ExerciseFormDialogComponent {
  readonly data = inject<{ exercise?: Exercise; categories: ExerciseCategory[] }>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<ExerciseFormDialogComponent>);
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(WorkoutService);
  private readonly notify = inject(NotificationService);
  readonly difficulties = DIFFICULTIES;
  saving = false;

  form = this.fb.group({
    exerciseName: [this.data.exercise?.exerciseName ?? '', Validators.required],
    exerciseCategoryId: [this.data.exercise?.exerciseCategoryId ?? null],
    muscleGroup: [this.data.exercise?.muscleGroup ?? ''],
    difficulty: [this.data.exercise?.difficulty ?? ''],
    instructions: [this.data.exercise?.instructions ?? ''],
    isActive: [this.data.exercise?.isActive ?? true],
  });

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const v = this.form.getRawValue();
    const payload = {
      exerciseName: v.exerciseName ?? undefined,
      exerciseCategoryId: v.exerciseCategoryId ?? undefined,
      muscleGroup: v.muscleGroup || undefined,
      difficulty: v.difficulty || undefined,
      instructions: v.instructions || undefined,
      isActive: v.isActive ?? undefined,
    };
    const req = this.data.exercise
      ? this.svc.updateExercise(this.data.exercise.exerciseId, payload)
      : this.svc.createExercise(payload);
    req.subscribe({
      next: () => { this.saving = false; this.notify.success('Saved'); this.ref.close(true); },
      error: () => { this.saving = false; this.notify.error('Save failed'); },
    });
  }
}
