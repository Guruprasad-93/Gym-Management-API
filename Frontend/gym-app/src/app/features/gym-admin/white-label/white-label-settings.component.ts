import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { WhiteLabelService } from '../../../core/services/white-label.service';
import { FileCategories } from '../../../shared/models/file.models';
import { WhiteLabelSettings } from '../../../shared/models/white-label.models';

@Component({
  selector: 'app-white-label-settings',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './white-label-settings.component.html',
  styleUrl: './white-label-settings.component.css',
})
export class WhiteLabelSettingsComponent implements OnInit {
  pageTitle = 'White Label';
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly svc = inject(WhiteLabelService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly logoCategory = FileCategories.GymLogo;
  gymId = '';
  loading = signal(true);
  saving = signal(false);
  settings = signal<WhiteLabelSettings | null>(null);

  form = this.fb.group({
    brandName: ['', Validators.required],
    appDisplayName: [''],
    primaryColor: ['#ff6600'],
    secondaryColor: ['#101828'],
    supportEmail: [''],
    supportPhone: [''],
    subDomain: [''],
    customDomain: [''],
    isWhiteLabelEnabled: [false],
    mobile: this.fb.group({
      appName: [''],
      androidPackageName: [''],
      iosBundleId: [''],
    }),
  });

  ngOnInit(): void {
    this.pageTitle = this.route.snapshot.data['title'] ?? 'White Label';
    this.gymId = this.auth.user()?.gymId ?? '';
    this.svc.getSettings().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.settings.set(res.data);
          this.patchForm(res.data);
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load white label settings');
      },
    });
    this.svc.getMobileSettings().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.form.controls.mobile.patchValue({
            appName: res.data.appName ?? '',
            androidPackageName: res.data.androidPackageName ?? '',
            iosBundleId: res.data.iosBundleId ?? '',
          });
        }
      },
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();
    this.svc
      .upsertSettings({
        brandName: v.brandName ?? '',
        appDisplayName: v.appDisplayName || undefined,
        primaryColor: v.primaryColor || undefined,
        secondaryColor: v.secondaryColor || undefined,
        supportEmail: v.supportEmail || undefined,
        supportPhone: v.supportPhone || undefined,
        subDomain: v.subDomain || undefined,
        customDomain: v.customDomain || undefined,
        isWhiteLabelEnabled: v.isWhiteLabelEnabled ?? false,
        logoFileId: this.settings()?.logoFileId,
        faviconFileId: this.settings()?.faviconFileId,
        loginBackgroundFileId: this.settings()?.loginBackgroundFileId,
      })
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.saving.set(false);
            this.notify.error(res.message ?? 'Save failed');
            return;
          }
          const mobile = v.mobile!;
          this.svc
            .upsertMobileSettings({
              appName: mobile.appName || undefined,
              androidPackageName: mobile.androidPackageName || undefined,
              iosBundleId: mobile.iosBundleId || undefined,
            })
            .subscribe({
              next: () => {
                this.saving.set(false);
                this.notify.success('Settings saved');
              },
              error: (err) => {
                this.saving.set(false);
                this.notify.error(err.error?.message ?? 'Mobile settings save failed');
              },
            });
        },
        error: (err) => {
          this.saving.set(false);
          this.notify.error(err.error?.message ?? 'Save failed');
        },
      });
  }

  private patchForm(s: WhiteLabelSettings): void {
    this.form.patchValue({
      brandName: s.brandName,
      appDisplayName: s.appDisplayName ?? '',
      primaryColor: s.primaryColor ?? '#ff6600',
      secondaryColor: s.secondaryColor ?? '#101828',
      supportEmail: s.supportEmail ?? '',
      supportPhone: s.supportPhone ?? '',
      subDomain: s.subDomain ?? '',
      customDomain: s.customDomain ?? '',
      isWhiteLabelEnabled: s.isWhiteLabelEnabled,
    });
  }
}
