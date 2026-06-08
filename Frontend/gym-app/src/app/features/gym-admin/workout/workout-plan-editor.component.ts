import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WorkoutService } from '../../../core/services/workout.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { Exercise } from '../../../shared/models/workout.models';

@Component({
  selector: 'app-workout-plan-editor',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatIconModule, MatCheckboxModule, MatProgressSpinnerModule, PageHeaderComponent,
  ],
  template: `
    <app-page-header [title]="isEdit ? 'Edit Workout Plan' : 'New Workout Plan'" subtitle="Days, sets, reps, and rest">
      <button mat-stroked-button routerLink="/gym-admin/workout-plans">Back</button>
    </app-page-header>
    @if (loading()) { <mat-spinner /> } @else {
      <form class="editor" [formGroup]="form" (ngSubmit)="save()">
        <div class="header-fields">
          <mat-form-field appearance="outline"><mat-label>Plan name</mat-label><input matInput formControlName="planName" required /></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Goal</mat-label><input matInput formControlName="goal" /></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>Duration (weeks)</mat-label><input matInput type="number" formControlName="durationWeeks" /></mat-form-field>
          <mat-checkbox formControlName="isActive">Active</mat-checkbox>
        </div>
        <mat-form-field appearance="outline" class="full"><mat-label>Description</mat-label><textarea matInput rows="2" formControlName="description"></textarea></mat-form-field>
        <div class="items-header">
          <h3>Exercises</h3>
          <button mat-stroked-button type="button" (click)="addRow()"><mat-icon>add</mat-icon> Add row</button>
        </div>
        <div formArrayName="exercises">
          @for (row of exercises.controls; track $index; let i = $index) {
            <div class="row" [formGroupName]="i">
              <mat-form-field appearance="outline"><mat-label>Day</mat-label><input matInput type="number" formControlName="dayNumber" min="1" /></mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Exercise</mat-label>
                <mat-select formControlName="exerciseId" required>
                  @for (e of library(); track e.exerciseId) {
                    <mat-option [value]="e.exerciseId">{{ e.exerciseName }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Sets</mat-label><input matInput type="number" formControlName="sets" /></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Reps</mat-label><input matInput formControlName="reps" /></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Weight</mat-label><input matInput formControlName="weight" /></mat-form-field>
              <mat-form-field appearance="outline"><mat-label>Rest (s)</mat-label><input matInput type="number" formControlName="restSeconds" /></mat-form-field>
              <button mat-icon-button type="button" color="warn" (click)="removeRow(i)"><mat-icon>delete</mat-icon></button>
            </div>
          }
        </div>
        <button mat-flat-button color="primary" type="submit" [disabled]="saving() || form.invalid">Save</button>
      </form>
    }
  `,
  styles: [
    `
      .editor { background: #fff; padding: 1.5rem; border-radius: 8px; }
      .header-fields { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; align-items: center; }
      .full { width: 100%; }
      .row { display: grid; grid-template-columns: 70px 1.5fr repeat(4, 90px) 40px; gap: 0.5rem; margin-bottom: 0.5rem; align-items: start; }
      .items-header { display: flex; justify-content: space-between; margin: 1rem 0; }
    `,
  ],
})
export class WorkoutPlanEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly svc = inject(WorkoutService);
  private readonly notify = inject(NotificationService);
  loading = signal(false);
  saving = signal(false);
  library = signal<Exercise[]>([]);
  isEdit = false;
  planId: number | null = null;

  form = this.fb.group({
    planName: ['', Validators.required],
    description: [''],
    goal: [''],
    durationWeeks: [null as number | null],
    isActive: [true],
    exercises: this.fb.array([]),
  });

  get exercises(): FormArray { return this.form.get('exercises') as FormArray; }

  ngOnInit(): void {
    this.svc.getExercises(false).subscribe({ next: (r) => r.success && r.data && this.library.set(r.data) });
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit = true;
      this.planId = Number(id);
      this.loading.set(true);
      this.svc.getById(this.planId).subscribe({
        next: (r) => { this.loading.set(false); if (r.success && r.data) this.patch(r.data); },
        error: () => { this.loading.set(false); this.notify.error('Load failed'); },
      });
    } else {
      this.addRow();
    }
  }

  addRow(): void {
    this.exercises.push(this.fb.group({
      dayNumber: [1, Validators.required],
      exerciseId: [null as number | null, Validators.required],
      sets: [3],
      reps: ['10'],
      weight: [''],
      restSeconds: [60],
      notes: [''],
      sortOrder: [this.exercises.length],
    }));
  }

  removeRow(i: number): void { this.exercises.removeAt(i); }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const raw = this.form.getRawValue();
    const payload = {
      planName: raw.planName!,
      description: raw.description || undefined,
      goal: raw.goal || undefined,
      durationWeeks: raw.durationWeeks ?? undefined,
      isActive: raw.isActive ?? true,
      exercises: (raw.exercises as Array<{
        dayNumber: number | null;
        exerciseId: number | null;
        sets: number | null;
        reps: string | null;
        weight: string | null;
        restSeconds: number | null;
        notes: string | null;
      }>).map((ex, idx) => ({
        dayNumber: Number(ex.dayNumber),
        exerciseId: Number(ex.exerciseId),
        sets: ex.sets ?? undefined,
        reps: ex.reps || undefined,
        weight: ex.weight || undefined,
        restSeconds: ex.restSeconds ?? undefined,
        notes: ex.notes || undefined,
        sortOrder: idx,
      })),
    };
    const req = this.isEdit && this.planId ? this.svc.update(this.planId, payload) : this.svc.create(payload);
    req.subscribe({
      next: () => { this.saving.set(false); this.notify.success('Saved'); this.router.navigate(['/gym-admin/workout-plans']); },
      error: () => { this.saving.set(false); this.notify.error('Save failed'); },
    });
  }

  private patch(plan: { planName: string; description?: string; goal?: string; durationWeeks?: number; isActive: boolean; exercises: unknown[] }): void {
    this.form.patchValue({ planName: plan.planName, description: plan.description ?? '', goal: plan.goal ?? '', durationWeeks: plan.durationWeeks ?? null, isActive: plan.isActive });
    (plan.exercises as { dayNumber: number; exerciseId: number; sets?: number; reps?: string; weight?: string; restSeconds?: number; notes?: string; sortOrder: number }[]).forEach((ex) => {
      this.exercises.push(this.fb.group({
        dayNumber: [ex.dayNumber], exerciseId: [ex.exerciseId], sets: [ex.sets ?? 3], reps: [ex.reps ?? ''],
        weight: [ex.weight ?? ''], restSeconds: [ex.restSeconds ?? 60], notes: [ex.notes ?? ''], sortOrder: [ex.sortOrder],
      }));
    });
  }
}
