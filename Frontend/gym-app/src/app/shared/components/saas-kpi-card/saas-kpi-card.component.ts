import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-saas-kpi-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CommonModule, MatIconModule],
  templateUrl: './saas-kpi-card.component.html',
  styleUrl: './saas-kpi-card.component.css',
})
export class SaasKpiCardComponent {
  @Input() label = '';
  @Input() value: number | string = 0;
  @Input() icon = 'insights';
  @Input() accent = '#ff6600';
  @Input() isCurrency = false;
  @Input() hint = '';

  get sparkPath(): string {
    const n = this.numericValue;
    const points = [
      [0, 24 - (n % 7) * 2],
      [20, 18 - (n % 5) * 2],
      [40, 22 - (n % 6) * 2],
      [60, 12 - (n % 4) * 2],
      [80, 16 - (n % 8) * 1.5],
      [120, 8 + (n % 3) * 2],
    ];
    return points.map((p, i) => `${i === 0 ? 'M' : 'L'}${p[0]},${p[1]}`).join(' ');
  }

  get sparkAreaPath(): string {
    return `${this.sparkPath} L120,32 L0,32 Z`;
  }

  private get numericValue(): number {
    const raw = typeof this.value === 'number' ? this.value : Number(String(this.value).replace(/[^\d.-]/g, ''));
    return Number.isFinite(raw) ? Math.abs(raw) : 0;
  }
}
