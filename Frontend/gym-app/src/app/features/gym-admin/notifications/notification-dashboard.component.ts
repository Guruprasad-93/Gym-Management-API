import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GymNotificationService } from '../../../core/services/gym-notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { NotificationDashboard } from '../../../shared/models/notification.models';

@Component({
  selector: 'app-notification-dashboard',
  standalone: true,
  imports: [RouterLink, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './notification-dashboard.component.html',
  styleUrl: './notification-dashboard.component.css',
})
export class NotificationDashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(GymNotificationService);

  loading = signal(true);
  dashboard = signal<NotificationDashboard | null>(null);

  ngOnInit(): void {
    this.svc.getDashboard().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.dashboard.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }
}
