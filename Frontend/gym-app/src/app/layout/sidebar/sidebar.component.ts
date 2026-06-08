import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { AppMenuItem } from '../../core/constants/menu.config';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, MatIconModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnChanges {
  @Input() menuItems: AppMenuItem[] = [];
  @Input() title = 'Gym SaaS';
  @Input() userName = '';
  @Input() userEmail = '';

  private readonly activeOptionsByRoute = new Map<string, { exact: boolean }>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['menuItems']) {
      this.buildActiveOptionsCache();
    }
  }

  /** Use exact match when another menu item is a child route (e.g. Branches vs Branch Dashboard). */
  linkActiveOptions(item: AppMenuItem): { exact: boolean } {
    return this.activeOptionsByRoute.get(item.route) ?? { exact: false };
  }

  private buildActiveOptionsCache(): void {
    this.activeOptionsByRoute.clear();
    for (const item of this.menuItems) {
      const exact = this.menuItems.some(
        (other) => other.route !== item.route && other.route.startsWith(`${item.route}/`)
      );
      this.activeOptionsByRoute.set(item.route, { exact });
    }
  }
}
