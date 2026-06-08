import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { WorkoutService } from '../../../core/services/workout.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { Roles } from '../../../core/constants/roles';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { MemberWorkoutView, WorkoutPlanExercise } from '../../../shared/models/workout.models';
import { AssignWorkoutPlanDialogComponent } from './assign-workout-plan-dialog.component';
import { MemberFilesGalleryComponent } from '../../../shared/components/member-files-gallery/member-files-gallery.component';
import { FileCategories } from '../../../shared/models/file.models';

@Component({
  selector: 'app-member-workout-view',
  standalone: true,
  imports: [
    DatePipe, RouterModule, MatButtonModule, MatIconModule, MatCheckboxModule, MatFormFieldModule,
    MatInputModule, MatProgressSpinnerModule, MatDialogModule, FormsModule, PageHeaderComponent,
    MemberFilesGalleryComponent,
  ],
  template: `
    <app-page-header [title]="workout?.planName || 'Workout Plan'" [subtitle]="workout?.memberName || (trainerMode() ? 'Trainer view' : '')">
      @if (memberId && canAssign()) {
        <button mat-stroked-button type="button" (click)="openAssign()"><mat-icon>swap_horiz</mat-icon> Change plan</button>
      }
    </app-page-header>

    @if (loading()) { <mat-spinner /> }
    @else if (!workout?.workoutPlanId) {
      <p>No active workout plan.</p>
      @if (memberId && canAssign()) {
        <button mat-flat-button color="primary" (click)="openAssign()">Assign plan</button>
      }
    } @else {
      <div class="meta">
        <span>Progress: {{ workout?.overallCompletionPercentage }}%</span>
        @if (workout?.goal) { <span>Goal: {{ workout?.goal }}</span> }
        @if (workout?.startDate) { <span>From: {{ workout?.startDate | date }}</span> }
      </div>
      @for (day of days(); track day) {
        <h3>Day {{ day }}</h3>
        <table class="tbl">
          <thead><tr><th>Exercise</th><th>Sets×Reps</th><th>Done</th><th>%</th><th>Notes</th></tr></thead>
          <tbody>
            @for (ex of exercisesForDay(day); track ex.workoutPlanExerciseId) {
              <tr>
                <td>{{ ex.exerciseName }}</td>
                <td>{{ ex.sets }}×{{ ex.reps }} &#64; {{ ex.weight || '—' }}</td>
                <td><mat-checkbox [checked]="!!ex.isCompleted" (change)="toggleComplete(ex, $event.checked)" /></td>
                <td><input type="number" min="0" max="100" class="pct" [ngModel]="ex.completionPercentage ?? 0" (ngModelChange)="updatePct(ex, $event)" /></td>
                <td>
                  @if (isTrainer()) {
                    <input class="note" placeholder="Trainer notes" [ngModel]="ex.trainerNotes" (blur)="saveNotes(ex, 'trainer', $any($event.target).value)" />
                  } @else {
                    <input class="note" placeholder="My notes" [ngModel]="ex.memberNotes" (blur)="saveNotes(ex, 'member', $any($event.target).value)" />
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
      @if (resolvedMemberId()) {
        <h3>Attachments</h3>
        <app-member-files-gallery
          [memberId]="resolvedMemberId()!"
          [category]="fileCategories.WorkoutAttachment"
          [assignedWorkoutPlanId]="workout?.assignedWorkoutPlanId ?? undefined"
          uploadLabel="Upload attachment"
          accept="image/*,.pdf,.doc,.docx,.txt" />
      }
    }
  `,
  styles: [
    `
      .meta { display: flex; gap: 1rem; margin-bottom: 1rem; color: #555; }
      .tbl { width: 100%; background: #fff; border-collapse: collapse; margin-bottom: 1.5rem; }
      .tbl th, .tbl td { padding: 0.5rem; border-bottom: 1px solid #eee; text-align: left; }
      .pct { width: 56px; }
      .note { width: 100%; min-width: 120px; }
    `,
  ],
})
export class MemberWorkoutViewComponent implements OnInit {
  readonly trainerMode = input(false);
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  readonly fileCategories = FileCategories;
  private readonly route = inject(ActivatedRoute);
  private readonly svc = inject(WorkoutService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  loading = signal(true);
  workout: MemberWorkoutView | null = null;
  memberId: number | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')
      ?? this.route.snapshot.paramMap.get('memberId');
    if (!id) {
      this.svc.getMyWorkout().subscribe({ next: (r) => this.done(r.data), error: () => this.fail() });
      return;
    }
    this.memberId = Number(id);
    this.svc.getMemberWorkout(this.memberId).subscribe({ next: (r) => this.done(r.data), error: () => this.fail() });
  }

  resolvedMemberId(): number | null {
    return this.memberId ?? this.workout?.memberId ?? null;
  }

  days(): number[] {
    const set = new Set((this.workout?.exercises ?? []).map((e) => e.dayNumber));
    return [...set].sort((a, b) => a - b);
  }

  exercisesForDay(day: number): WorkoutPlanExercise[] {
    return (this.workout?.exercises ?? []).filter((e) => e.dayNumber === day);
  }

  isTrainer(): boolean {
    return this.trainerMode() || (this.auth.hasRole(Roles.Trainer) && !this.auth.hasRole(Roles.GymAdmin));
  }

  canAssign(): boolean {
    return this.auth.hasPermission(this.permissions.AssignWorkoutPlan) || this.auth.hasPermission(this.permissions.ManageWorkoutPlans);
  }

  openAssign(): void {
    if (!this.memberId) return;
    this.dialog.open(AssignWorkoutPlanDialogComponent, { width: '480px', data: { memberId: this.memberId } })
      .afterClosed().subscribe((ok) => ok && this.reload());
  }

  toggleComplete(ex: WorkoutPlanExercise, checked: boolean): void {
    this.patch(ex, { isCompleted: checked, completionPercentage: checked ? 100 : 0 });
  }

  updatePct(ex: WorkoutPlanExercise, pct: number): void {
    this.patch(ex, { completionPercentage: pct, isCompleted: pct >= 100 });
  }

  saveNotes(ex: WorkoutPlanExercise, kind: 'trainer' | 'member', value: string): void {
    this.patch(ex, kind === 'trainer' ? { trainerNotes: value } : { memberNotes: value });
  }

  private patch(ex: WorkoutPlanExercise, extra: Partial<{ isCompleted: boolean; completionPercentage: number; trainerNotes: string; memberNotes: string }>): void {
    const mid = this.memberId ?? this.workout?.memberId;
    if (!this.workout?.assignedWorkoutPlanId || !mid) return;
    this.svc.updateProgress({
      memberId: mid,
      assignedWorkoutPlanId: this.workout.assignedWorkoutPlanId,
      workoutPlanExerciseId: ex.workoutPlanExerciseId,
      ...extra,
    }).subscribe({
      next: () => this.reload(),
      error: () => this.notify.error('Update failed'),
    });
  }

  private reload(): void {
    const mid = this.memberId ?? this.workout?.memberId;
    if (!mid) {
      this.svc.getMyWorkout().subscribe({ next: (r) => this.done(r.data) });
      return;
    }
    this.svc.getMemberWorkout(mid).subscribe({ next: (r) => this.done(r.data) });
  }

  private done(data?: MemberWorkoutView): void {
    this.loading.set(false);
    this.workout = data ?? null;
  }

  private fail(): void {
    this.loading.set(false);
    this.notify.error('Failed to load workout');
  }
}
