import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WebsiteService } from '../../../core/services/website.service';
import { NotificationService } from '../../../core/services/notification.service';
import { GymWebsiteSettings } from '../../../shared/models/website.models';

@Component({
  selector: 'app-website-builder',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './website-builder.component.html',
  styleUrl: './website-builder.component.css',
})
export class WebsiteBuilderComponent implements OnInit {
  private readonly service = inject(WebsiteService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  settings = signal<GymWebsiteSettings | null>(null);
  loading = signal(true);
  saving = signal(false);

  form = this.fb.group({
    websiteSlug: ['', Validators.required],
    websiteTitle: [''],
    websiteDescription: [''],
    primaryColor: ['#ff6600'],
    secondaryColor: ['#101828'],
    contactPhone: [''],
    contactEmail: [''],
    whatsAppNumber: [''],
    address: [''],
    metaTitle: [''],
    metaDescription: [''],
    metaKeywords: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getSettings().subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) {
          this.settings.set(r.data);
          this.form.patchValue({
            websiteSlug: r.data.websiteSlug,
            websiteTitle: r.data.websiteTitle ?? '',
            websiteDescription: r.data.websiteDescription ?? '',
            primaryColor: r.data.primaryColor ?? '#ff6600',
            secondaryColor: r.data.secondaryColor ?? '#101828',
            contactPhone: r.data.contactPhone ?? '',
            contactEmail: r.data.contactEmail ?? '',
            whatsAppNumber: r.data.whatsAppNumber ?? '',
            address: r.data.address ?? '',
            metaTitle: r.data.metaTitle ?? '',
            metaDescription: r.data.metaDescription ?? '',
            metaKeywords: r.data.metaKeywords ?? '',
          });
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load website settings');
      },
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.service.upsertSettings(this.form.getRawValue() as never).subscribe({
      next: () => {
        this.saving.set(false);
        this.notify.success('Settings saved');
        this.load();
      },
      error: () => {
        this.saving.set(false);
        this.notify.error('Failed to save settings');
      },
    });
  }

  publish(): void {
    this.service.publish().subscribe({
      next: () => {
        this.notify.success('Website published');
        this.load();
      },
      error: () => this.notify.error('Publish failed'),
    });
  }

  unpublish(): void {
    this.service.unpublish().subscribe({
      next: () => {
        this.notify.success('Website unpublished');
        this.load();
      },
      error: () => this.notify.error('Unpublish failed'),
    });
  }
}
