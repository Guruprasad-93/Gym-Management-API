import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MobilePushService } from '../../../core/services/mobile-push.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PushTemplates } from '../../../shared/models/mobile-push.models';

@Component({
  selector: 'app-mobile-notifications',
  standalone: true,
  imports: [FormsModule, RouterLink, MatIconModule],
  templateUrl: './mobile-notifications.component.html',
  styleUrl: './mobile-notifications.component.css',
})
export class MobileNotificationsComponent implements OnInit {
  private readonly svc = inject(MobilePushService);
  private readonly notify = inject(NotificationService);

  templates = PushTemplates;
  selectedTemplateTitle = PushTemplates[0].title;
  title = '';
  message = '';
  sending = signal(false);

  ngOnInit(): void {
    this.applyTemplate();
  }

  applyTemplate(): void {
    const template = this.templates.find((t) => t.title === this.selectedTemplateTitle) ?? this.templates[0];
    this.title = template.title;
    this.message = template.message;
  }

  send(): void {
    this.sending.set(true);
    this.svc.sendCampaign({ title: this.title, message: this.message }).subscribe({
      next: (res) => {
        this.sending.set(false);
        this.notify.success(res.message ?? 'Campaign sent.');
      },
      error: () => {
        this.sending.set(false);
        this.notify.error('Failed to send campaign.');
      },
    });
  }
}
