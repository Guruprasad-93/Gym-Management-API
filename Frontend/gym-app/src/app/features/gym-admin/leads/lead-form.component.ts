import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LeadService } from '../../../core/services/lead.service';
import { MembershipService } from '../../../core/services/membership.service';
import { TrainerService } from '../../../core/services/trainer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LEAD_SOURCES, LEAD_STATUSES } from '../../../shared/models/lead.models';

@Component({
  selector: 'app-lead-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header [title]="isEdit() ? 'Edit Lead' : 'Create Lead'" subtitle="Capture prospect details">
      <button mat-stroked-button type="button" routerLink="/gym-admin/leads">Back to list</button>
    </app-page-header>

    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else {
      <form class="form-card" [formGroup]="form" (ngSubmit)="submit()">
        <div class="grid">
          <mat-form-field appearance="outline">
            <mat-label>Full name</mat-label>
            <input matInput formControlName="fullName" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Mobile</mat-label>
            <input matInput formControlName="mobileNumber" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput formControlName="email" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Gender</mat-label>
            <input matInput formControlName="gender" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Age</mat-label>
            <input matInput type="number" formControlName="age" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Lead source</mat-label>
            <mat-select formControlName="leadSource">
              @for (src of sources; track src) {
                <mat-option [value]="src">{{ src }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              @for (s of statuses; track s) {
                <mat-option [value]="s">{{ s }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Interested plan</mat-label>
            <mat-select formControlName="interestedPlanId">
              <mat-option [value]="null">None</mat-option>
              @for (plan of plans(); track plan.id) {
                <mat-option [value]="plan.id">{{ plan.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Assigned trainer</mat-label>
            <mat-select formControlName="assignedTrainerId">
              <mat-option [value]="null">None</mat-option>
              @for (t of trainers(); track t.id) {
                <mat-option [value]="t.id">{{ t.fullName }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        </div>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Address</mat-label>
          <textarea matInput rows="2" formControlName="address"></textarea>
        </mat-form-field>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Notes</mat-label>
          <textarea matInput rows="3" formControlName="notes"></textarea>
        </mat-form-field>
        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || saving()">
            {{ isEdit() ? 'Save changes' : 'Create lead' }}
          </button>
        </div>
      </form>
    }
  `,
  styles: [
    `
      .form-card { background: #fff; padding: 1.5rem; border-radius: 8px; max-width: 960px; }
      .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; }
      .full-width { width: 100%; }
      .actions { margin-top: 1rem; }
      .center-spinner { margin: 2rem auto; display: block; }
    `,
  ],
})
export class LeadFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly leadService = inject(LeadService);
  private readonly planService = inject(MembershipService);
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);

  readonly sources = LEAD_SOURCES;
  readonly statuses = LEAD_STATUSES;
  isEdit = signal(false);
  loading = signal(false);
  saving = signal(false);
  leadId = signal<number | null>(null);
  plans = signal<{ id: number; name: string }[]>([]);
  trainers = signal<{ id: number; fullName: string }[]>([]);

  form = this.fb.group({
    fullName: ['', Validators.required],
    mobileNumber: ['', Validators.required],
    email: [''],
    gender: [''],
    age: [null as number | null],
    address: [''],
    leadSource: ['WalkIn', Validators.required],
    interestedPlanId: [null as number | null],
    status: ['New', Validators.required],
    assignedTrainerId: [null as number | null],
    notes: [''],
  });

  ngOnInit(): void {
    this.planService.getPlans().subscribe((res) => {
      this.plans.set((res.data ?? []).map((p) => ({ id: p.id, name: p.planName })));
    });
    this.trainerService.getPaged(null, { pageNumber: 1, pageSize: 100 }).subscribe((res) => {
      this.trainers.set((res.data?.items ?? []).map((t) => ({ id: t.id, fullName: t.fullName ?? 'Trainer' })));
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.leadId.set(+id);
      this.loading.set(true);
      this.leadService.getById(+id).subscribe({
        next: (res) => {
          const l = res.data!.lead;
          this.form.patchValue({
            fullName: l.fullName,
            mobileNumber: l.mobileNumber,
            email: l.email ?? '',
            gender: l.gender ?? '',
            age: l.age ?? null,
            address: l.address ?? '',
            leadSource: l.leadSource,
            interestedPlanId: l.interestedPlanId ?? null,
            status: l.status,
            assignedTrainerId: l.assignedTrainerId ?? null,
            notes: l.notes ?? '',
          });
          this.loading.set(false);
        },
        error: () => {
          this.notify.error('Lead not found');
          this.router.navigate(['/gym-admin/leads']);
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const value = this.form.getRawValue();
    const payload = {
      ...value,
      email: value.email || undefined,
      age: value.age ?? undefined,
      interestedPlanId: value.interestedPlanId ?? undefined,
      assignedTrainerId: value.assignedTrainerId ?? undefined,
    };

    const req = this.isEdit()
      ? this.leadService.update(this.leadId()!, payload as never)
      : this.leadService.create(payload as never);

    req.subscribe({
      next: () => {
        this.notify.success(this.isEdit() ? 'Lead updated' : 'Lead created');
        this.router.navigate(['/gym-admin/leads']);
      },
      error: (err) => {
        this.notify.error(err?.error?.message ?? 'Save failed');
        this.saving.set(false);
      },
    });
  }
}
