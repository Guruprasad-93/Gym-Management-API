import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DietService } from '../../../core/services/diet.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { DietCategory, MEAL_TIMES } from '../../../shared/models/diet.models';

@Component({
  selector: 'app-diet-plan-editor',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    PageHeaderComponent,
  ],
  template: `
    <app-page-header [title]="isEdit ? 'Edit Diet Plan' : 'New Diet Plan'" subtitle="Meals, portions, and calories">
      <button mat-stroked-button type="button" routerLink="/gym-admin/diet-plans">Back</button>
    </app-page-header>

    @if (loading()) {
      <mat-spinner />
    } @else {
      <form class="editor" [formGroup]="form" (ngSubmit)="save()">
        <div class="header-fields">
          <mat-form-field appearance="outline">
            <mat-label>Plan name</mat-label>
            <input matInput formControlName="planName" required />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Category</mat-label>
            <mat-select formControlName="dietCategoryId">
              <mat-option [value]="null">None</mat-option>
              @for (c of categories(); track c.dietCategoryId) {
                <mat-option [value]="c.dietCategoryId">{{ c.categoryName }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Target calories</mat-label>
            <input matInput type="number" formControlName="targetCalories" />
          </mat-form-field>
          <mat-checkbox formControlName="isActive">Active</mat-checkbox>
        </div>
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput rows="2" formControlName="description"></textarea>
        </mat-form-field>

        <div class="items-header">
          <h3>Meal items</h3>
          <button mat-stroked-button type="button" (click)="addItem()"><mat-icon>add</mat-icon> Add item</button>
        </div>

        <div formArrayName="items" class="items-list">
          @for (item of items.controls; track $index; let i = $index) {
            <div class="item-row" [formGroupName]="i">
              <mat-form-field appearance="outline">
                <mat-label>Meal time</mat-label>
                <mat-select formControlName="mealTime">
                  @for (m of mealTimes; track m) {
                    <mat-option [value]="m">{{ m }}</mat-option>
                  }
                </mat-select>
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Food</mat-label>
                <input matInput formControlName="foodName" required />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Quantity</mat-label>
                <input matInput formControlName="quantity" placeholder="e.g. 200g" />
              </mat-form-field>
              <mat-form-field appearance="outline">
                <mat-label>Calories</mat-label>
                <input matInput type="number" formControlName="calories" />
              </mat-form-field>
              <mat-form-field appearance="outline" class="notes">
                <mat-label>Notes</mat-label>
                <input matInput formControlName="notes" />
              </mat-form-field>
              <button mat-icon-button type="button" color="warn" (click)="removeItem(i)"><mat-icon>delete</mat-icon></button>
            </div>
          }
        </div>

        <div class="actions">
          <button mat-flat-button color="primary" type="submit" [disabled]="saving() || form.invalid">Save plan</button>
        </div>
      </form>
    }
  `,
  styles: [
    `
      .editor { background: #fff; padding: 1.5rem; border-radius: 8px; }
      .header-fields { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; align-items: center; }
      .full-width { width: 100%; }
      .items-header { display: flex; justify-content: space-between; align-items: center; margin: 1rem 0; }
      .item-row {
        display: grid;
        grid-template-columns: 1fr 1.5fr 1fr 100px 1.5fr 40px;
        gap: 0.5rem;
        align-items: start;
        margin-bottom: 0.5rem;
      }
      @media (max-width: 900px) {
        .item-row { grid-template-columns: 1fr 1fr; }
      }
      .actions { margin-top: 1.5rem; }
    `,
  ],
})
export class DietPlanEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly svc = inject(DietService);
  private readonly notify = inject(NotificationService);

  readonly mealTimes = MEAL_TIMES;
  loading = signal(false);
  saving = signal(false);
  categories = signal<DietCategory[]>([]);
  isEdit = false;
  planId: number | null = null;

  form = this.fb.group({
    planName: ['', Validators.required],
    description: [''],
    dietCategoryId: [null as number | null],
    targetCalories: [null as number | null],
    isActive: [true],
    items: this.fb.array([]),
  });

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  ngOnInit(): void {
    this.svc.getCategories().subscribe({
      next: (res) => { if (res.success && res.data) this.categories.set(res.data); },
    });
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit = true;
      this.planId = Number(id);
      this.loading.set(true);
      this.svc.getById(this.planId).subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) this.patchForm(res.data);
        },
        error: () => {
          this.loading.set(false);
          this.notify.error('Failed to load plan');
        },
      });
    } else {
      this.addItem();
    }
  }

  addItem(): void {
    this.items.push(
      this.fb.group({
        mealTime: ['Breakfast', Validators.required],
        foodName: ['', Validators.required],
        quantity: [''],
        calories: [null as number | null],
        notes: [''],
        sortOrder: [this.items.length],
      })
    );
  }

  removeItem(i: number): void {
    this.items.removeAt(i);
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    const raw = this.form.getRawValue();
    const payload = {
      planName: raw.planName!,
      description: raw.description || undefined,
      dietCategoryId: raw.dietCategoryId ?? undefined,
      targetCalories: raw.targetCalories ?? undefined,
      isActive: raw.isActive ?? true,
      items: (raw.items as Array<{
        mealTime: string | null;
        foodName: string | null;
        quantity: string | null;
        calories: number | null;
        notes: string | null;
      }>).map((it, idx) => ({
        mealTime: it.mealTime!,
        foodName: it.foodName!,
        quantity: it.quantity || undefined,
        calories: it.calories ?? undefined,
        notes: it.notes || undefined,
        sortOrder: idx,
      })),
    };
    const req = this.isEdit && this.planId
      ? this.svc.update(this.planId, payload)
      : this.svc.create(payload);
    req.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.notify.success('Plan saved');
          this.router.navigate(['/gym-admin/diet-plans']);
        }
      },
      error: () => {
        this.saving.set(false);
        this.notify.error('Save failed');
      },
    });
  }

  private patchForm(plan: {
    planName: string;
    description?: string;
    dietCategoryId?: number;
    targetCalories?: number;
    isActive: boolean;
    items: { mealTime: string; foodName: string; quantity?: string; calories?: number; notes?: string; sortOrder: number }[];
  }): void {
    this.form.patchValue({
      planName: plan.planName,
      description: plan.description ?? '',
      dietCategoryId: plan.dietCategoryId ?? null,
      targetCalories: plan.targetCalories ?? null,
      isActive: plan.isActive,
    });
    plan.items.forEach((it) => {
      this.items.push(
        this.fb.group({
          mealTime: [it.mealTime, Validators.required],
          foodName: [it.foodName, Validators.required],
          quantity: [it.quantity ?? ''],
          calories: [it.calories ?? null],
          notes: [it.notes ?? ''],
          sortOrder: [it.sortOrder],
        })
      );
    });
  }
}
