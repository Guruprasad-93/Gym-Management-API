import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { WebsiteService } from '../../../core/services/website.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GymWebsiteGalleryItem } from '../../../shared/models/website.models';

@Component({
  selector: 'app-website-gallery',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule, PageHeaderComponent],
  template: `
    <app-page-header title="Website Gallery" subtitle="Images displayed on your public site">
      <a mat-stroked-button routerLink="/gym-admin/website-builder">Back</a>
    </app-page-header>
    <form [formGroup]="form" (ngSubmit)="add()" class="form-row">
      <mat-form-field><mat-label>File ID</mat-label><input matInput type="number" formControlName="fileId" /></mat-form-field>
      <mat-form-field><mat-label>Caption</mat-label><input matInput formControlName="caption" /></mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">Add Image</button>
    </form>
    <div class="gallery-grid">
      @for (item of gallery(); track item.id) {
        <div class="card">
          @if (item.publicUrl) { <img [src]="item.publicUrl" [alt]="item.caption ?? 'Gallery'" /> }
          <p>{{ item.caption ?? item.originalFileName }}</p>
          <button mat-button color="warn" (click)="remove(item.id)">Remove</button>
        </div>
      }
    </div>
  `,
  styles: `.form-row { display:flex; gap:1rem; margin-bottom:1rem; } .gallery-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(180px,1fr)); gap:1rem; } .card { border:1px solid #ddd; border-radius:8px; padding:.5rem; } img { width:100%; height:120px; object-fit:cover; border-radius:4px; }`,
})
export class WebsiteGalleryComponent implements OnInit {
  private readonly service = inject(WebsiteService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  gallery = signal<GymWebsiteGalleryItem[]>([]);
  form = this.fb.group({ fileId: [null as number | null, Validators.required], caption: [''], displayOrder: [0] });

  ngOnInit(): void { this.load(); }
  load(): void {
    this.service.getGallery().subscribe({
      next: (r) => { if (r.success && r.data) this.gallery.set(r.data); },
      error: () => this.notify.error('Failed to load gallery'),
    });
  }
  add(): void {
    if (this.form.invalid) return;
    this.service.addGalleryItem(this.form.getRawValue() as never).subscribe({
      next: () => { this.notify.success('Added'); this.form.reset({ displayOrder: 0 }); this.load(); },
      error: () => this.notify.error('Failed to add image'),
    });
  }
  remove(id: number): void {
    this.service.deleteGalleryItem(id).subscribe({ next: () => { this.notify.success('Removed'); this.load(); }, error: () => this.notify.error('Remove failed') });
  }
}
