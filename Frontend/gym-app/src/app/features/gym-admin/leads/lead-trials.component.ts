import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LeadService } from '../../../core/services/lead.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { LeadTrial } from '../../../shared/models/lead.models';

@Component({
  selector: 'app-lead-trials',
  standalone: true,
  imports: [DatePipe, RouterModule, MatButtonModule, MatProgressSpinnerModule, PageHeaderComponent],
  template: `
    <app-page-header title="Today's Trials" subtitle="Scheduled trial sessions for today">
      <button mat-stroked-button type="button" routerLink="/gym-admin/leads">Back to leads</button>
    </app-page-header>

    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else {
      <div class="list">
        @for (t of trials(); track t.id) {
          <div class="item">
            <strong>{{ t.leadName }}</strong>
            <span>{{ t.mobileNumber }}</span>
            <div>{{ t.trialDate | date: 'medium' }} — {{ t.trainerName || 'Unassigned' }}</div>
            <div>Status: {{ t.attendanceStatus }}</div>
            <a [routerLink]="['/gym-admin/leads', t.leadId]">View lead</a>
          </div>
        } @empty {
          <p>No trials scheduled for today.</p>
        }
      </div>
    }
  `,
  styles: [
    `
      .list { display: flex; flex-direction: column; gap: 12px; }
      .item { background: #fff; padding: 1rem; border-radius: 8px; display: grid; gap: 4px; }
      .center-spinner { margin: 2rem auto; display: block; }
    `,
  ],
})
export class LeadTrialsComponent implements OnInit {
  private readonly leadService = inject(LeadService);
  private readonly notify = inject(NotificationService);
  trials = signal<LeadTrial[]>([]);
  loading = signal(true);

  ngOnInit(): void {
    this.leadService.getTodaysTrials().subscribe({
      next: (res) => {
        this.trials.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.notify.error('Failed to load trials');
        this.loading.set(false);
      },
    });
  }
}
