import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { PrivilegeService } from '../../../core/services/privilege.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-privilege-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Create Privilege</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="privilegeName" placeholder="VIEW_REPORTS" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Category</mat-label>
          <input matInput formControlName="category" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button color="primary" type="button" (click)="save()">Save</button>
    </mat-dialog-actions>
  `,
  styles: [`.full-width { width: 100%; min-width: 300px; }`],
})
export class PrivilegeFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly privilegeService = inject(PrivilegeService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<PrivilegeFormDialogComponent>);

  readonly form = this.fb.nonNullable.group({
    privilegeName: ['', Validators.required],
    category: ['General', Validators.required],
    description: [''],
  });

  save(): void {
    if (this.form.invalid) return;
    this.privilegeService.create(this.form.getRawValue()).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Privilege created');
          this.ref.close(true);
        }
      },
      error: (err) => this.notify.error(err.error?.message ?? 'Create failed'),
    });
  }
}
