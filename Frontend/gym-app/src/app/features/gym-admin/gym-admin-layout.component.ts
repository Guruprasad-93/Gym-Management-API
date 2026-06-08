import { Component, inject, OnInit } from '@angular/core';
import { MainLayoutComponent } from '../../layout/main-layout/main-layout.component';
import { MenuService } from '../../core/services/menu.service';
import { AuthService } from '../../core/services/auth.service';
import { untilDestroyed } from '../../core/utils/destroy-ref.util';

@Component({
  selector: 'app-gym-admin-layout',
  standalone: true,
  imports: [MainLayoutComponent],
  template: `
    <app-main-layout
      [menuItems]="menuService.gymAdminMenu()"
      title="Gym Admin"
      [userName]="auth.user()?.name ?? ''"
      [userEmail]="auth.user()?.email ?? ''" />
  `,
})
export class GymAdminLayoutComponent implements OnInit {
  readonly menuService = inject(MenuService);
  readonly auth = inject(AuthService);

  ngOnInit(): void {
    this.auth.refreshPermissions().pipe(untilDestroyed()).subscribe();
  }
}
