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
import { GymWebsitePage } from '../../../shared/models/website.models';

@Component({
  selector: 'app-website-pages',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule, MatTableModule, PageHeaderComponent],
  template: `
    <app-page-header title="Website Pages" subtitle="Manage custom content pages">
      <a mat-stroked-button routerLink="/gym-admin/website-builder">Back</a>
    </app-page-header>
    <form [formGroup]="form" (ngSubmit)="create()" class="form-row">
      <mat-form-field><mat-label>Page Name</mat-label><input matInput formControlName="pageName" /></mat-form-field>
      <mat-form-field><mat-label>Slug</mat-label><input matInput formControlName="slug" /></mat-form-field>
      <mat-form-field class="wide"><mat-label>Content</mat-label><textarea matInput rows="2" formControlName="pageContent"></textarea></mat-form-field>
      <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">Add Page</button>
    </form>
    <table mat-table [dataSource]="pages()" class="full-width">
      <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>Name</th><td mat-cell *matCellDef="let p">{{ p.pageName }}</td></ng-container>
      <ng-container matColumnDef="slug"><th mat-header-cell *matHeaderCellDef>Slug</th><td mat-cell *matCellDef="let p">{{ p.slug }}</td></ng-container>
      <ng-container matColumnDef="actions"><th mat-header-cell *matHeaderCellDef></th><td mat-cell *matCellDef="let p"><button mat-button color="warn" (click)="remove(p.id)">Delete</button></td></ng-container>
      <tr mat-header-row *matHeaderRowDef="cols"></tr>
      <tr mat-row *matRowDef="let row; columns: cols"></tr>
    </table>
  `,
  styles: `.form-row { display:flex; flex-wrap:wrap; gap:1rem; margin-bottom:1rem; } .wide { min-width:280px; } .full-width { width:100%; }`,
})
export class WebsitePagesComponent implements OnInit {
  private readonly service = inject(WebsiteService);
  private readonly notify = inject(NotificationService);
  private readonly fb = inject(FormBuilder);
  pages = signal<GymWebsitePage[]>([]);
  cols = ['name', 'slug', 'actions'];
  form = this.fb.group({ pageName: ['', Validators.required], slug: ['', Validators.required], pageContent: [''], displayOrder: [0] });

  ngOnInit(): void { this.load(); }
  load(): void {
    this.service.getPages().subscribe({
      next: (r) => { if (r.success && r.data) this.pages.set(r.data); },
      error: () => this.notify.error('Failed to load pages'),
    });
  }
  create(): void {
    if (this.form.invalid) return;
    this.service.createPage(this.form.getRawValue() as never).subscribe({
      next: () => { this.notify.success('Page created'); this.form.reset({ displayOrder: 0 }); this.load(); },
      error: () => this.notify.error('Failed to create page'),
    });
  }
  remove(id: number): void {
    this.service.deletePage(id).subscribe({ next: () => { this.notify.success('Deleted'); this.load(); }, error: () => this.notify.error('Delete failed') });
  }
}
