import { Component, OnInit, inject, signal } from '@angular/core';

import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { MatIconModule } from '@angular/material/icon';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../core/services/auth.service';

import { NotificationService } from '../../../core/services/notification.service';

import { SaasSubscriptionService } from '../../../core/services/saas-subscription.service';

import { ProfilePhotoManagerComponent } from '../../../shared/components/profile-photo-manager/profile-photo-manager.component';

import { FileCategories } from '../../../shared/models/file.models';

import { GymBranding } from '../../../shared/models/saas.models';



@Component({

  selector: 'app-gym-branding',

  standalone: true,

  imports: [ReactiveFormsModule, MatIconModule, MatProgressSpinnerModule, ProfilePhotoManagerComponent],

  templateUrl: './gym-branding.component.html',

  styleUrl: './gym-branding.component.css',

})

export class GymBrandingComponent implements OnInit {

  private readonly auth = inject(AuthService);

  private readonly saas = inject(SaasSubscriptionService);

  private readonly notify = inject(NotificationService);

  private readonly fb = inject(FormBuilder);



  readonly logoCategory = FileCategories.GymLogo;

  gymId = '';

  loading = signal(true);

  saving = signal(false);



  form = this.fb.group({

    primaryColor: ['#ff6600'],

    secondaryColor: ['#101828'],

    receiptHeaderText: [''],

    invoiceFooterText: [''],

  });



  ngOnInit(): void {

    this.gymId = this.auth.user()?.gymId ?? '';

    this.saas.getBranding().subscribe({

      next: (res) => {

        this.loading.set(false);

        if (res.success && res.data) this.patchForm(res.data);

      },

      error: () => {

        this.loading.set(false);

        this.notify.error('Failed to load branding');

      },

    });

  }



  save(): void {

    this.saving.set(true);

    const v = this.form.getRawValue();

    this.saas

      .updateBranding({

        primaryColor: v.primaryColor ?? undefined,

        secondaryColor: v.secondaryColor ?? undefined,

        receiptHeaderText: v.receiptHeaderText ?? undefined,

        invoiceFooterText: v.invoiceFooterText ?? undefined,

      })

      .subscribe({

        next: (res) => {

          this.saving.set(false);

          if (res.success) this.notify.success('Branding saved');

        },

        error: () => {

          this.saving.set(false);

          this.notify.error('Save failed');

        },

      });

  }



  private patchForm(b: GymBranding): void {

    this.form.patchValue({

      primaryColor: b.primaryColor ?? '#ff6600',

      secondaryColor: b.secondaryColor ?? '#101828',

      receiptHeaderText: b.receiptHeaderText ?? '',

      invoiceFooterText: b.invoiceFooterText ?? '',

    });

  }

}

