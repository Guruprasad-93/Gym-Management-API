import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Membership } from '../../../shared/models/membership-payment.models';

@Component({
  selector: 'app-renew-membership-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Renew Membership</h2>
    <p>{{ membership.memberName }} — {{ membership.planName }}</p>
    <mat-dialog-content>
      <form [formGroup]="form"><mat-form-field appearance="outline" class="full-width"><mat-label>Notes</mat-label><textarea matInput formControlName="notes"></textarea></mat-form-field></form>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancel</button><button mat-flat-button color="primary" (click)="save()">Renew</button></mat-dialog-actions>
  `,
  styles: [`.full-width { width:100%; }`],
})
export class RenewMembershipDialogComponent {
  readonly membership = inject<Membership>(MAT_DIALOG_DATA);
  private readonly fb = inject(FormBuilder);
  private readonly svc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<RenewMembershipDialogComponent>);
  form = this.fb.nonNullable.group({ notes: [''] });

  save(): void {
    this.svc.renew(this.membership.id, this.form.getRawValue()).subscribe({
      next: (res) => { if (res.success) { this.notify.success('Renewed'); this.ref.close(true); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Renew failed'),
    });
  }
}
