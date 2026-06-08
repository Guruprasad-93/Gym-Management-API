import { DatePipe } from '@angular/common';
import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { MemberProgressEntry } from '../../../shared/models/member-self-service.models';

Chart.register(...registerables);

@Component({
  selector: 'app-member-progress',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, SaasChartCardComponent],
  templateUrl: './member-progress.component.html',
  styleUrl: './member-progress.component.css',
})
export class MemberProgressComponent implements OnInit, AfterViewInit {
  @ViewChild('weightChart') weightChartRef?: ElementRef<HTMLCanvasElement>;

  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private chart?: Chart;

  entries = signal<MemberProgressEntry[]>([]);

  form = this.fb.group({
    progressDate: [new Date().toISOString().slice(0, 10), Validators.required],
    weight: [null as number | null],
    waist: [null as number | null],
    bmi: [null as number | null],
  });

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.renderChart(), 0);
  }

  load(): void {
    this.service.getProgressTrends().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.entries.set(res.data.entries);
          setTimeout(() => this.renderChart(), 0);
        }
      },
      error: () => this.notify.error('Failed to load progress'),
    });
  }

  save(): void {
    const v = this.form.getRawValue();
    this.service.createProgress({
      progressDate: v.progressDate!,
      weight: v.weight ?? undefined,
      waist: v.waist ?? undefined,
      bmi: v.bmi ?? undefined,
    }).subscribe({
      next: () => {
        this.notify.success('Progress saved');
        this.load();
      },
      error: () => this.notify.error('Failed to save progress'),
    });
  }

  exportPdf(): void {
    this.service.exportProgressPdf().subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'progress-report.pdf';
        a.click();
        URL.revokeObjectURL(url);
      },
    });
  }

  private renderChart(): void {
    const el = this.weightChartRef?.nativeElement;
    const data = this.entries().filter((e) => e.weight != null);
    if (!el || !data.length) return;
    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'line',
      data: {
        labels: data.map((e) => e.progressDate),
        datasets: [{
          label: 'Weight (kg)',
          data: data.map((e) => e.weight!),
          borderColor: '#ff6600',
          backgroundColor: 'rgba(255, 102, 0, 0.12)',
          fill: true,
          tension: 0.35,
        }],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } },
    });
  }
}
