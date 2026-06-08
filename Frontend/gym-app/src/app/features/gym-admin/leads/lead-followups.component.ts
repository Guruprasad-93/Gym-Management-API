import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LeadService } from '../../../core/services/lead.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LeadFollowUp } from '../../../shared/models/lead.models';

@Component({
  selector: 'app-lead-followups',
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
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Pending Follow-ups" subtitle="Upcoming lead follow-up tasks">
      <button mat-stroked-button type="button" routerLink="/gym-admin/leads">Back to leads</button>
    </app-page-header>

    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else {
      <div class="list">
        @for (f of followUps(); track f.id) {
          <div class="item">
            <div>
              <strong>{{ f.leadName }}</strong>
              <span>{{ f.mobileNumber }}</span>
            </div>
            <div>{{ f.followUpType }} — {{ f.followUpDate | date: 'medium' }}</div>
            <div>{{ f.remarks || 'No remarks' }}</div>
            <a [routerLink]="['/gym-admin/leads', f.leadId]">View lead</a>
          </div>
        } @empty {
          <p>No pending follow-ups.</p>
        }
      </div>
    }
  `,
  styles: [
    `
      .list { display: flex; flex-direction: column; gap: 12px; }
      .item { background: #fff; padding: 1rem; border-radius: 8px; display: grid; gap: 4px; }
      .item span { color: #666; margin-left: 8px; }
      .center-spinner { margin: 2rem auto; display: block; }
    `,
  ],
})
export class LeadFollowupsComponent implements OnInit {
  private readonly leadService = inject(LeadService);
  private readonly notify = inject(NotificationService);
  followUps = signal<LeadFollowUp[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.leadService.getPendingFollowUps().subscribe({
      next: (res) => {
        this.followUps.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load follow-ups');
        this.loading.set(false);
      },
    });
  }
}
