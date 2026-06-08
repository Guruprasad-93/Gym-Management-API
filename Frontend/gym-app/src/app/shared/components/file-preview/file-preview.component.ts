import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FileService } from '../../../core/services/file.service';

@Component({
  selector: 'app-file-preview',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    @if (src) {
      <div
        class="preview-wrap"
        [class.circle]="circle"
        [style.width.px]="width"
        [style.height.px]="height">
        @if (isImage()) {
          <img [src]="src" [alt]="alt" class="preview-img" />
        } @else {
          <a [href]="src" target="_blank" rel="noopener" class="doc-link">
            <mat-icon>description</mat-icon>
            <span>{{ alt }}</span>
          </a>
        }
        @if (showRemove && removable) {
          <button mat-icon-button type="button" class="remove-btn" (click)="removed.emit()" aria-label="Remove">
            <mat-icon>close</mat-icon>
          </button>
        }
      </div>
    } @else if (showPlaceholder) {
      <div
        class="preview-wrap placeholder"
        [class.circle]="circle"
        [style.width.px]="width"
        [style.height.px]="height">
        <mat-icon>{{ placeholderIcon }}</mat-icon>
      </div>
    }
  `,
  styles: [
    `
      .preview-wrap {
        position: relative;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        overflow: hidden;
        background: #fafafa;
      }
      .preview-wrap.circle {
        border-radius: 50%;
      }
      .preview-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }
      .placeholder {
        color: #9e9e9e;
      }
      .placeholder mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
      }
      .doc-link {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        text-decoration: none;
        color: inherit;
      }
      .remove-btn {
        position: absolute;
        top: 2px;
        right: 2px;
        background: rgba(255, 255, 255, 0.9);
      }
    `,
  ],
})
export class FilePreviewComponent {
  private readonly fileService = inject(FileService);

  @Input() publicUrl: string | null | undefined;
  @Input() contentType = 'image/jpeg';
  @Input() alt = 'File preview';
  @Input() width = 120;
  @Input() height = 120;
  @Input() circle = false;
  @Input() showPlaceholder = true;
  @Input() placeholderIcon = 'image';
  @Input() showRemove = false;
  @Input() removable = false;

  @Output() readonly removed = new EventEmitter<void>();

  get src(): string | null {
    const url = this.publicUrl;
    if (!url) return null;
    return this.fileService.contentUrl(url);
  }

  isImage(): boolean {
    return this.contentType.startsWith('image/');
  }
}
