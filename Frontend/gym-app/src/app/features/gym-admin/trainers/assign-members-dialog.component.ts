import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule, MatSelectionListChange } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TrainerService } from '../../../core/services/trainer.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Member } from '../../../shared/models/member.models';
import { Trainer } from '../../../shared/models/trainer.models';

@Component({
  selector: 'app-assign-members-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatListModule,
    MatIconModule,
  ],
  template: `
    <h2 mat-dialog-title>Assign Members</h2>
    <p class="subtitle">Trainer: {{ trainer.fullName || trainer.specialization || trainer.id }}</p>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Search unassigned members</mat-label>
        <input matInput [formControl]="searchControl" />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>

      @if (loading()) {
        <mat-spinner class="center-spinner" />
      } @else if (members().length === 0) {
        <p class="empty">No unassigned members found.</p>
      } @else {
        <mat-selection-list (selectionChange)="onSelectionChange($event)">
          @for (member of members(); track member.id) {
            <mat-list-option [value]="member.id" [selected]="selectedIds.has(member.id)">
              {{ member.fullName }} — {{ member.email }}
            </mat-list-option>
          }
        </mat-selection-list>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button
        mat-flat-button
        color="primary"
        type="button"
        [disabled]="saving || selectedIds.size === 0"
        (click)="assign()">
        @if (saving) {
          <mat-spinner diameter="22" />
        } @else {
          Assign ({{ selectedIds.size }})
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
      }
      .empty {
        color: #666;
        padding: 1rem 0;
      }
      .center-spinner {
        margin: 1rem auto;
        display: block;
      }
    `,
  ],
})
export class AssignMembersDialogComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly trainerService = inject(TrainerService);
  private readonly notify = inject(NotificationService);
  private readonly ref = inject(MatDialogRef<AssignMembersDialogComponent>);
  readonly trainer = inject<Trainer>(MAT_DIALOG_DATA);

  loading = signal(true);
  members = signal<Member[]>([]);
  selectedIds = new Set<number>();
  saving = false;

  searchControl = this.fb.nonNullable.control('');

  ngOnInit(): void {
    this.loadMembers();
    this.searchControl.valueChanges
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(() => this.loadMembers());
  }

  loadMembers(): void {
    this.loading.set(true);
    this.trainerService
      .getUnassignedMembers(this.trainer.id, this.searchControl.value || undefined)
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) this.members.set(res.data);
        },
        error: () => {
          this.loading.set(false);
          this.notify.error('Failed to load members');
        },
      });
  }

  onSelectionChange(event: MatSelectionListChange): void {
    for (const option of event.options) {
      const id = option.value as number;
      if (option.selected) this.selectedIds.add(id);
      else this.selectedIds.delete(id);
    }
  }

  assign(): void {
    if (this.selectedIds.size === 0) return;
    this.saving = true;
    this.trainerService
      .assignMembers(this.trainer.id, { memberIds: [...this.selectedIds] })
      .subscribe({
        next: (res) => {
          this.saving = false;
          if (res.success) {
            this.notify.success('Members assigned');
            this.ref.close(true);
          }
        },
        error: (err) => {
          this.saving = false;
          this.notify.error(err.error?.message ?? 'Assignment failed');
        },
      });
  }
}
