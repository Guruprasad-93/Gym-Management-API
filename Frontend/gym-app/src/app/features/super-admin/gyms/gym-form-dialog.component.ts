import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { GymService } from '../../../core/services/gym.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Gym } from '../../../shared/models/gym.models';
import { ProfilePhotoManagerComponent } from '../../../shared/components/profile-photo-manager/profile-photo-manager.component';
import { FileCategories } from '../../../shared/models/file.models';

@Component({
  selector: 'app-gym-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ProfilePhotoManagerComponent,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Gym' : 'Add Gym' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-grid">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Phone</mat-label>
          <input matInput formControlName="phone" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Address</mat-label>
          <textarea matInput rows="2" formControlName="address"></textarea>
        </mat-form-field>
        @if (isEdit && data) {
          <app-profile-photo-manager
            title="Gym logo"
            [category]="logoCategory"
            [gymId]="data.id"
            [size]="120"
            [circle]="false"
            uploadLabel="Upload logo" />
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" type="button" [disabled]="loading" (click)="save()">
        Save
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .full-width {
        width: 100%;
      }
      .form-grid {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        min-width: 320px;
      }
    `,
  ],
})
export class GymFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly gymService = inject(GymService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<GymFormDialogComponent>);
  readonly data = inject<Gym | null>(MAT_DIALOG_DATA);

  isEdit = !!this.data;
  loading = false;
  readonly logoCategory = FileCategories.GymLogo;

  readonly form = this.fb.nonNullable.group({
    name: [this.data?.name ?? '', Validators.required],
    email: [this.data?.email ?? ''],
    phone: [this.data?.phone ?? ''],
    address: [this.data?.address ?? ''],
  });

  save(): void {
    if (this.form.invalid) return;
    this.loading = true;
    const dto = this.form.getRawValue();
    const req = this.isEdit
      ? this.gymService.update(this.data!.id, dto)
      : this.gymService.create(dto);
    req.subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.notify.success(this.isEdit ? 'Gym updated' : 'Gym created');
          this.ref.close(true);
        } else {
          this.notify.error(res.message ?? 'Save failed');
        }
      },
      error: (err) => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Save failed');
      },
    });
  }
}
