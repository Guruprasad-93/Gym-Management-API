import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { RoleService } from '../../../core/services/role.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Role } from '../../../shared/models/role.models';

@Component({
  selector: 'app-role-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>{{ data ? 'Edit Role' : 'Create Role' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Role Name</mat-label>
          <input matInput formControlName="roleName" />
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput rows="2" formControlName="description"></textarea>
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
export class RoleFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly roleService = inject(RoleService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<RoleFormDialogComponent>);
  readonly data = inject<Role | null>(MAT_DIALOG_DATA);

  readonly form = this.fb.nonNullable.group({
    roleName: [this.data?.roleName ?? '', Validators.required],
    description: [this.data?.description ?? ''],
  });

  save(): void {
    if (this.form.invalid) return;
    const dto = this.form.getRawValue();
    const req = this.data
      ? this.roleService.update(this.data.id, dto)
      : this.roleService.create(dto);
    req.subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Saved');
          this.ref.close(true);
        }
      },
      error: (err) => this.notify.error(err.error?.message ?? 'Save failed'),
    });
  }
}
