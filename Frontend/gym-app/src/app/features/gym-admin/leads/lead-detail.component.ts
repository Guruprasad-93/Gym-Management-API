import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { LeadService } from '../../../core/services/lead.service';
import { MembershipService } from '../../../core/services/membership.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LEAD_STATUS_LABELS, LeadDetail } from '../../../shared/models/lead.models';

@Component({
  selector: 'app-lead-detail',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    PageHeaderComponent,
  ],
  template: `
    @if (detail()) {
      <app-page-header [title]="detail()!.lead.fullName" [subtitle]="detail()!.lead.mobileNumber">
        <button mat-stroked-button type="button" routerLink="/gym-admin/leads">Back</button>
        <button mat-stroked-button type="button" [routerLink]="['/gym-admin/leads/edit', detail()!.lead.id]">Edit</button>
        @if (auth.hasPermission(permissions.ConvertLeads) && detail()!.lead.status !== 'Converted') {
          <button mat-flat-button color="primary" type="button" (click)="showConvert = true">Convert to member</button>
        }
      </app-page-header>

      <div class="layout">
        <section class="card">
          <h3>Lead info</h3>
          <p><strong>Status:</strong> {{ statusLabel(detail()!.lead.status) }}</p>
          <p><strong>Source:</strong> {{ detail()!.lead.leadSource }}</p>
          <p><strong>Trainer:</strong> {{ detail()!.lead.assignedTrainerName || '—' }}</p>
          <p><strong>Plan:</strong> {{ detail()!.lead.interestedPlanName || '—' }}</p>
          <p><strong>Notes:</strong> {{ detail()!.lead.notes || '—' }}</p>
        </section>

        <section class="card">
          <h3>Timeline</h3>
          @for (a of detail()!.activities; track a.id) {
            <div class="timeline-item">
              <strong>{{ a.activityType }}</strong>
              <span>{{ a.description }}</span>
              <small>{{ a.createdDate | date: 'medium' }}</small>
            </div>
          } @empty {
            <p class="empty">No activities yet.</p>
          }
        </section>

        <section class="card">
          <h3>Follow-ups</h3>
          @for (f of detail()!.followUps; track f.id) {
            <div class="timeline-item">
              <strong>{{ f.followUpType }}</strong> — {{ f.status }}
              <span>{{ f.remarks || '' }}</span>
              <small>{{ f.followUpDate | date: 'medium' }}</small>
            </div>
          }
        </section>

        <section class="card">
          <h3>Trial history</h3>
          @for (t of detail()!.trials; track t.id) {
            <div class="timeline-item">
              <strong>{{ t.trialDate | date: 'medium' }}</strong> — {{ t.attendanceStatus }}
              @if (t.feedback) { <span>{{ t.feedback }}</span> }
            </div>
          }
        </section>
      </div>

      @if (showConvert) {
        <div class="convert-overlay">
          <form class="convert-card" [formGroup]="convertForm" (ngSubmit)="convert()">
            <h3>Convert to member</h3>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Membership plan</mat-label>
              <mat-select formControlName="membershipPlanId">
                @for (plan of plans(); track plan.id) {
                  <mat-option [value]="plan.id">{{ plan.name }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Start date</mat-label>
              <input matInput type="date" formControlName="startDate" />
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email (optional)</mat-label>
              <input matInput formControlName="email" />
            </mat-form-field>
            <div class="actions">
              <button mat-button type="button" (click)="showConvert = false">Cancel</button>
              <button mat-flat-button color="primary" type="submit" [disabled]="convertForm.invalid">Convert</button>
            </div>
          </form>
        </div>
      }
    } @else if (loading()) {
      <mat-spinner class="center-spinner" />
    }
  `,
  styles: [
    `
      .layout { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 1rem; }
      .card { background: #fff; border-radius: 8px; padding: 1rem; }
      .timeline-item { border-left: 3px solid #1565c0; padding-left: 10px; margin-bottom: 12px; display: flex; flex-direction: column; gap: 2px; }
      .timeline-item small { color: #666; }
      .empty { color: #888; }
      .convert-overlay { position: fixed; inset: 0; background: rgba(0,0,0,.4); display: flex; align-items: center; justify-content: center; z-index: 100; }
      .convert-card { background: #fff; padding: 1.5rem; border-radius: 8px; width: min(420px, 90vw); }
      .full-width { width: 100%; }
      .actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 1rem; }
      .center-spinner { margin: 2rem auto; display: block; }
    `,
  ],
})
export class LeadDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly leadService = inject(LeadService);
  private readonly planService = inject(MembershipService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;

  detail = signal<LeadDetail | null>(null);
  loading = signal(true);
  plans = signal<{ id: number; name: string }[]>([]);
  showConvert = false;

  convertForm = this.fb.group({
    membershipPlanId: [null as number | null, Validators.required],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    email: [''],
  });

  ngOnInit(): void {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.planService.getPlans().subscribe((res) => {
      this.plans.set((res.data ?? []).map((p) => ({ id: p.id, name: p.planName })));
    });
    this.leadService.getById(id).subscribe({
      next: (res) => {
        this.detail.set(res.data!);
        this.convertForm.patchValue({ email: res.data!.lead.email ?? '' });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  statusLabel(s: string): string {
    return LEAD_STATUS_LABELS[s] ?? s;
  }

  convert(): void {
    if (this.convertForm.invalid || !this.detail()) return;
    const v = this.convertForm.getRawValue();
    this.leadService.convertToMember({
      leadId: this.detail()!.lead.id,
      membershipPlanId: v.membershipPlanId!,
      startDate: v.startDate!,
      email: v.email || undefined,
    }).subscribe({
      next: (res) => {
        const pwd = res.data?.temporaryPassword;
        this.notify.success(pwd ? `Converted. Temp password: ${pwd}` : 'Lead converted to member');
        this.showConvert = false;
        this.ngOnInit();
      },
      error: (err) => this.notify.error(err?.error?.message ?? 'Conversion failed'),
    });
  }
}
