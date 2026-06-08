import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MemberSelfServiceService } from '../../../core/services/member-self-service.service';
import { NotificationService } from '../../../core/services/notification.service';
import { FeedbackTypes, MemberFeedback } from '../../../shared/models/member-self-service.models';

@Component({
  selector: 'app-member-feedback',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './member-feedback.component.html',
  styleUrl: './member-feedback.component.css',
})
export class MemberFeedbackComponent implements OnInit {
  private readonly service = inject(MemberSelfServiceService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  items = signal<MemberFeedback[]>([]);

  form = this.fb.group({
    feedbackType: [FeedbackTypes.Gym, Validators.required],
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comments: [''],
    trainerId: [null as number | null],
  });

  ngOnInit(): void {
    this.load();
  }

  starRating(rating: number): string {
    return '★'.repeat(rating) + '☆'.repeat(Math.max(0, 5 - rating));
  }

  load(): void {
    this.service.getFeedback().subscribe({
      next: (r) => {
        if (r.success && r.data) this.items.set(r.data);
      },
    });
  }

  submit(): void {
    const v = this.form.getRawValue();
    this.service.submitFeedback({
      feedbackType: v.feedbackType!,
      rating: v.rating!,
      comments: v.comments || undefined,
      trainerId: v.trainerId ?? undefined,
    }).subscribe({
      next: () => {
        this.notify.success('Thank you for your feedback');
        this.form.patchValue({ comments: '' });
        this.load();
      },
      error: () => this.notify.error('Failed to submit feedback'),
    });
  }
}
