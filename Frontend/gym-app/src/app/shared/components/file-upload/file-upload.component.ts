import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FileService } from '../../../core/services/file.service';
import { NotificationService } from '../../../core/services/notification.service';
import { StoredFile, UploadFileRequest } from '../../models/file.models';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <input
      #fileInput
      type="file"
      class="hidden-input"
      [accept]="accept"
      (change)="onFileSelected($event)" />
    <button
      mat-stroked-button
      type="button"
      [disabled]="disabled || uploading()"
      (click)="fileInput.click()">
      @if (uploading()) {
        <mat-spinner diameter="20" />
      } @else {
        <mat-icon>upload</mat-icon>
      }
      {{ label }}
    </button>
  `,
  styles: [
    `
      .hidden-input {
        display: none;
      }
    `,
  ],
})
export class FileUploadComponent {
  private readonly fileService = inject(FileService);
  private readonly notify = inject(NotificationService);

  @Input() label = 'Upload file';
  @Input() accept = 'image/*';
  @Input() disabled = false;
  @Input({ required: true }) request!: UploadFileRequest;

  @Output() readonly uploaded = new EventEmitter<StoredFile>();

  uploading = signal(false);

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file || !this.request) return;

    this.uploading.set(true);
    this.fileService.upload(file, this.request).subscribe({
      next: (res) => {
        this.uploading.set(false);
        if (res.success && res.data) {
          this.notify.success('File uploaded.');
          this.uploaded.emit(res.data);
        } else {
          this.notify.error(res.message ?? 'Upload failed.');
        }
      },
      error: (err) => {
        this.uploading.set(false);
        this.notify.error(err.error?.message ?? 'Upload failed.');
      },
    });
  }
}
