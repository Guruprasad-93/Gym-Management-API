import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MobilePushService } from '../../../core/services/mobile-push.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { PushCampaignHistory, PushNotificationAnalytics } from '../../../shared/models/mobile-push.models';

@Component({
  selector: 'app-mobile-analytics',
  standalone: true,
  imports: [
    DatePipe,
    RouterLink,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    SaasKpiCardComponent,
  ],
  templateUrl: './mobile-analytics.component.html',
  styleUrl: './mobile-analytics.component.css',
})
export class MobileAnalyticsComponent implements OnInit {
  private readonly svc = inject(MobilePushService);

  loading = signal(true);
  analytics = signal<PushNotificationAnalytics | null>(null);
  campaigns = signal<PushCampaignHistory[]>([]);
  displayedColumns = ['title', 'sentDate', 'recipientCount', 'sentCount'];

  ngOnInit(): void {
    this.svc.getAnalytics().subscribe({
      next: (res) => {
        if (res.success && res.data) this.analytics.set(res.data);
        this.loadCampaigns();
      },
      error: () => this.loading.set(false),
    });
  }

  private loadCampaigns(): void {
    this.svc.getCampaigns().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.campaigns.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }
}
