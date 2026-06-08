import { DatePipe } from '@angular/common';
import { Component, OnInit, AfterViewInit, ElementRef, ViewChild, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Chart, registerables } from 'chart.js';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SaasChartCardComponent } from '../../../shared/components/saas-chart-card/saas-chart-card.component';
import { WorkoutTracking } from '../../../shared/models/member-self-service.models';

Chart.register(...registerables);

@Component({
  selector: 'app-member-workouts',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, MatIconModule, SaasChartCardComponent],
  templateUrl: './member-workouts.component.html',
  styleUrl: './member-workouts.component.css',
})
export class MemberWorkoutsComponent implements OnInit, AfterViewInit {
  @ViewChild('chartRef') chartRef?: ElementRef<HTMLCanvasElement>;

  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  private chart?: Chart;

  workouts = signal<WorkoutTracking[]>([]);
  streak = signal(0);

  form = this.fb.group({
    workoutPlanId: [1, Validators.required],
    completionPercentage: [100, [Validators.required, Validators.min(0), Validators.max(100)]],
    exerciseCompleted: [''],
  });

  ngOnInit(): void {
    this.load();
    this.service.getWorkoutStreak().subscribe({
      next: (r) => {
        if (r.success) this.streak.set(r.data ?? 0);
      },
    });
  }

  ngAfterViewInit(): void {
    setTimeout(() => this.renderChart(), 0);
  }

  load(): void {
    this.service.getWorkouts().subscribe({
      next: (r) => {
        if (r.success && r.data) {
          this.workouts.set(r.data);
          setTimeout(() => this.renderChart(), 0);
        }
      },
      error: () => this.notify.error('Failed to load workouts'),
    });
  }

  save(): void {
    const v = this.form.getRawValue();
    this.service.upsertWorkout({
      workoutPlanId: v.workoutPlanId!,
      completionPercentage: v.completionPercentage!,
      exerciseCompleted: v.exerciseCompleted || undefined,
    }).subscribe({
      next: () => {
        this.notify.success('Workout logged');
        this.load();
      },
      error: () => this.notify.error('Failed to save workout'),
    });
  }

  private renderChart(): void {
    const el = this.chartRef?.nativeElement;
    const data = this.workouts();
    if (!el || !data.length) return;
    this.chart?.destroy();
    this.chart = new Chart(el, {
      type: 'bar',
      data: {
        labels: data.map((d) => d.workoutDate),
        datasets: [{
          label: 'Completion %',
          data: data.map((d) => d.completionPercentage),
          backgroundColor: '#ff6600',
          borderRadius: 6,
        }],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: { y: { max: 100, grid: { color: '#f2f4f7' } }, x: { grid: { display: false } } },
      },
    });
  }
}
