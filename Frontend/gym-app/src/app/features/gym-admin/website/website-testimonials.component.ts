import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { WebsiteService } from '../../../core/services/website.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GymWebsiteTestimonial } from '../../../shared/models/website.models';

@Component({
  selector: 'app-website-testimonials',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule, MatTableModule, PageHeaderComponent],
  template: `
    <app-page-header title="Testimonials" subtitle="Member reviews for your website">
      <a mat-stroked-button routerLink="/gym-admin/website-builder">Back</a>
    </app-page-header>
    <form [formGroup]="form" (ngSubmit)="create()" class="form-row">
      <mat-form-field><mat-label>Member Name</mat-label><input matInput formControlName="memberName" /></mat-form-field>
      <mat-form-field><mat-label>Rating</mat-label><input matInput type="number" formControlName="rating" /></mat-form-field>
      <mat-form-field class="wide"><mat-label>Review</mat-label><textarea matInput formControlName="reviewText"></textarea></mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">Add</button>
    </form>
    <table mat-table [dataSource]="items()" class="full-width">
      <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let t">{{ t.memberName }}</td></ng-container>
      <ng-container matColumnDef="rating"><th mat-header-cell *matHeaderCellDef>Rating</th><td mat-cell *matCellDef="let t">{{ t.rating }}</td></ng-container>
      <ng-container matColumnDef="approved"><th mat-header-cell *matHeaderCellDef>Approved</th><td mat-cell *matCellDef="let t">{{ t.isApproved ? 'Yes' : 'No' }}</td></ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols"></tr>
    </table>
  `,
  styles: `.form-row { display:flex; flex-wrap:wrap; gap:1rem; margin-bottom:1rem; } .wide { min-width:280px; } .full-width { width:100%; }`,
})
export class WebsiteTestimonialsComponent implements OnInit {
  private readonly service = inject(WebsiteService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  items = signal<GymWebsiteTestimonial[]>([]);
  cols = ['name', 'rating', 'approved'];
  form = this.fb.group({ memberName: ['', Validators.required], rating: [5, Validators.required], reviewText: [''], isApproved: [true] });

  ngOnInit(): void { this.load(); }
  load(): void {
    this.service.getTestimonials().subscribe({
      next: (r) => { if (r.success && r.data) this.items.set(r.data); },
      error: () => this.notify.error('Failed to load testimonials'),
    });
  }
  create(): void {
    if (this.form.invalid) return;
    this.service.createTestimonial(this.form.getRawValue() as never).subscribe({
      next: () => { this.notify.success('Testimonial added'); this.form.reset({ rating: 5, isApproved: true }); this.load(); },
      error: () => this.notify.error('Failed to add testimonial'),
    });
  }
}
