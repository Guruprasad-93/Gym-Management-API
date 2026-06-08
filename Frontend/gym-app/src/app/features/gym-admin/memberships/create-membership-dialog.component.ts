import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MembershipService } from '../../../core/services/membership.service';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';
import { MembershipPlan } from '../../../shared/models/membership-payment.models';

@Component({
  selector: 'app-create-membership-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatSelectModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Assign Membership</h2>
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width"><mat-label>Member</mat-label>
          <mat-select formControlName="memberId">@for (m of members(); track m.id) { <mat-option [value]="m.id">{{ m.fullName }}</mat-option> }</mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Plan</mat-label>
          <mat-select formControlName="membershipPlanId">@for (p of plans(); track p.id) { <mat-option [value]="p.id">{{ p.planName }} — {{ p.price | number:'1.2-2' }}</mat-option> }</mat-select>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width"><mat-label>Start Date</mat-label><input matInput type="date" formControlName="startDate" /></mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancel</button><button mat-flat-button color="primary" [disabled]="form.invalid" (click)="save()">Save</button></mat-dialog-actions>
  `,
  styles: [`.full-width { width:100%; min-width:300px; }`],
})
export class CreateMembershipDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly membershipSvc = inject(MembershipService);
  private readonly memberSvc = inject(MemberService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<CreateMembershipDialogComponent>);
  members = signal<Member[]>([]);
  plans = signal<MembershipPlan[]>([]);
  form = this.fb.nonNullable.group({
    memberId: [null as number | null, Validators.required],
    membershipPlanId: [null as number | null, Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
  });

  ngOnInit(): void {
    this.memberSvc.getPaged(null, { pageNumber: 1, pageSize: 100 }).subscribe((res) => res.success && res.data && this.members.set(res.data.items));
    this.membershipSvc.getPlans().subscribe((res) => res.success && res.data && this.plans.set(res.data));
  }

  save(): void {
    const raw = this.form.getRawValue();
    if (raw.memberId === null || raw.membershipPlanId === null) return;
    this.membershipSvc.create({
      memberId: raw.memberId,
      membershipPlanId: raw.membershipPlanId,
      startDate: raw.startDate,
    }).subscribe({
      next: (res) => { if (res.success) { this.notify.success('Membership created'); this.ref.close(true); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Create failed'),
    });
  }
}
