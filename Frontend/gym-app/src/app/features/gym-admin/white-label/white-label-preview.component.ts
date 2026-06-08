import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WhiteLabelService } from '../../../core/services/white-label.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { WhiteLabelPreview } from '../../../shared/models/white-label.models';

@Component({
  selector: 'app-white-label-preview',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatButtonModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header title="White Label Preview" subtitle="Preview login, website, and mobile branding before publishing">
      <a mat-stroked-button routerLink="/gym-admin/white-label">Back to settings</a>
    </app-page-header>

    @if (loading()) { <mat-spinner /> } @else if (preview) {
      <div class="preview-grid">
        <mat-card>
          <mat-card-header><mat-card-title>Login page</mat-card-title></mat-card-header>
          <mat-card-content>
            <div class="login-preview" [style.background]="loginBackground">
              @if (preview.login.logoUrl) {
                <img [src]="preview.login.logoUrl" alt="Logo" class="logo" />
              }
              <h3 [style.color]="preview.login.primaryColor ?? '#fff'">{{ preview.login.appDisplayName || preview.login.brandName }}</h3>
              <p>Sign in to your account</p>
            </div>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header><mat-card-title>Website</mat-card-title></mat-card-header>
          <mat-card-content>
            <header [style.background]="preview.website.primaryColor ?? '#1565c0'" class="site-header">
              @if (preview.website.logoUrl) { <img [src]="preview.website.logoUrl" alt="Logo" /> }
              <span>{{ preview.website.websiteTitle || preview.website.brandName }}</span>
            </header>
          </mat-card-content>
        </mat-card>

        <mat-card>
          <mat-card-header><mat-card-title>Mobile app</mat-card-title></mat-card-header>
          <mat-card-content class="mobile-preview">
            @if (preview.mobile.appIconUrl) { <img [src]="preview.mobile.appIconUrl" alt="App icon" class="icon" /> }
            <strong>{{ preview.mobile.appName || preview.login.brandName }}</strong>
            @if (preview.mobile.splashScreenUrl) {
              <img [src]="preview.mobile.splashScreenUrl" alt="Splash" class="splash" />
            }
          </mat-card-content>
        </mat-card>
      </div>
    }
  `,
  styles: `
    .preview-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
    .login-preview { padding: 2rem; border-radius: 8px; text-align: center; color: #fff; min-height: 200px; }
    .logo { max-height: 64px; margin-bottom: 1rem; }
    .site-header { color: #fff; padding: 1rem; display: flex; align-items: center; gap: .75rem; border-radius: 4px; }
    .site-header img { max-height: 40px; }
    .mobile-preview { display: flex; flex-direction: column; align-items: center; gap: .75rem; }
    .icon { width: 72px; height: 72px; border-radius: 16px; }
    .splash { max-width: 100%; border-radius: 8px; }
  `,
})
export class WhiteLabelPreviewComponent implements OnInit {
  private readonly svc = inject(WhiteLabelService);
  loading = signal(true);
  preview: WhiteLabelPreview | null = null;

  get loginBackground(): string {
    const bg = this.preview?.login.loginBackgroundUrl;
    const primary = this.preview?.login.primaryColor ?? '#1a237e';
    return bg ? `url(${bg}) center/cover` : `linear-gradient(135deg, ${primary}, #3949ab)`;
  }

  ngOnInit(): void {
    this.svc.getPreview().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.preview = res.data;
      },
      error: () => this.loading.set(false),
    });
  }
}
