import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

function passwordMatchValidator(control: AbstractControl) {
  const password = control.get('newPassword')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-change-password-page',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="auth-page">
      <mat-card class="auth-card">
        <mat-card-title>Change Password</mat-card-title>
        <mat-card-subtitle>
          @if (auth.mustChangePassword()) {
            You must set a new password before continuing.
          } @else {
            Update your account password.
          }
        </mat-card-subtitle>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Current Password</mat-label>
              <input matInput type="password" formControlName="currentPassword" autocomplete="current-password" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>New Password</mat-label>
              <input matInput type="password" formControlName="newPassword" autocomplete="new-password" />
              @if (form.controls.newPassword.hasError('minlength') && form.controls.newPassword.touched) {
                <mat-error>Minimum 8 characters</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Confirm New Password</mat-label>
              <input matInput type="password" formControlName="confirmPassword" autocomplete="new-password" />
              @if (form.hasError('passwordMismatch') && form.touched) {
                <mat-error>Passwords do not match</mat-error>
              }
            </mat-form-field>
            <button mat-flat-button color="primary" class="full-width submit-btn" [disabled]="loading">
              @if (loading) {
                <mat-spinner diameter="22" />
              } @else {
                Update Password
              }
            </button>
          </form>
        </mat-card-content>
        @if (!auth.mustChangePassword()) {
          <mat-card-actions>
            <a mat-button routerLink="/auth/login">Back to login</a>
          </mat-card-actions>
        }
      </mat-card>
    </div>
  `,
  styles: [
    `
      .auth-page {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: linear-gradient(135deg, #1a237e 0%, #3949ab 100%);
        padding: 1rem;
      }
      .auth-card {
        width: 100%;
        max-width: 420px;
        padding: 1rem;
      }
      .full-width {
        width: 100%;
        display: block;
      }
      .submit-btn {
        margin-top: 0.5rem;
        height: 48px;
      }
    `,
  ],
})
export class ChangePasswordPageComponent {
  readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);

  loading = false;

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    },
    { validators: passwordMatchValidator }
  );

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { currentPassword, newPassword } = this.form.getRawValue();
    this.loading = true;
    this.auth.changePassword({ currentPassword, newPassword }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.notify.success('Password updated. Please sign in again.');
          this.auth.clearSession();
          this.router.navigate(['/auth/login']);
        } else {
          this.notify.error(res.message ?? 'Password change failed');
        }
      },
      error: (err) => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Password change failed');
      },
    });
  }
}
