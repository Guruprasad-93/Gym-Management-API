import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime } from 'rxjs';
import { WorkoutService } from '../../../core/services/workout.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { Exercise, ExerciseCategory, DIFFICULTIES } from '../../../shared/models/workout.models';
import { ExerciseFormDialogComponent } from './exercise-form-dialog.component';

@Component({
  selector: 'app-exercise-library',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Exercise Library" subtitle="Manage gym exercises">
      @if (auth.hasPermission(permissions.ManageWorkoutPlans)) {
        <button mat-flat-button color="primary" type="button" (click)="openCreate()"><mat-icon>add</mat-icon> Add Exercise</button>
      }
    </app-page-header>
    <div class="filters">
      <mat-form-field appearance="outline"><mat-label>Search</mat-label><input matInput [formControl]="search" /></mat-form-field>
    </div>
    @if (loading()) { <mat-spinner /> } @else {
      <table mat-table [dataSource]="exercises()" class="table-card">
        <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let r">{{ r.exerciseName }}</td></ng-container>
        <ng-container matColumnDef="category"><th mat-header-cell *matHeaderCellDef>Category</th><td mat-cell *matCellDef="let r">{{ r.categoryName || '—' }}</td></ng-container>
        <ng-container matColumnDef="muscle"><th mat-header-cell *matHeaderCellDef>Muscle</th><td mat-cell *matCellDef="let r">{{ r.muscleGroup || '—' }}</td></ng-container>
        <ng-container matColumnDef="difficulty"><th mat-header-cell *matHeaderCellDef>Level</th><td mat-cell *matCellDef="let r">{{ r.difficulty || '—' }}</td></ng-container>
        <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef></th>
          <td mat-cell *matCellDef="let r">
            @if (auth.hasPermission(permissions.ManageWorkoutPlans)) {
              <button mat-icon-button (click)="openEdit(r)"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button color="warn" (click)="remove(r)"><mat-icon>delete</mat-icon></button>
            }
          </td>
        </ng-container>
        <tr mat-header-row *matHeaderRowDef="cols"></tr>
        <tr mat-row *matRowDef="let row; columns: cols"></tr>
      </table>
    }
  `,
  styles: [`.filters { margin-bottom: 1rem; } .table-card { width: 100%; background: #fff; border-radius: 8px; }`],
})
export class ExerciseLibraryComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(WorkoutService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  loading = signal(true);
  exercises = signal<Exercise[]>([]);
  categories = signal<ExerciseCategory[]>([]);
  cols = ['name', 'category', 'muscle', 'difficulty', 'actions'];
  search = new FormControl('');

  ngOnInit(): void {
    this.svc.getCategories().subscribe({ next: (r) => r.success && r.data && this.categories.set(r.data) });
    this.search.valueChanges.pipe(debounceTime(300)).subscribe(() => this.load());
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getExercises(true, undefined, this.search.value || undefined).subscribe({
      next: (r) => { this.loading.set(false); if (r.success && r.data) this.exercises.set(r.data); },
      error: () => { this.loading.set(false); this.notify.error('Failed to load exercises'); },
    });
  }

  openCreate(): void {
    this.dialog.open(ExerciseFormDialogComponent, { width: '520px', data: { categories: this.categories() } })
      .afterClosed().subscribe((ok) => ok && this.load());
  }

  openEdit(ex: Exercise): void {
    this.dialog.open(ExerciseFormDialogComponent, { width: '520px', data: { exercise: ex, categories: this.categories() } })
      .afterClosed().subscribe((ok) => ok && this.load());
  }

  remove(ex: Exercise): void {
    if (!confirm(`Delete "${ex.exerciseName}"?`)) return;
    this.svc.deleteExercise(ex.exerciseId).subscribe({
      next: () => { this.notify.success('Deleted'); this.load(); },
      error: () => this.notify.error('Delete failed'),
    });
  }
}
