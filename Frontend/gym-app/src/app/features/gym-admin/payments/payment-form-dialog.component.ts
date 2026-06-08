import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { PaymentService } from '../../../core/services/payment.service';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Membership } from '../../../shared/models/membership-payment.models';

const METHODS = ['Cash', 'UPI', 'Card', 'Bank Transfer'];

@Component({
  selector: 'app-payment-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Record Payment</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width"><mat-label>Membership</mat-label>
          <mat-select formControlName="membershipId" (selectionChange)="onMembershipChange($event.value)">
            @for (m of memberships; track m.id) {
              <mat-option [value]="m.id">{{ m.memberName }} — {{ m.planName }} ({{ m.amount ?? m.planPrice }})</mat-option>
            }
          </mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Amount</mat-label><input matInput type="number" formControlName="amount" readonly /></mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Method</mat-label>
          <mat-select formControlName="paymentMethod">@for (m of methods; track m) { <mat-option [value]="m">{{ m }}</mat-option> }</mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Reference</mat-label><input matInput formControlName="transactionReference" /></mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Payment Date</mat-label><input matInput type="datetime-local" formControlName="paymentDate" /></mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancel</button><button mat-flat-button color="primary" [disabled]="form.invalid" (click)="save()">Save</button></mat-dialog-actions>
  `,
  styles: [`.full-width { width:100%; min-width:320px; }`],
})
export class PaymentFormDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly paymentSvc = inject(PaymentService);
  private readonly membershipSvc = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<PaymentFormDialogComponent>);
  methods = METHODS;
  memberships: Membership[] = [];
  form = this.fb.nonNullable.group({
    membershipId: [null as number | null, Validators.required],
    memberId: [0, Validators.min(1)],
    amount: [0, Validators.min(0.01)],
    paymentMethod: ['Cash', Validators.required],
    transactionReference: [''],
    paymentDate: [new Date().toISOString().slice(0, 16), Validators.required],
  });

  ngOnInit(): void {
    this.membershipSvc.getAll(undefined, false).subscribe((res) => {
      if (res.success && res.data) this.memberships = res.data.filter((m) => m.status === 'Active');
    });
  }

  onMembershipChange(id: number | null): void {
    if (id === null) return;
    const m = this.memberships.find((x) => x.id === id);
    if (m) this.form.patchValue({ memberId: m.memberId, amount: m.amount ?? m.planPrice });
  }

  save(): void {
    const raw = this.form.getRawValue();
    if (raw.membershipId === null) return;
    this.paymentSvc.create({
      memberId: raw.memberId,
      membershipId: raw.membershipId,
      amount: raw.amount,
      paymentMethod: raw.paymentMethod,
      transactionReference: raw.transactionReference || undefined,
      paymentDate: new Date(raw.paymentDate).toISOString(),
    }).subscribe({
      next: (res) => { if (res.success) { this.notify.success('Payment recorded'); this.ref.close(true); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Payment failed'),
    });
  }
}
