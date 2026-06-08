import { Component, OnInit, inject, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { ReferralStats } from '../../../shared/models/member-self-service.models';

@Component({
  selector: 'app-member-referrals',
  standalone: true,
  imports: [MatIconModule, SaasKpiCardComponent],
  templateUrl: './member-referrals.component.html',
  styleUrl: './member-referrals.component.css',
})
export class MemberReferralsComponent implements OnInit {
  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  stats = signal<ReferralStats | null>(null);

  ngOnInit(): void {
    this.service.getReferrals().subscribe({
      next: (r) => {
        if (r.success) this.stats.set(r.data ?? null);
      },
      error: () => this.notify.error('Failed to load referrals'),
    });
  }

  copyCode(): void {
    const code = this.stats()?.referralCode;
    if (code) {
      navigator.clipboard.writeText(code);
      this.notify.success('Referral code copied');
    }
  }
}
