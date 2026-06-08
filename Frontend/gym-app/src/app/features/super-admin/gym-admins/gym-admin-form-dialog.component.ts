import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GymAdminService } from '../../../core/services/gym-admin.service';
import { GymService } from '../../../core/services/gym.service';
import { NotificationService } from '../../../core/services/notification.service';
import { GymAdmin } from '../../../shared/models/gym-admin.models';
import { Gym } from '../../../shared/models/gym.models';

@Component({
  selector: 'app-gym-admin-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Gym Admin' : 'Create Gym Admin' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Gym</mat-label>
          <mat-select formControlName="gymId">
            @for (gym of gyms; track gym.id) {
              <mat-option [value]="gym.id">{{ gym.name }}</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Full Name</mat-label>
          <input matInput formControlName="name" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" />
        </mat-form-field>
        @if (!isEdit) {
          <mat-checkbox formControlName="generateTemporaryPassword">
            Generate temporary password
          </mat-checkbox>
          @if (!form.controls.generateTemporaryPassword.value) {
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" />
            </mat-form-field>
          }
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" type="button" [disabled]="loading" (click)="save()">
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
export class GymAdminFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly gymAdminService = inject(GymAdminService);
  private readonly gymService = inject(GymService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<GymAdminFormDialogComponent>);
  readonly data = inject<GymAdmin | { presetGymId: string } | null>(MAT_DIALOG_DATA);

  private get editAdmin(): GymAdmin | null {
    return this.data && 'userId' in this.data ? this.data : null;
  }

  private get presetGymId(): string | null {
    return this.data && 'presetGymId' in this.data ? this.data.presetGymId : null;
  }

  isEdit = !!this.editAdmin;
  loading = false;
  gyms: Gym[] = [];

  readonly form = this.fb.nonNullable.group({
    gymId: [this.editAdmin?.gymId ?? this.presetGymId ?? '', Validators.required],
    name: [this.editAdmin?.name ?? '', Validators.required],
    email: [this.editAdmin?.email ?? '', [Validators.required, Validators.email]],
    generateTemporaryPassword: [true],
    password: [''],
  });

  ngOnInit(): void {
    this.gymService.getAll().subscribe({
      next: (res) => {
        if (res.success && res.data) this.gyms = res.data.filter((g) => g.isActive);
      },
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const raw = this.form.getRawValue();

    if (this.isEdit && this.editAdmin) {
      this.gymAdminService
        .update(this.editAdmin.userId, {
          gymId: raw.gymId,
          name: raw.name,
          email: raw.email,
        })
        .subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success) {
              this.notify.success('Gym admin updated');
              this.ref.close({ updated: true });
            }
          },
          error: (err) => {
            this.loading = false;
            this.notify.error(err.error?.message ?? 'Update failed');
          },
        });
      return;
    }

    this.gymAdminService
      .create({
        gymId: raw.gymId,
        name: raw.name,
        email: raw.email,
        password: raw.generateTemporaryPassword ? undefined : raw.password,
        generateTemporaryPassword: raw.generateTemporaryPassword,
      })
      .subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success && res.data) {
            this.ref.close(res.data);
          }
        },
        error: (err) => {
          this.loading = false;
          this.notify.error(err.error?.message ?? 'Create failed');
        },
      });
  }
}
