import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface TemporaryPasswordDialogData {
  title: string;
  email: string;
  temporaryPassword: string;
  message?: string;
}

@Component({
  selector: 'app-temporary-password-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      @if (data.message) {
        <p>{{ data.message }}</p>
      }
      <p><strong>Email:</strong> {{ data.email }}</p>
      <div class="password-box">
        <code>{{ data.temporaryPassword }}</code>
        <button mat-icon-button type="button" (click)="copy()" aria-label="Copy password">
          <mat-icon>content_copy</mat-icon>
        </button>
      </div>
      <p class="hint">Share this password securely. The user must change it on first login.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-flat-button color="primary" mat-dialog-close>Done</button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .password-box {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        background: #f5f5f5;
        padding: 0.75rem 1rem;
        border-radius: 8px;
        margin: 1rem 0;
      }
      code {
        flex: 1;
        word-break: break-all;
        font-size: 1rem;
      }
      .hint {
        font-size: 0.85rem;
        color: #666;
      }
    `,
  ],
})
export class TemporaryPasswordDialogComponent {
  readonly data = inject<TemporaryPasswordDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<TemporaryPasswordDialogComponent>);

  copy(): void {
    navigator.clipboard.writeText(this.data.temporaryPassword);
  }
}
