import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../../core/services/auth.service';
import { FileService } from '../../../core/services/file.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Permissions } from '../../../core/constants/permissions';
import { Roles } from '../../../core/constants/roles';
import { FileCategories, FileCategory, StoredFile } from '../../models/file.models';
import { FilePreviewComponent } from '../file-preview/file-preview.component';
import { FileUploadComponent } from '../file-upload/file-upload.component';

@Component({
  selector: 'app-profile-photo-manager',
  standalone: true,
  imports: [MatCardModule, FilePreviewComponent, FileUploadComponent],
  template: `
    <div class="photo-manager">
      <app-file-preview
        [publicUrl]="photoUrl()"
        contentType="image/jpeg"
        [alt]="title"
        [width]="size"
        [height]="size"
        [circle]="circle" />
      @if (canUpload()) {
        <app-file-upload
          [label]="uploadLabel"
          accept="image/jpeg,image/png,image/webp,image/gif"
          [request]="uploadRequest"
          (uploaded)="onUploaded($event)" />
      }
    </div>
  `,
  styles: [
    `
      .photo-manager {
        display: flex;
        flex-direction: column;
        align-items: flex-start;
        gap: 0.75rem;
      }
    `,
  ],
})
export class ProfilePhotoManagerComponent implements OnInit {
  private readonly fileService = inject(FileService);
  private readonly auth = inject(AuthService);
  private readonly notify = inject(NotificationService);

  @Input({ required: true }) title = 'Profile photo';
  @Input({ required: true }) category!: FileCategory;
  @Input() memberId?: number;
  @Input() trainerId?: number;
  @Input() gymId?: string;
  @Input() size = 120;
  @Input() circle = true;
  @Input() uploadLabel = 'Change photo';

  @Output() readonly photoChanged = new EventEmitter<StoredFile>();

  photoUrl = signal<string | null>(null);

  get uploadRequest() {
    return {
      fileCategory: this.category,
      gymId: this.gymId ?? null,
      memberId: this.memberId ?? null,
      trainerId: this.trainerId ?? null,
    };
  }

  ngOnInit(): void {
    this.loadExisting();
  }

  canUpload(): boolean {
    if (this.auth.hasPermission(Permissions.UploadFiles) || this.auth.hasPermission(Permissions.ManageFiles)) {
      return true;
    }
    if (
      this.category === FileCategories.MemberProfilePhoto ||
      this.category === FileCategories.MemberProgressPhoto
    ) {
      return this.auth.hasRole(Roles.Member);
    }
    if (this.category === FileCategories.TrainerProfilePhoto) {
      return this.auth.hasRole(Roles.Trainer);
    }
    if (this.category === FileCategories.GymLogo) {
      return this.auth.hasRole(Roles.GymAdmin) || this.auth.hasRole(Roles.SuperAdmin);
    }
    return false;
  }

  loadExisting(): void {
    if (this.memberId && this.category === FileCategories.MemberProfilePhoto) {
      this.fileService.getMemberFiles(this.memberId, FileCategories.MemberProfilePhoto).subscribe({
        next: (res) => {
          const latest = res.data?.[0];
          if (latest) this.photoUrl.set(latest.publicUrl);
        },
      });
      return;
    }
    if (this.trainerId && this.category === FileCategories.TrainerProfilePhoto) {
      this.fileService.getTrainerFiles(this.trainerId, FileCategories.TrainerProfilePhoto).subscribe({
        next: (res) => {
          const latest = res.data?.[0];
          if (latest) this.photoUrl.set(latest.publicUrl);
        },
      });
      return;
    }
    if (this.category === FileCategories.GymLogo) {
      this.fileService.getGymLogo(this.gymId ?? null).subscribe({
        next: (res) => {
          if (res.data?.publicUrl) this.photoUrl.set(res.data.publicUrl);
        },
      });
    }
  }

  onUploaded(file: StoredFile): void {
    this.photoUrl.set(file.publicUrl);
    this.photoChanged.emit(file);
    this.notify.success('Photo updated.');
  }
}
