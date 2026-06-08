import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { GymNotificationService } from '../../../core/services/gym-notification.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { NOTIFICATION_TYPES, NotificationSetting, NotificationTemplate } from '../../../shared/models/notification.models';
import { NotificationTemplateFormDialogComponent } from './notification-template-form-dialog.component';

@Component({
  selector: 'app-notification-template-list',
  standalone: true,
  imports: [
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header title="Notification Templates" subtitle="WhatsApp template mapping per event">
      <button mat-stroked-button routerLink="..">Dashboard</button>
      @if (auth.hasPermission(permissions.ManageNotifications)) {
        <button mat-flat-button color="primary" type="button" (click)="openCreate()">
          <mat-icon>add</mat-icon> Add Template
        </button>
      }
    </app-page-header>

    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else {
      <div class="table-card">
        <table mat-table [dataSource]="templates">
          <ng-container matColumnDef="type"><th mat-header-cell *matHeaderCellDef>Type</th><td mat-cell *matCellDef="let r">{{ r.notificationType }}</td></ng-container>
          <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Template</th><td mat-cell *matCellDef="let r">{{ r.templateName }}</td></ng-container>
          <ng-container matColumnDef="active"><th mat-header-cell *matHeaderCellDef>Active</th><td mat-cell *matCellDef="let r">{{ r.isActive ? 'Yes' : 'No' }}</td></ng-container>
          <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let r">
              @if (auth.hasPermission(permissions.ManageNotifications)) {
                <button mat-icon-button type="button" (click)="openEdit(r)"><mat-icon>edit</mat-icon></button>
                <button mat-icon-button type="button" (click)="remove(r)"><mat-icon>delete</mat-icon></button>
              }
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="cols"></tr>
          <tr mat-row *matRowDef="let row; columns: cols"></tr>
        </table>
      </div>

      <h3>Notification types</h3>
      <div class="settings-grid">
        @for (type of types; track type) {
          <div class="setting-row">
            <span>{{ type }}</span>
            <mat-slide-toggle
              [checked]="isEnabled(type)"
              [disabled]="!auth.hasPermission(permissions.ManageNotifications)"
              (change)="toggle(type, $event.checked)">
            </mat-slide-toggle>
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      .table-card { background: #fff; border-radius: 8px; overflow: auto; margin-bottom: 1.5rem; }
      table { width: 100%; }
      .settings-grid { display: grid; gap: 0.75rem; max-width: 640px; }
      .setting-row { display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid #eee; }
      .center-spinner { margin: 2rem auto; display: block; }
      h3 { margin-top: 1rem; }
    `,
  ],
})
export class NotificationTemplateListComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  private readonly svc = inject(GymNotificationService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  loading = signal(true);
  templates: NotificationTemplate[] = [];
  settings: NotificationSetting[] = [];
  readonly types = NOTIFICATION_TYPES;
  cols = ['type', 'name', 'active', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.svc.getTemplates(true).subscribe({
      next: (res) => {
        if (res.success && res.data) this.templates = res.data;
        this.svc.getSettings().subscribe({
          next: (s) => {
            this.loading.set(false);
            if (s.success && s.data) this.settings = s.data;
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  isEnabled(type: string): boolean {
    return this.settings.find((s) => s.notificationType === type)?.isEnabled ?? true;
  }

  toggle(type: string, enabled: boolean): void {
    this.svc.updateSettings([{ notificationType: type, isEnabled: enabled }]).subscribe({
      next: (res) => res.success && this.notify.success('Setting updated'),
      error: (e) => this.notify.error(e.error?.message ?? 'Update failed'),
    });
  }

  openCreate(): void {
    this.dialog.open(NotificationTemplateFormDialogComponent, { width: '520px', data: null })
      .afterClosed().subscribe((ok) => ok && this.load());
  }

  openEdit(row: NotificationTemplate): void {
    this.dialog.open(NotificationTemplateFormDialogComponent, { width: '520px', data: row })
      .afterClosed().subscribe((ok) => ok && this.load());
  }

  remove(row: NotificationTemplate): void {
    if (!confirm('Delete this template?')) return;
    this.svc.deleteTemplate(row.id).subscribe({
      next: (res) => { if (res.success) { this.notify.success('Deleted'); this.load(); } },
      error: (e) => this.notify.error(e.error?.message ?? 'Delete failed'),
    });
  }
}
