import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TrainerService } from '../../../core/services/trainer.service';
import { MemberService } from '../../../core/services/member.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';
import { Trainer } from '../../../shared/models/trainer.models';

@Component({
  selector: 'app-assign-trainer-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <h2 mat-dialog-title>Assign Trainer</h2>
    <p class="subtitle">Member: {{ member.fullName }}</p>
    <mat-dialog-content>
      @if (loading()) {
        <mat-spinner class="center-spinner" />
      } @else {
        <form [formGroup]="form">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Trainer</mat-label>
            <mat-select formControlName="trainerId">
              <mat-option [value]="null">None (Gym Admin handles)</mat-option>
              @for (trainer of trainers(); track trainer.id) {
                <mat-option [value]="trainer.id">
                  {{ trainer.fullName || trainer.specialization || trainer.id }}
                </mat-option>
              }
            </mat-select>
          </mat-form-field>
        </form>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      @if (member.trainerId) {
        <button mat-stroked-button color="warn" type="button" [disabled]="saving" (click)="remove()">
          Remove Assignment
        </button>
      }
      <button mat-flat-button color="primary" type="button" [disabled]="saving || form.invalid" (click)="assign()">
        @if (saving) {
          <mat-spinner diameter="22" />
        } @else {
          Assign
        }
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .subtitle {
        margin: 0 1.5rem;
        color: #666;
      }
      .full-width {
        width: 100%;
        min-width: 300px;
      }
      .center-spinner {
        margin: 1rem auto;
        display: block;
      }
    `,
  ],
})
export class AssignTrainerDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly trainerService = inject(TrainerService);
  private readonly memberService = inject(MemberService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<AssignTrainerDialogComponent>);
  readonly member = inject<Member>(MAT_DIALOG_DATA);

  loading = signal(true);
  trainers = signal<Trainer[]>([]);
  saving = false;

  form = this.fb.group({
    trainerId: [this.member.trainerId ?? null as number | null],
  });

  ngOnInit(): void {
    this.trainerService
      .getPaged(null, { pageNumber: 1, pageSize: 100, sortColumn: 'UserName', sortDirection: 'asc' })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) this.trainers.set(res.data.items.filter((t) => t.isActive));
        },
        error: () => {
          this.loading.set(false);
          this.notify.error('Failed to load trainers');
        },
      });
  }

  assign(): void {
    const trainerId = this.form.value.trainerId;
    if (!trainerId) {
      this.remove();
      return;
    }
    this.saving = true;
    this.memberService.assignTrainer(this.member.id, { trainerId }).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.success) {
          this.notify.success('Trainer assigned');
          this.ref.close(true);
        }
      },
      error: (err) => {
        this.saving = false;
        this.notify.error(err.error?.message ?? 'Assignment failed');
      },
    });
  }

  remove(): void {
    this.saving = true;
    this.memberService.removeTrainerAssignment(this.member.id).subscribe({
      next: (res) => {
        this.saving = false;
        if (res.success) {
          this.notify.success('Trainer assignment removed');
          this.ref.close(true);
        }
      },
      error: () => {
        this.saving = false;
        this.notify.error('Failed to remove assignment');
      },
    });
  }
}
