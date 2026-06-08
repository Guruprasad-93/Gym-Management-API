import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { TrainerService } from '../../../core/services/trainer.service';
import { TrainerDashboard } from '../../../shared/models/trainer.models';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';

@Component({
  selector: 'app-trainer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './trainer-dashboard.component.html',
  styleUrl: './trainer-dashboard.component.css',
})
export class TrainerDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);

  stats: TrainerDashboard | null = null;
  loading = true;

  ngOnInit(): void {
    this.trainerService.getMe().subscribe({
      next: (meRes) => {
        if (!meRes.success || !meRes.data) {
          this.loading = false;
          this.notify.error('Trainer profile not found');
          return;
        }
        this.trainerService.getDashboard(meRes.data.id).subscribe({
          next: (res) => {
            this.loading = false;
            if (res.success && res.data) this.stats = res.data;
          },
          error: () => {
            this.loading = false;
            this.notify.error('Failed to load dashboard');
          },
        });
      },
      error: () => {
        this.loading = false;
        this.notify.error('Failed to load trainer profile');
      },
    });
  }
}
