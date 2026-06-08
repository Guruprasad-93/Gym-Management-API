import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { AppMenuItem } from '../../core/constants/menu.config';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterModule, MatSidenavModule, SidebarComponent, HeaderComponent],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css',
})
export class MainLayoutComponent {
  @Input() menuItems: AppMenuItem[] = [];
  @Input() title = 'Gym SaaS';
  @Input() userName = '';
  @Input() userEmail = '';

  sidenavOpened = true;

  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }

  onSidenavOpenedChange(opened: boolean): void {
    this.sidenavOpened = opened;
  }
}
