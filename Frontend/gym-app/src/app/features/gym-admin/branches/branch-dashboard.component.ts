import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BranchService } from '../../../core/services/branch.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { BranchDashboardItem } from '../../../shared/models/branch.models';

@Component({
  selector: 'app-branch-dashboard',
  standalone: true,
  imports: [RouterLink, MatIconModule, MatProgressSpinnerModule, SaasKpiCardComponent],
  templateUrl: './branch-dashboard.component.html',
  styleUrl: './branch-dashboard.component.css',
})
export class BranchDashboardComponent implements OnInit {
  private readonly service = inject(BranchService);
  private readonly notify = inject(NotificationService);

  loading = signal(true);
  items = signal<BranchDashboardItem[]>([]);

  ngOnInit(): void {
    this.service.getDashboard().subscribe({
      next: (r) => {
        this.loading.set(false);
        if (r.success && r.data) this.items.set(r.data);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load branch dashboard');
      },
    });
  }
}
