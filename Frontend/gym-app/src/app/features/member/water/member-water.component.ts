import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { SaasKpiCardComponent } from '../../../shared/components/saas-kpi-card/saas-kpi-card.component';
import { WaterIntake } from '../../../shared/models/member-self-service.models';

Chart.register(...registerables);

@Component({
  selector: 'app-member-water',
  standalone: true,
  imports: [ReactiveFormsModule, SaasChartCardComponent, SaasKpiCardComponent],
  templateUrl: './member-water.component.html',
  styleUrl: './member-water.component.css',
})
export class MemberWaterComponent implements OnInit, AfterViewInit {
  @ViewChild('chartRef') chartRef?: ElementRef<HTMLCanvasElement>;

  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private chart?: Chart;

  today = signal<WaterIntake | null>(null);
  history = signal<WaterIntake[]>([]);

  form = this.fb.group({
    targetLitres: [2.5, Validators.required],
    consumedLitres: [0, Validators.required],
  });

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.renderChart(), 0);
  }

  load(): void {
    this.service.getTodayWater().subscribe({
      next: (r) => {
        if (r.success && r.data) {
          this.today.set(r.data);
          this.form.patchValue({ targetLitres: r.data.targetLitres, consumedLitres: r.data.consumedLitres });
        }
      },
    });
    this.service.getProgressTrends().subscribe({
      next: (r) => {
        if (r.success && r.data?.waterHistory) {
          this.history.set(r.data.waterHistory);
          setTimeout(() => this.renderChart(), 0);
        }
      },
    });
  }

  save(): void {
    const v = this.form.getRawValue();
    this.service.upsertWater({ targetLitres: v.targetLitres!, consumedLitres: v.consumedLitres! }).subscribe({
      next: (r) => {
        if (r.success) {
          this.today.set(r.data ?? null);
          this.notify.success('Water intake saved');
          this.load();
        }
      },
      error: () => this.notify.error('Failed to save'),
    });
  }

  private renderChart(): void {
    const el = this.chartRef?.nativeElement;
    const data = this.history();
    if (!el || !data.length) return;
    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'bar',
      data: {
        labels: data.map((d) => d.logDate),
        datasets: [
          { label: 'Consumed (L)', data: data.map((d) => d.consumedLitres), backgroundColor: '#06aed4', borderRadius: 6 },
          { label: 'Target (L)', data: data.map((d) => d.targetLitres), backgroundColor: '#e0f2fe', borderRadius: 6 },
        ],
      },
      options: { responsive: true, maintainAspectRatio: false },
    });
  }
}
