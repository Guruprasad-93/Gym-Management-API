import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TrainerService } from '../../../core/services/trainer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Trainer } from '../../../shared/models/trainer.models';

@Component({
  selector: 'app-trainer-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Trainer' : 'Add Trainer' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-grid">
        @if (!isEdit) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Full Name</mat-label>
            <input matInput formControlName="name" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" />
          </mat-form-field>
        }
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Specialization</mat-label>
          <input matInput formControlName="specialization" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Bio</mat-label>
          <textarea matInput rows="3" formControlName="bio"></textarea>
        </mat-form-field>
        @if (isEdit) {
          <mat-checkbox formControlName="isActive">Active</mat-checkbox>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" type="button" [disabled]="loading || form.invalid" (click)="save()">
        @if (loading) {
          <mat-spinner diameter="22" />
        } @else {
          Save
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .form-grid {
        min-width: 360px;
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
      }
      .full-width {
        width: 100%;
      }
    `,
  ],
})
export class TrainerFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<TrainerFormDialogComponent>);
  readonly data = inject<Trainer | null>(MAT_DIALOG_DATA, { optional: true });

  loading = false;
  isEdit = false;

  readonly form = this.fb.nonNullable.group({
    name: [''],
    email: [''],
    password: [''],
    specialization: [''],
    bio: [''],
    isActive: [true],
  });

  ngOnInit(): void {
    this.isEdit = !!this.data;
    if (this.isEdit && this.data) {
      this.form.patchValue({
        specialization: this.data.specialization ?? '',
        bio: this.data.bio ?? '',
        isActive: this.data.isActive,
      });
      this.form.controls.name.clearValidators();
      this.form.controls.email.clearValidators();
      this.form.controls.password.clearValidators();
    } else {
      this.form.controls.name.setValidators([Validators.required, Validators.maxLength(100)]);
      this.form.controls.email.setValidators([Validators.required, Validators.email]);
      this.form.controls.password.setValidators([Validators.required, Validators.minLength(8)]);
    }
    this.form.updateValueAndValidity();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const raw = this.form.getRawValue();

    if (this.isEdit && this.data) {
      this.trainerService
        .update(this.data.id, {
          specialization: raw.specialization || undefined,
          bio: raw.bio || undefined,
          isActive: raw.isActive,
        })
        .subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success) {
              this.notify.success('Trainer updated');
              this.ref.close(true);
            }
          },
          error: (err) => {
            this.loading = false;
            this.notify.error(err.error?.message ?? 'Update failed');
          },
        });
    } else {
      this.trainerService
        .create({
          name: raw.name,
          email: raw.email,
          password: raw.password,
          specialization: raw.specialization || undefined,
          bio: raw.bio || undefined,
        })
        .subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success) {
              this.notify.success('Trainer created');
              this.ref.close(true);
            }
          },
          error: (err) => {
            this.loading = false;
            this.notify.error(err.error?.message ?? 'Create failed');
          },
        });
    }
  }
}
