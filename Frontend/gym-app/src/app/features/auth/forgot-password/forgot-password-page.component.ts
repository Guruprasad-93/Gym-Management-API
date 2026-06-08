import { Component, inject } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Router, RouterModule } from '@angular/router';

import { MatCardModule } from '@angular/material/card';

import { MatFormFieldModule } from '@angular/material/form-field';

import { MatInputModule } from '@angular/material/input';

import { MatButtonModule } from '@angular/material/button';

import { AuthService } from '../../../core/services/auth.service';

import { NotificationService } from '../../../core/services/notification.service';



@Component({

  selector: 'app-forgot-password-page',

  standalone: true,

  imports: [

    ReactiveFormsModule,

    RouterModule,

    MatCardModule,

    MatFormFieldModule,

    MatInputModule,

    MatButtonModule,

  ],

  template: `

    <div class="auth-page">

      <mat-card class="auth-card">

        <mat-card-title>Forgot Password</mat-card-title>

        <mat-card-content>

          <p class="hint">

            Enter your email address. If an account exists, you will receive instructions to reset your password.

          </p>

          <form [formGroup]="form" (ngSubmit)="submit()">

            <mat-form-field appearance="outline" class="full-width">

              <mat-label>Email</mat-label>

              <input matInput type="email" formControlName="email" autocomplete="email" />

              @if (form.controls.email.invalid && form.controls.email.touched) {

                <mat-error>Valid email is required</mat-error>

              }

            </mat-form-field>

            <button mat-flat-button color="primary" class="full-width" [disabled]="loading">

              Send Reset Instructions

            </button>

          </form>

          @if (resetLink) {

            <div class="dev-reset">

              <p class="dev-label">Development reset link:</p>

              <a mat-stroked-button [href]="resetLink" class="full-width dev-link">Open reset password page</a>

            </div>

          }

        </mat-card-content>

        <mat-card-actions>

          <a mat-button routerLink="/auth/login">Back to login</a>

        </mat-card-actions>

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

        background: #f4f6f9;

        padding: 1rem;

      }

      .auth-card {

        width: 100%;

        max-width: 420px;

        padding: 1rem;

      }

      .full-width {

        width: 100%;

      }

      .hint {

        color: rgba(0, 0, 0, 0.6);

        font-size: 0.9rem;

        margin-bottom: 1rem;

      }

      .dev-reset {

        margin-top: 1.25rem;

        padding-top: 1rem;

        border-top: 1px dashed #ccc;

      }

      .dev-label {

        font-size: 0.85rem;

        color: #666;

        margin-bottom: 0.5rem;

      }

      .dev-link {

        text-decoration: none;

      }

    `,

  ],

})

export class ForgotPasswordPageComponent {

  private readonly fb = inject(FormBuilder);

  private readonly auth = inject(AuthService);

  private readonly router = inject(Router);

  private readonly notify = inject(NotificationService);



  loading = false;

  resetLink: string | null = null;



  readonly form = this.fb.nonNullable.group({

    email: ['', [Validators.required, Validators.email]],

  });



  submit(): void {

    if (this.form.invalid) {

      this.form.markAllAsTouched();

      return;

    }

    this.loading = true;

    this.resetLink = null;

    this.auth.forgotPassword(this.form.controls.email.value).subscribe({

      next: (res) => {

        this.loading = false;

        if (res.success) {

          this.resetLink = res.data?.resetLink ?? null;

          this.notify.success(

            res.message ?? 'If the email exists, a password reset link has been sent.'

          );

          if (this.resetLink) {

            return;

          }

          this.router.navigate(['/auth/login']);

        } else {

          this.notify.error(res.message ?? 'Request failed');

        }

      },

      error: (err) => {

        this.loading = false;

        this.notify.error(err.error?.message ?? 'Request failed');

      },

    });

  }

}


