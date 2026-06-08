import { Component, Input, Output, EventEmitter, inject, ChangeDetectionStrategy } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-header',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatIconModule, MatButtonModule, MatMenuModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  @Input() title = '';
  @Input() userName = '';
  @Input() userEmail = '';
  @Output() menuToggle = new EventEmitter<void>();

  private readonly auth = inject(AuthService);

  get searchPlaceholder(): string {
    const normalized = this.title.toLowerCase();
    if (normalized.includes('super')) return 'Search gyms, members, subscriptions...';
    if (normalized.includes('gym admin')) return 'Search members, trainers, payments...';
    if (normalized.includes('member')) return 'Search goals, workouts, progress...';
    if (normalized.includes('trainer')) return 'Search members, sessions, schedules...';
    return 'Search...';
  }

  logout(): void {
    this.auth.logout();
  }
}
