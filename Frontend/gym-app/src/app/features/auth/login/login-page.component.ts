import { Component, OnInit, inject, signal } from '@angular/core';
import { NgStyle } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WhiteLabelService } from '../../../core/services/white-label.service';
import { getDefaultRouteForUser } from '../../../core/constants/menu.config';
import { WhiteLabelLoginBranding } from '../../../shared/models/white-label.models';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    NgStyle,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatIconModule,
  ],
  template: `
    <div class="auth-page" [ngStyle]="pageBackgroundStyle">
      <mat-card class="auth-card">
        <mat-card-header>
          <div mat-card-avatar class="avatar-wrap">
            @if (branding()?.logoUrl) {
              <img [src]="branding()!.logoUrl!" alt="Logo" class="brand-logo" />
            } @else {
              <mat-icon>fitness_center</mat-icon>
            }
          </div>
          <mat-card-title>{{ displayName }}</mat-card-title>
          <mat-card-subtitle>Sign in to your account</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" autocomplete="email" />
              @if (form.controls.email.invalid && form.controls.email.touched) {
                <mat-error>Valid email is required</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password" />
              @if (form.controls.password.invalid && form.controls.password.touched) {
                <mat-error>Password is required (min 6 characters)</mat-error>
              }
            </mat-form-field>
            <mat-checkbox formControlName="rememberMe">Remember me</mat-checkbox>
            <button mat-flat-button class="full-width submit-btn" [disabled]="loading" [style.background]="primaryColor">
              @if (loading) {
                <mat-spinner diameter="22" />
              } @else {
                Sign In
              }
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-button routerLink="/auth/forgot-password">Forgot password?</a>
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
        background: linear-gradient(135deg, #1a237e 0%, #3949ab 50%, #5c6bc0 100%);
        padding: 1rem;
      }
      .auth-card {
        width: 100%;
        max-width: 420px;
        padding: 0.5rem;
      }
      .brand-logo {
        width: 40px;
        height: 40px;
        object-fit: contain;
        border-radius: 4px;
      }
      .avatar-wrap {
        display: flex;
        align-items: center;
        justify-content: center;
      }
      .full-width {
        width: 100%;
        display: block;
      }
      .submit-btn {
        margin-top: 1rem;
        height: 48px;
        color: #fff;
      }
    `,
  ],
})
export class LoginPageComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notify = inject(NotificationService);
  private readonly whiteLabel = inject(WhiteLabelService);

  loading = false;
  branding = signal<WhiteLabelLoginBranding | null>(null);

  readonly form = this.fb.nonNullable.group({
    email: [localStorage.getItem('gym_remember_email') ?? '', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [false],
  });

  get displayName(): string {
    const b = this.branding();
    return b?.appDisplayName || b?.brandName || 'Gym Management';
  }

  get primaryColor(): string {
    return this.branding()?.primaryColor ?? '#3949ab';
  }

  get pageBackgroundStyle(): Record<string, string> {
    const b = this.branding();
    if (b?.loginBackgroundUrl) {
      return { background: `url(${b.loginBackgroundUrl}) center/cover no-repeat` };
    }
    const primary = b?.primaryColor ?? '#1a237e';
    const secondary = b?.secondaryColor ?? '#5c6bc0';
    return { background: `linear-gradient(135deg, ${primary} 0%, ${secondary} 100%)` };
  }

  ngOnInit(): void {
    const params = new URLSearchParams(window.location.search);
    const host = window.location.hostname;
    const subDomain = params.get('subDomain') ?? this.resolveSubDomainFromHost();
    const customDomain = params.get('customDomain')
      ?? (host !== 'localhost' && !host.startsWith('127.') ? host : undefined);
    const gymId = params.get('gymId') ?? undefined;

    if (!subDomain && !customDomain && !gymId) {
      return;
    }

    this.whiteLabel.getLoginBranding({ subDomain: subDomain || undefined, customDomain, gymId }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branding.set(res.data);
      },
      error: () => undefined,
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const { email, password, rememberMe } = this.form.getRawValue();
    if (rememberMe) {
      localStorage.setItem('gym_remember_email', email);
    } else {
      localStorage.removeItem('gym_remember_email');
    }

    this.auth.login({ email, password }).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          if (res.data.mustChangePassword) {
            this.notify.warning('You must change your password before continuing.');
            this.router.navigate(['/auth/change-password']);
            return;
          }
          this.notify.success('Login successful');
          this.router.navigateByUrl(getDefaultRouteForUser(res.data.roles));
          return;
        }
        this.notify.error(res.message ?? 'Login failed');
      },
      error: (err) => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Invalid email or password');
      },
    });
  }

  private resolveSubDomainFromHost(): string | undefined {
    const host = window.location.hostname;
    if (!host.includes('.gymsaas.com')) return undefined;
    const part = host.split('.')[0];
    return part && part !== 'www' ? part : undefined;
  }
}
