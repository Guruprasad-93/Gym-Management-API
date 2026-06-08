import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';

@Component({
  selector: 'app-member-form-dialog',
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
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Member' : 'Add Member' }}</h2>
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
        } @else {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Full Name</mat-label>
            <input matInput formControlName="fullName" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
          </mat-form-field>
        }
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Phone</mat-label>
          <input matInput formControlName="phone" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Date of Birth</mat-label>
          <input matInput type="date" formControlName="dateOfBirth" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Gender</mat-label>
          <input matInput formControlName="gender" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Height (cm)</mat-label>
          <input matInput type="number" formControlName="height" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Weight (kg)</mat-label>
          <input matInput type="number" formControlName="weight" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Address</mat-label>
          <textarea matInput rows="2" formControlName="address"></textarea>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Emergency Contact</mat-label>
          <input matInput formControlName="emergencyContact" />
        </mat-form-field>
        @if (!isEdit) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Join Date</mat-label>
            <input matInput type="date" formControlName="joinDate" />
          </mat-form-field>
        }
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
export class MemberFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly memberService = inject(MemberService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<MemberFormDialogComponent>);
  readonly data = inject<Member | null>(MAT_DIALOG_DATA, { optional: true });

  isEdit = false;
  loading = false;

  readonly form = this.fb.nonNullable.group({
    name: [''],
    fullName: [''],
    email: [''],
    password: [''],
    phone: [''],
    dateOfBirth: [''],
    gender: [''],
    height: [null as number | null],
    weight: [null as number | null],
    address: [''],
    emergencyContact: [''],
    joinDate: [new Date().toISOString().slice(0, 10)],
    isActive: [true],
  });

  ngOnInit(): void {
    this.isEdit = !!this.data;
    if (this.isEdit && this.data) {
      this.form.patchValue({
        fullName: this.data.fullName,
        email: this.data.email,
        phone: this.data.phone ?? '',
        dateOfBirth: this.data.dateOfBirth ?? '',
        gender: this.data.gender ?? '',
        height: this.data.height ?? null,
        weight: this.data.weight ?? null,
        address: this.data.address ?? '',
        emergencyContact: this.data.emergencyContact ?? '',
        isActive: this.data.isActive,
      });
    } else {
      this.form.controls.name.setValidators([Validators.required, Validators.maxLength(100)]);
      this.form.controls.email.setValidators([Validators.required, Validators.email]);
      this.form.controls.password.setValidators([Validators.required, Validators.minLength(8)]);
      this.form.controls.joinDate.setValidators([Validators.required]);
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
      this.memberService
        .update(this.data.id, {
          fullName: raw.fullName || undefined,
          email: raw.email || undefined,
          phone: raw.phone || undefined,
          dateOfBirth: raw.dateOfBirth || undefined,
          gender: raw.gender || undefined,
          height: raw.height ?? undefined,
          weight: raw.weight ?? undefined,
          address: raw.address || undefined,
          emergencyContact: raw.emergencyContact || undefined,
          isActive: raw.isActive,
        })
        .subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success) {
              this.notify.success('Member updated');
              this.ref.close(true);
            }
          },
          error: (err) => {
            this.loading = false;
            this.notify.error(err.error?.message ?? 'Update failed');
          },
        });
    } else {
      this.memberService
        .create({
          name: raw.name,
          email: raw.email,
          password: raw.password,
          phone: raw.phone || undefined,
          dateOfBirth: raw.dateOfBirth || undefined,
          gender: raw.gender || undefined,
          height: raw.height ?? undefined,
          weight: raw.weight ?? undefined,
          address: raw.address || undefined,
          emergencyContact: raw.emergencyContact || undefined,
          joinDate: raw.joinDate,
        })
        .subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success) {
              this.notify.success('Member created');
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
