import { Injectable, inject, computed } from '@angular/core';
import { AuthService } from './auth.service';
import {
  AppMenuItem,
  GYM_ADMIN_MENU,
  MEMBER_MENU,
  SUPER_ADMIN_MENU,
  TRAINER_MENU,
  filterMenuItems,
} from '../constants/menu.config';
import { Roles } from '../constants/roles';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly auth = inject(AuthService);

  readonly superAdminMenu = computed(() =>
    filterMenuItems(
      SUPER_ADMIN_MENU,
      this.auth.user()?.roles ?? [],
      this.auth.user()?.permissions ?? []
    )
  );

  readonly gymAdminMenu = computed(() =>
    filterMenuItems(
      GYM_ADMIN_MENU,
      this.auth.user()?.roles ?? [],
      this.auth.user()?.permissions ?? []
    )
  );

  readonly trainerMenu = computed(() =>
    filterMenuItems(
      TRAINER_MENU,
      this.auth.user()?.roles ?? [],
      this.auth.user()?.permissions ?? []
    )
  );

  readonly memberMenu = computed(() =>
    filterMenuItems(
      MEMBER_MENU,
      this.auth.user()?.roles ?? [],
      this.auth.user()?.permissions ?? []
    )
  );

  getMenuForCurrentUser(): AppMenuItem[] {
    const user = this.auth.user();
    if (!user) return [];
    if (user.roles.includes(Roles.SuperAdmin)) return this.superAdminMenu();
    if (user.roles.includes(Roles.GymAdmin)) return this.gymAdminMenu();
    if (user.roles.includes(Roles.Trainer)) return this.trainerMenu();
    if (user.roles.includes(Roles.Member)) return this.memberMenu();
    return [];
  }
}
