import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { GymNotificationService } from '../../../core/services/gym-notification.service';
import { NotificationService } from '../../../core/services/notification.service';
import { NOTIFICATION_TYPES, NotificationTemplate } from '../../../shared/models/notification.models';

@Component({
  selector: 'app-notification-template-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Template' : 'Create Template' }}</h2>
    <mat-dialog-content [formGroup]="form">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Type</mat-label>
        <mat-select formControlName="notificationType">
          @for (t of types; track t) {
            <mat-option [value]="t">{{ t }}</mat-option>
          }
        </mat-select>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>WhatsApp template name</mat-label>
        <input matInput formControlName="templateName" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Body template</mat-label>
        <textarea matInput rows="3" formControlName="bodyTemplate"></textarea>
      </mat-form-field>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Variables JSON</mat-label>
        <textarea matInput rows="2" formControlName="variablesJson" placeholder='{"memberName":"{{1}}"}'></textarea>
      </mat-form-field>
      <mat-checkbox formControlName="isActive">Active</mat-checkbox>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="ref.close()">Cancel</button>
      <button mat-flat-button color="primary" type="button" [disabled]="form.invalid || saving" (click)="save()">Save</button>
    </mat-dialog-actions>
  `,
  styles: [`.full-width { width: 100%; display: block; }`],
})
export class NotificationTemplateFormDialogComponent {
  private readonly svc = inject(GymNotificationService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  readonly ref = inject(MatDialogRef<NotificationTemplateFormDialogComponent>);
  readonly data = inject<NotificationTemplate | null>(MAT_DIALOG_DATA);
  readonly types = NOTIFICATION_TYPES;
  readonly isEdit = !!inject<NotificationTemplate | null>(MAT_DIALOG_DATA);
  saving = false;

  form = this.fb.nonNullable.group({
    notificationType: ['', Validators.required],
    templateName: ['', Validators.required],
    bodyTemplate: [''],
    variablesJson: [''],
    isActive: [true],
  });

  constructor() {
    const d = this.data;
    if (d) {
      this.form.patchValue({
        notificationType: d.notificationType,
        templateName: d.templateName,
        bodyTemplate: d.bodyTemplate ?? '',
        variablesJson: d.variablesJson ?? '',
        isActive: d.isActive,
      });
    }
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const dto = this.form.getRawValue();
    const req = this.isEdit
      ? this.svc.updateTemplate(this.data!.id, dto)
      : this.svc.createTemplate(dto);
    req.subscribe({
      next: (res) => {
        this.saving = false;
        if (res.success) {
          this.notify.success(this.isEdit ? 'Template updated' : 'Template created');
          this.ref.close(true);
        }
      },
      error: (e) => {
        this.saving = false;
        this.notify.error(e.error?.message ?? 'Save failed');
      },
    });
  }
}
