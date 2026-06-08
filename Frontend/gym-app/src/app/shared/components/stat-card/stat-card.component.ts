import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <mat-card class="stat-card" [style.border-left-color]="color">
      <div class="stat-body">
        <div>
          <p class="label">{{ label }}</p>
          <h2 class="value">
            @if (isCurrency) {
              {{ value | currency }}
            } @else {
              {{ value }}
            }
          </h2>
        </div>
        <mat-icon [style.color]="color">{{ icon }}</mat-icon>
      </div>
    </mat-card>
  `,
  styles: [
    `
      .stat-card {
        border-left: 4px solid;
        height: 100%;
      }
      .stat-body {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.5rem;
      }
      .label {
        margin: 0;
        color: #666;
        font-size: 0.85rem;
      }
      .value {
        margin: 0.25rem 0 0;
        font-size: 1.75rem;
      }
      mat-icon {
        font-size: 2.5rem;
        width: 2.5rem;
        height: 2.5rem;
        opacity: 0.85;
      }
    `,
  ],
})
export class StatCardComponent {
  @Input() label = '';
  @Input() value: number | string = 0;
  @Input() icon = 'insights';
  @Input() color = '#1a237e';
  @Input() isCurrency = false;
}
