import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TrainerService } from '../../../core/services/trainer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { Member } from '../../../shared/models/member.models';
import { Trainer, TrainerDashboard } from '../../../shared/models/trainer.models';
import { AssignMembersDialogComponent } from './assign-members-dialog.component';
import { TrainerFormDialogComponent } from './trainer-form-dialog.component';
import { ProfilePhotoManagerComponent } from '../../../shared/components/profile-photo-manager/profile-photo-manager.component';
import { FileCategories } from '../../../shared/models/file.models';

@Component({
  selector: 'app-trainer-detail',
  standalone: true,
  imports: [
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatTooltipModule,
    PageHeaderComponent,
    ProfilePhotoManagerComponent,
  ],
  template: `
    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else if (trainer()) {
      <app-page-header
        [title]="trainer()!.fullName || 'Trainer #' + trainer()!.id"
        [subtitle]="trainer()!.specialization || 'Trainer profile'">
        <button mat-button type="button" routerLink="/gym-admin/trainers">
          <mat-icon>arrow_back</mat-icon> Back
        </button>
        @if (auth.hasPermission(permissions.UpdateTrainer)) {
          <button mat-flat-button color="primary" type="button" (click)="openEdit()">
            <mat-icon>edit</mat-icon> Edit
          </button>
        }
        @if (auth.hasPermission(permissions.AssignMemberToTrainer)) {
          <button mat-stroked-button type="button" (click)="openAssign()">
            <mat-icon>group_add</mat-icon> Assign Members
          </button>
        }
      </app-page-header>

      @if (dashboard()) {
        <div class="stats-grid">
          <mat-card>
            <mat-card-title>Active Members</mat-card-title>
            <mat-card-content class="stat">{{ dashboard()!.assignedActiveMembers }}</mat-card-content>
          </mat-card>
          <mat-card>
            <mat-card-title>Inactive Members</mat-card-title>
            <mat-card-content class="stat">{{ dashboard()!.assignedInactiveMembers }}</mat-card-content>
          </mat-card>
          <mat-card>
            <mat-card-title>Unassigned in Gym</mat-card-title>
            <mat-card-content class="stat">{{ dashboard()!.unassignedMembersInGym }}</mat-card-content>
          </mat-card>
          <mat-card>
            <mat-card-title>Active Diet Plans</mat-card-title>
            <mat-card-content class="stat">{{ dashboard()!.activeDietPlans }}</mat-card-content>
          </mat-card>
          <mat-card>
            <mat-card-title>Active Workout Plans</mat-card-title>
            <mat-card-content class="stat">{{ dashboard()!.activeWorkoutPlans }}</mat-card-content>
          </mat-card>
        </div>
      }

      <mat-card class="info-card">
        <mat-card-header>
          <mat-card-title>Profile</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-profile-photo-manager
            title="Profile photo"
            [category]="fileCategories.TrainerProfilePhoto"
            [trainerId]="trainer()!.id"
            uploadLabel="Upload profile photo" />
          <p><strong>Email:</strong> {{ trainer()!.email || '—' }}</p>
          <p><strong>Bio:</strong> {{ trainer()!.bio || '—' }}</p>
          <p><strong>Status:</strong> {{ trainer()!.isActive ? 'Active' : 'Inactive' }}</p>
          <p><strong>Member count:</strong> {{ trainer()!.assignedMemberCount }}</p>
        </mat-card-content>
      </mat-card>

      <mat-card class="members-card">
        <mat-card-header>
          <mat-card-title>Assigned Members</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          @if (members().length === 0) {
            <p class="empty">No members assigned yet.</p>
          } @else {
            <table mat-table [dataSource]="members()" class="members-table">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let row">{{ row.fullName }}</td>
              </ng-container>
              <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef>Email</th>
                <td mat-cell *matCellDef="let row">{{ row.email }}</td>
              </ng-container>
              <ng-container matColumnDef="phone">
                <th mat-header-cell *matHeaderCellDef>Phone</th>
                <td mat-cell *matCellDef="let row">{{ row.phone || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef></th>
                <td mat-cell *matCellDef="let row">
                  @if (auth.hasPermission(permissions.AssignMemberToTrainer)) {
                    <button
                      mat-icon-button
                      color="warn"
                      type="button"
                      (click)="unassign(row)"
                      matTooltip="Remove assignment">
                      <mat-icon>person_remove</mat-icon>
                    </button>
                  }
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="memberColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: memberColumns"></tr>
            </table>
          }
        </mat-card-content>
      </mat-card>
    }
  `,
  styles: [
    `
      .stats-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
        gap: 1rem;
        margin-bottom: 1rem;
      }
      .stat {
        font-size: 2rem;
        font-weight: 600;
        padding-top: 0.5rem;
      }
      .info-card,
      .members-card {
        margin-bottom: 1rem;
      }
      .members-table {
        width: 100%;
      }
      .empty {
        color: #666;
      }
      .center-spinner {
        margin: 2rem auto;
        display: block;
      }
    `,
  ],
})
export class TrainerDetailComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  readonly fileCategories = FileCategories;
  private readonly route = inject(ActivatedRoute);
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  trainer = signal<Trainer | null>(null);
  dashboard = signal<TrainerDashboard | null>(null);
  members = signal<Member[]>([]);
  loading = signal(true);

  memberColumns = ['name', 'email', 'phone', 'actions'];

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.load(id);
  }

  load(id: number): void {
    this.loading.set(true);
    this.trainerService.getById(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.trainer.set(res.data);
          this.loadDashboard(id);
          this.loadMembers(id);
        } else {
          this.loading.set(false);
        }
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Trainer not found');
      },
    });
  }

  loadDashboard(id: number): void {
    this.trainerService.getDashboard(id).subscribe({
      next: (res) => {
        if (res.success && res.data) this.dashboard.set(res.data);
      },
    });
  }

  loadMembers(id: number): void {
    this.trainerService.getMembers(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.members.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openEdit(): void {
    const t = this.trainer();
    if (!t) return;
    const ref = this.dialog.open(TrainerFormDialogComponent, { width: '480px', data: t });
    ref.afterClosed().subscribe((ok) => ok && this.load(t.id));
  }

  openAssign(): void {
    const t = this.trainer();
    if (!t) return;
    const ref = this.dialog.open(AssignMembersDialogComponent, { width: '560px', data: t });
    ref.afterClosed().subscribe((ok) => ok && this.load(t.id));
  }

  unassign(member: Member): void {
    if (!confirm(`Remove ${member.fullName} from this trainer?`)) return;
    this.trainerService.removeMemberAssignment(member.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('Assignment removed');
          const t = this.trainer();
          if (t) this.load(t.id);
        }
      },
      error: () => this.notify.error('Failed to remove assignment'),
    });
  }
}
