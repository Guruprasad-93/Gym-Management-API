import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-saas-chart-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './saas-chart-card.component.html',
  styleUrl: './saas-chart-card.component.css',
})
export class SaasChartCardComponent {
  @Input() title = '';
  @Input() subtitle = '';
}
