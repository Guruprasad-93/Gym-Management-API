import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { MemberDetails } from '../../../shared/models/member.models';
import { MemberFormDialogComponent } from './member-form-dialog.component';
import { AssignTrainerDialogComponent } from './assign-trainer-dialog.component';
import { ProfilePhotoManagerComponent } from '../../../shared/components/profile-photo-manager/profile-photo-manager.component';
import { MemberFilesGalleryComponent } from '../../../shared/components/member-files-gallery/member-files-gallery.component';
import { FileCategories } from '../../../shared/models/file.models';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTabsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatTooltipModule,
    PageHeaderComponent,
    ProfilePhotoManagerComponent,
    MemberFilesGalleryComponent,
  ],
  template: `
    @if (loading()) {
      <mat-spinner class="center-spinner" />
    } @else if (member()) {
      <app-page-header [title]="member()!.fullName" subtitle="Member profile">
        <button mat-button type="button" routerLink="/gym-admin/members">
          <mat-icon>arrow_back</mat-icon> Back
        </button>
        @if (auth.hasPermission(permissions.UpdateMember)) {
          <button mat-flat-button color="primary" type="button" (click)="openEdit()">
            <mat-icon>edit</mat-icon> Edit
          </button>
        }
        @if (auth.hasPermission(permissions.AssignTrainer)) {
          <button mat-stroked-button type="button" (click)="openAssignTrainer()">
            <mat-icon>person_pin</mat-icon> Assign Trainer
          </button>
        }
        @if (auth.hasPermission(permissions.ViewMemberDiet)) {
          <button mat-stroked-button type="button" [routerLink]="['diet']">
            <mat-icon>restaurant_menu</mat-icon> Diet Plan
          </button>
        }
        @if (auth.hasPermission(permissions.ViewMemberWorkout)) {
          <button mat-stroked-button type="button" [routerLink]="['workout']">
            <mat-icon>fitness_center</mat-icon> Workout
          </button>
        }
      </app-page-header>

      <mat-card class="profile-card">
        <mat-card-content>
          <div class="profile-top">
            <app-profile-photo-manager
              title="Profile photo"
              [category]="fileCategories.MemberProfilePhoto"
              [memberId]="member()!.id"
              uploadLabel="Upload profile photo" />
          </div>
          <div class="profile-grid">
            <div><strong>Email:</strong> {{ member()!.email }}</div>
            <div><strong>Phone:</strong> {{ member()!.phone || '—' }}</div>
            <div><strong>Gender:</strong> {{ member()!.gender || '—' }}</div>
            <div><strong>Age:</strong> {{ member()!.age ?? '—' }}</div>
            <div><strong>Height:</strong> {{ member()!.height ? member()!.height + ' cm' : '—' }}</div>
            <div><strong>Weight:</strong> {{ member()!.weight ? member()!.weight + ' kg' : '—' }}</div>
            <div><strong>Address:</strong> {{ member()!.address || '—' }}</div>
            <div><strong>Emergency:</strong> {{ member()!.emergencyContact || '—' }}</div>
            <div><strong>Join Date:</strong> {{ member()!.joinDate }}</div>
            <div><strong>Trainer:</strong> {{ member()!.trainerName || 'Gym Admin' }}</div>
            <div><strong>Status:</strong> {{ member()!.isActive ? 'Active' : 'Inactive' }}</div>
          </div>
        </mat-card-content>
      </mat-card>

      <mat-tab-group>
        <mat-tab label="Membership">
          <div class="tab-content">
            <p><strong>Status:</strong> {{ member()!.membershipStatus || 'None' }}</p>
            <p><strong>Plan:</strong> {{ member()!.membershipPlanName || '—' }}</p>
            <p><strong>End Date:</strong> {{ member()!.membershipEndDate || '—' }}</p>
          </div>
        </mat-tab>
        <mat-tab label="Payments">
          <div class="tab-content">
            @if (member()!.paymentHistory.length === 0) {
              <p class="empty">No payment history.</p>
            } @else {
              <table mat-table [dataSource]="member()!.paymentHistory" class="data-table">
                <ng-container matColumnDef="date">
                  <th mat-header-cell *matHeaderCellDef>Date</th>
                  <td mat-cell *matCellDef="let row">{{ row.paymentDate }}</td>
                </ng-container>
                <ng-container matColumnDef="amount">
                  <th mat-header-cell *matHeaderCellDef>Amount</th>
                  <td mat-cell *matCellDef="let row">{{ row.amount | number: '1.2-2' }}</td>
                </ng-container>
                <ng-container matColumnDef="method">
                  <th mat-header-cell *matHeaderCellDef>Method</th>
                  <td mat-cell *matCellDef="let row">{{ row.paymentMethod || '—' }}</td>
                </ng-container>
                <ng-container matColumnDef="status">
                  <th mat-header-cell *matHeaderCellDef>Status</th>
                  <td mat-cell *matCellDef="let row">{{ row.status }}</td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="paymentColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: paymentColumns"></tr>
              </table>
            }
          </div>
        </mat-tab>
        <mat-tab label="Progress Photos">
          <div class="tab-content">
            <app-member-files-gallery
              [memberId]="member()!.id"
              [category]="fileCategories.MemberProgressPhoto"
              uploadLabel="Add progress photo"
              accept="image/jpeg,image/png,image/webp,image/gif" />
          </div>
        </mat-tab>
        <mat-tab label="Progress">
          <div class="tab-content">
            @if (member()!.progress.length === 0) {
              <p class="empty">No progress records yet.</p>
            } @else {
              <table mat-table [dataSource]="member()!.progress" class="data-table">
                <ng-container matColumnDef="type">
                  <th mat-header-cell *matHeaderCellDef>Type</th>
                  <td mat-cell *matCellDef="let row">{{ row.progressType }}</td>
                </ng-container>
                <ng-container matColumnDef="date">
                  <th mat-header-cell *matHeaderCellDef>Date</th>
                  <td mat-cell *matCellDef="let row">{{ row.recordedDate }}</td>
                </ng-container>
                <ng-container matColumnDef="detail">
                  <th mat-header-cell *matHeaderCellDef>Detail</th>
                  <td mat-cell *matCellDef="let row">
                    @if (row.weightKg) {
                      {{ row.weightKg }} kg
                    } @else {
                      {{ row.detail || '—' }}
                    }
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="progressColumns"></tr>
                <tr mat-row *matRowDef="let row; columns: progressColumns"></tr>
              </table>
            }
          </div>
        </mat-tab>
      </mat-tab-group>
    }
  `,
  styles: [
    `
      .profile-card {
        margin-bottom: 1rem;
      }
      .profile-top {
        margin-bottom: 1rem;
      }
      .profile-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
        gap: 0.75rem;
      }
      .tab-content {
        padding: 1rem 0;
      }
      .data-table {
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
export class MemberDetailComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  readonly fileCategories = FileCategories;
  private readonly route = inject(ActivatedRoute);
  private readonly memberService = inject(MemberService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  member = signal<MemberDetails | null>(null);
  loading = signal(true);

  paymentColumns = ['date', 'amount', 'method', 'status'];
  progressColumns = ['type', 'date', 'detail'];

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.load(id);
  }

  load(id: number): void {
    this.loading.set(true);
    this.memberService.getDetails(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.member.set(res.data);
      },
      error: () => {
        this.loading.set(false);
        this.notify.error('Failed to load member details');
      },
    });
  }

  openEdit(): void {
    const m = this.member();
    if (!m) return;
    const ref = this.dialog.open(MemberFormDialogComponent, { width: '520px', data: m });
    ref.afterClosed().subscribe((ok) => ok && this.load(m.id));
  }

  openAssignTrainer(): void {
    const m = this.member();
    if (!m) return;
    const ref = this.dialog.open(AssignTrainerDialogComponent, { width: '420px', data: m });
    ref.afterClosed().subscribe((ok) => ok && this.load(m.id));
  }
}
