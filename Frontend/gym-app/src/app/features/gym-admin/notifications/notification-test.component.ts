import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { GymNotificationService } from '../../../core/services/gym-notification.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NOTIFICATION_TYPES } from '../../../shared/models/notification.models';

@Component({
  selector: 'app-notification-test',
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCardModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Test Notification" subtitle="Send a test WhatsApp message">
      <button mat-stroked-button routerLink="..">Dashboard</button>
    </app-page-header>

    <mat-card class="form-card">
      <mat-card-content [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Phone number</mat-label>
          <input matInput formControlName="phoneNumber" placeholder="9876543210" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Notification type</mat-label>
          <mat-select formControlName="notificationType">
            @for (t of types; track t) { <mat-option [value]="t">{{ t }}</mat-option> }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Template name override</mat-label>
          <input matInput formControlName="templateName" />
        </mat-form-field>
        <button mat-flat-button color="primary" type="button" [disabled]="form.invalid || sending" (click)="send()">
          Send test
        </button>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`.form-card { max-width: 480px; } .full-width { width: 100%; display: block; }`],
})
export class NotificationTestComponent {
  private readonly svc = inject(GymNotificationService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  readonly types = NOTIFICATION_TYPES;
  sending = false;

  form = this.fb.nonNullable.group({
    phoneNumber: ['', Validators.required],
    notificationType: ['PaymentSuccess', Validators.required],
    templateName: [''],
  });

  send(): void {
    if (this.form.invalid) return;
    this.sending = true;
    const v = this.form.getRawValue();
    this.svc.sendTest({
      phoneNumber: v.phoneNumber,
      notificationType: v.notificationType,
      templateName: v.templateName || undefined,
      variables: { memberName: 'Test User', amount: '999' },
    }).subscribe({
      next: (res) => {
        this.sending = false;
        if (res.success) this.notify.success(`Test sent — status: ${res.data?.status ?? 'OK'}`);
      },
      error: (e) => {
        this.sending = false;
        this.notify.error(e.error?.message ?? 'Send failed');
      },
    });
  }
}
