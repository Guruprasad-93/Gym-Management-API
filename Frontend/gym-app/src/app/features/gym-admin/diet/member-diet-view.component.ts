import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DietService } from '../../../core/services/diet.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AuthService } from '../../../core/services/auth.service';
import { Permissions } from '../../../core/constants/permissions';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { MemberDietPlanView } from '../../../shared/models/diet.models';
import { AssignDietPlanDialogComponent } from './assign-diet-plan-dialog.component';
import { MemberFilesGalleryComponent } from '../../../shared/components/member-files-gallery/member-files-gallery.component';
import { FileCategories } from '../../../shared/models/file.models';

@Component({
  selector: 'app-member-diet-view',
  standalone: true,
  imports: [
    DatePipe, RouterModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule,
    PageHeaderComponent, MemberFilesGalleryComponent,
  ],
  template: `
    <app-page-header [title]="diet?.planName || 'Member Diet Plan'" [subtitle]="diet?.memberName || ''">
      @if (memberId && auth.hasPermission(permissions.AssignDietPlan)) {
        <button mat-stroked-button type="button" (click)="openAssign()"><mat-icon>swap_horiz</mat-icon> Change plan</button>
      }
      @if (diet?.dietPlanId && auth.hasPermission(permissions.ExportDietPlans)) {
        <button mat-stroked-button type="button" (click)="exportPdf()"><mat-icon>picture_as_pdf</mat-icon> PDF</button>
      }
    </app-page-header>

    @if (loading()) {
      <mat-spinner />
    } @else if (!diet?.dietPlanId) {
      <p class="empty">No active diet plan assigned.</p>
      @if (memberId && auth.hasPermission(permissions.AssignDietPlan)) {
        <button mat-flat-button color="primary" type="button" (click)="openAssign()">Assign diet plan</button>
      }
    } @else {
      <div class="meta">
        @if (diet?.categoryName) { <span>Category: {{ diet?.categoryName }}</span> }
        @if (diet?.targetCalories) { <span>Target: {{ diet?.targetCalories }} kcal</span> }
        @if (diet?.startDate) { <span>From: {{ diet?.startDate | date }}</span> }
        @if (diet?.endDate) { <span>To: {{ diet?.endDate | date }}</span> }
      </div>
      @if (diet?.planDescription) {
        <p class="desc">{{ diet?.planDescription }}</p>
      }
      <table class="meal-table">
        <thead><tr><th>Meal</th><th>Food</th><th>Qty</th><th>Cal</th><th>Notes</th></tr></thead>
        <tbody>
          @for (item of diet?.items ?? []; track $index) {
            <tr>
              <td>{{ item.mealTime }}</td>
              <td>{{ item.foodName }}</td>
              <td>{{ item.quantity || '—' }}</td>
              <td>{{ item.calories ?? '—' }}</td>
              <td>{{ item.notes || '—' }}</td>
            </tr>
          }
        </tbody>
      </table>
      @if (memberId) {
        <h3>Attachments</h3>
        <app-member-files-gallery
          [memberId]="memberId"
          [category]="fileCategories.DietAttachment"
          [assignedDietPlanId]="diet?.assignedDietPlanId ?? undefined"
          uploadLabel="Upload attachment"
          accept="image/*,.pdf,.doc,.docx,.txt" />
      }
    }
  `,
  styles: [
    `
      .meta { display: flex; flex-wrap: wrap; gap: 1rem; margin-bottom: 1rem; color: #555; }
      .desc { margin-bottom: 1rem; }
      .meal-table { width: 100%; border-collapse: collapse; background: #fff; border-radius: 8px; }
      .meal-table th, .meal-table td { padding: 0.75rem; border-bottom: 1px solid #eee; text-align: left; }
      .empty { color: #666; }
    `,
  ],
})
export class MemberDietViewComponent implements OnInit {
  readonly auth = inject(AuthService);
  readonly permissions = Permissions;
  readonly fileCategories = FileCategories;
  private readonly route = inject(ActivatedRoute);
  private readonly svc = inject(DietService);
  private readonly notify = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  loading = signal(true);
  diet: MemberDietPlanView | null = null;
  memberId: number | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.svc.getMyDiet().subscribe({
        next: (res) => this.finish(res.data),
        error: () => this.fail(),
      });
      return;
    }
    this.memberId = Number(id);
    this.svc.getMemberDiet(this.memberId).subscribe({
      next: (res) => this.finish(res.data),
      error: () => this.fail(),
    });
  }

  openAssign(): void {
    this.dialog
      .open(AssignDietPlanDialogComponent, {
        width: '480px',
        data: { memberId: this.memberId! },
      })
      .afterClosed()
      .subscribe((ok) => ok && this.reload());
  }

  exportPdf(): void {
    if (!this.diet?.dietPlanId) return;
    this.svc.downloadPdf(this.diet.dietPlanId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `diet-plan-${this.diet!.dietPlanId}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.notify.error('Export failed'),
    });
  }

  private reload(): void {
    if (!this.memberId) return;
    this.loading.set(true);
    this.svc.getMemberDiet(this.memberId).subscribe({
      next: (res) => this.finish(res.data),
      error: () => this.fail(),
    });
  }

  private finish(data?: MemberDietPlanView): void {
    this.loading.set(false);
    this.diet = data ?? null;
    if (!this.memberId && data?.memberId) this.memberId = data.memberId;
  }

  private fail(): void {
    this.loading.set(false);
    this.notify.error('Failed to load diet plan');
  }
}
