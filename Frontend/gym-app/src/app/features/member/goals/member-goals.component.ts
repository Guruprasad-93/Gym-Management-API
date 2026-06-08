import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { GoalTypes, MemberGoal } from '../../../shared/models/member-self-service.models';

@Component({
  selector: 'app-member-goals',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './member-goals.component.html',
  styleUrl: './member-goals.component.css',
})
export class MemberGoalsComponent implements OnInit {
  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  goalTypes = Object.values(GoalTypes);
  goals = signal<MemberGoal[]>([]);
  saving = signal(false);

  form = this.fb.group({
    goalType: ['WeightLoss', Validators.required],
    targetValue: [0, [Validators.required, Validators.min(0.1)]],
    targetDate: ['', Validators.required],
  });

  ngOnInit(): void {
    this.load();
  }

  formatGoalType(value: string): string {
    return value.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  load(): void {
    this.service.getGoals().subscribe({
      next: (res) => {
        if (res.success && res.data) this.goals.set(res.data);
      },
      error: () => this.notify.error('Failed to load goals'),
    });
  }

  create(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const v = this.form.getRawValue();
    this.service.createGoal({ goalType: v.goalType!, targetValue: v.targetValue!, targetDate: v.targetDate!, currentValue: 0 }).subscribe({
      next: () => {
        this.saving.set(false);
        this.notify.success('Goal created');
        this.form.reset({ goalType: 'WeightLoss', targetValue: 0, targetDate: '' });
        this.load();
      },
      error: () => {
        this.saving.set(false);
        this.notify.error('Failed to create goal');
      },
    });
  }

  complete(goalId: number): void {
    this.service.completeGoal(goalId).subscribe({
      next: () => {
        this.notify.success('Goal completed!');
        this.load();
      },
      error: () => this.notify.error('Failed to complete goal'),
    });
  }
}
