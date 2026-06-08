import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { WhiteLabelService } from '../core/services/white-label.service';
import { getDefaultRouteForUser } from '../core/constants/menu.config';
import { WhiteLabelLoginBranding } from '../shared/models/white-label.models';

const DEFAULT_HERO_BG =
  'url(https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1200&q=80)';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  form: FormGroup;
  submitted = false;
  loading = false;
  showPassword = false;
  errorMessage: string | null = null;
  branding = signal<WhiteLabelLoginBranding | null>(null);

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly whiteLabel = inject(WhiteLabelService);

  constructor() {
    this.form = this.fb.group({
      email: [
        localStorage.getItem('gym_remember_email') ?? '',
        [Validators.required, Validators.email, Validators.maxLength(256)],
      ],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [!!localStorage.getItem('gym_remember_email')],
    });
  }

  get emailControl() {
    return this.form.get('email');
  }

  get passwordControl() {
    return this.form.get('password');
  }

  get primaryColor(): string {
    return this.branding()?.primaryColor ?? '#ff6600';
  }

  get brandName(): string {
    const b = this.branding();
    return (b?.appDisplayName || b?.brandName || 'FITNESS PRO').toUpperCase();
  }

  get brandTagline(): string {
    return this.branding()?.brandName && this.branding()?.appDisplayName
      ? this.branding()!.brandName!.toUpperCase()
      : 'GYM MANAGEMENT';
  }

  get supportEmail(): string | null {
    return this.branding()?.supportEmail ?? null;
  }

  get heroBackground(): string {
    const url = this.branding()?.loginBackgroundUrl;
    return url ? `url(${url})` : DEFAULT_HERO_BG;
  }

  ngOnInit(): void {
    this.loadBranding();
  }

  submit(): void {
    this.submitted = true;
    this.errorMessage = null;

    if (this.form.invalid) return;

    this.loading = true;
    const { email, password, rememberMe } = this.form.getRawValue();

    if (rememberMe) {
      localStorage.setItem('gym_remember_email', email);
    } else {
      localStorage.removeItem('gym_remember_email');
    }

    this.auth.login({ email: email!, password: password! }).subscribe({
      next: (response) => {
        this.loading = false;
        if (response.success && response.data) {
          if (response.data.mustChangePassword) {
            this.router.navigate(['/auth/change-password']);
            return;
          }
          this.router.navigateByUrl(getDefaultRouteForUser(response.data.roles));
          return;
        }
        this.errorMessage = response.message ?? 'Login failed.';
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message ?? 'Invalid email or password.';
      },
    });
  }

  private loadBranding(): void {
    const params = new URLSearchParams(window.location.search);
    const host = window.location.hostname;
    const subDomain = params.get('subDomain') ?? this.resolveSubDomainFromHost();
    const customDomain =
      params.get('customDomain') ??
      (host !== 'localhost' && !host.startsWith('127.') ? host : undefined);
    const gymId = params.get('gymId') ?? undefined;

    if (!subDomain && !customDomain && !gymId) return;

    this.whiteLabel.getLoginBranding({ subDomain: subDomain || undefined, customDomain, gymId }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branding.set(res.data);
      },
      error: () => undefined,
    });
  }

  private resolveSubDomainFromHost(): string | undefined {
    const host = window.location.hostname;
    if (!host.includes('.gymsaas.com')) return undefined;
    const part = host.split('.')[0];
    return part && part !== 'www' ? part : undefined;
  }
}
