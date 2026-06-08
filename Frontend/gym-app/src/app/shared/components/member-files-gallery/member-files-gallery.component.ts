import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { FileService } from '../../../core/services/file.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { Roles } from '../../../core/constants/roles';
import { FileCategories, FileCategory, MemberFile } from '../../models/file.models';
import { FilePreviewComponent } from '../file-preview/file-preview.component';
import { FileUploadComponent } from '../file-upload/file-upload.component';

@Component({
  selector: 'app-member-files-gallery',
  standalone: true,
  imports: [
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    FilePreviewComponent,
    FileUploadComponent,
  ],
  template: `
    <div class="gallery-header">
      @if (canUpload()) {
        <app-file-upload
          [label]="uploadLabel"
          [accept]="accept"
          [request]="uploadRequest"
          (uploaded)="onUploaded()" />
      }
      @if (category === progressCategory) {
        <mat-form-field appearance="outline" class="notes-field">
          <mat-label>Notes (optional)</mat-label>
          <input matInput [(ngModel)]="notes" />
        </mat-form-field>
      }
    </div>

    @if (files().length === 0) {
      <p class="empty">No files yet.</p>
    } @else {
      <div class="gallery-grid">
        @for (file of files(); track file.memberFileId) {
          <div class="gallery-item">
            <app-file-preview
              [publicUrl]="file.publicUrl"
              [contentType]="file.contentType"
              [alt]="file.originalFileName"
              [width]="140"
              [height]="140" />
            <div class="meta">
              <span class="name">{{ file.originalFileName }}</span>
              @if (file.takenAt) {
                <span class="date">{{ file.takenAt }}</span>
              }
              @if (file.notes) {
                <span class="notes">{{ file.notes }}</span>
              }
            </div>
            @if (canDelete()) {
              <button mat-icon-button type="button" color="warn" (click)="remove(file)">
                <mat-icon>delete</mat-icon>
              </button>
            }
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      .gallery-header {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 1rem;
        margin-bottom: 1rem;
      }
      .notes-field {
        min-width: 220px;
      }
      .gallery-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
        gap: 1rem;
      }
      .gallery-item {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
      }
      .meta {
        font-size: 0.8rem;
        color: #555;
      }
      .meta .name {
        display: block;
        font-weight: 500;
      }
      .empty {
        color: #666;
      }
    `,
  ],
})
export class MemberFilesGalleryComponent implements OnInit {
  private readonly fileService = inject(FileService);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);

  @Input({ required: true }) memberId!: number;
  @Input({ required: true }) category!: FileCategory;
  @Input() uploadLabel = 'Upload';
  @Input() accept = 'image/jpeg,image/png,image/webp,image/gif';
  @Input() assignedDietPlanId?: number;
  @Input() assignedWorkoutPlanId?: number;

  readonly progressCategory = FileCategories.MemberProgressPhoto;
  files = signal<MemberFile[]>([]);
  notes = '';

  ngOnInit(): void {
    this.load();
  }

  get uploadRequest() {
    return {
      fileCategory: this.category,
      memberId: this.memberId,
      notes: this.notes || null,
      takenAt: new Date().toISOString().slice(0, 10),
      assignedDietPlanId: this.assignedDietPlanId ?? null,
      assignedWorkoutPlanId: this.assignedWorkoutPlanId ?? null,
    };
  }

  canUpload(): boolean {
    if (this.auth.hasPermission(Permissions.UploadFiles) || this.auth.hasPermission(Permissions.ManageFiles)) {
      return true;
    }
    if (
      this.category === FileCategories.MemberProgressPhoto ||
      this.category === FileCategories.MemberProfilePhoto
    ) {
      return this.auth.hasRole(Roles.Member);
    }
    return false;
  }

  canDelete(): boolean {
    return (
      this.auth.hasPermission(Permissions.DeleteFiles) || this.auth.hasPermission(Permissions.ManageFiles)
    );
  }

  load(): void {
    this.fileService.getMemberFiles(this.memberId, this.category).subscribe({
      next: (res) => {
        if (res.success && res.data) this.files.set(res.data);
      },
      error: () => this.notify.error('Failed to load files'),
    });
  }

  onUploaded(): void {
    this.notes = '';
    this.load();
  }

  remove(file: MemberFile): void {
    if (!confirm('Delete this file?')) return;
    this.fileService.delete(file.fileId).subscribe({
      next: (res) => {
        if (res.success) {
          this.notify.success('File deleted.');
          this.load();
        }
      },
      error: () => this.notify.error('Delete failed'),
    });
  }
}
