import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { WebsiteService } from '../../core/services/website.service';
import { PublicWebsiteContextService } from './public-website-context.service';

@Component({
  selector: 'app-public-website-shell',
  standalone: true,
  providers: [PublicWebsiteContextService],
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    @if (ctx.site()) {
      <div class="site" [style.--primary]="ctx.site()!.settings.primaryColor || '#1565c0'" [style.--secondary]="ctx.site()!.settings.secondaryColor || '#ff6f00'">
        <header class="topbar">
          <div class="brand">
            @if (ctx.site()!.settings.logoUrl) { <img [src]="ctx.site()!.settings.logoUrl" alt="Logo" /> }
            <strong>{{ ctx.site()!.settings.websiteTitle || ctx.site()!.gymName }}</strong>
          </div>
          <nav>
            <a [routerLink]="['/website', ctx.slug]" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Home</a>
            <a [routerLink]="['/website', ctx.slug, 'about']" routerLinkActive="active">About</a>
            <a [routerLink]="['/website', ctx.slug, 'plans']" routerLinkActive="active">Plans</a>
            <a [routerLink]="['/website', ctx.slug, 'trainers']" routerLinkActive="active">Trainers</a>
            <a [routerLink]="['/website', ctx.slug, 'gallery']" routerLinkActive="active">Gallery</a>
            <a [routerLink]="['/website', ctx.slug, 'contact']" routerLinkActive="active">Contact</a>
          </nav>
        </header>
        <router-outlet />
        <footer class="footer">
          <p>{{ ctx.site()!.settings.address }}</p>
          <p>{{ ctx.site()!.settings.contactPhone }} · {{ ctx.site()!.settings.contactEmail }}</p>
        </footer>
      </div>
    }
  `,
  styles: `
    .site { min-height: 100vh; background: #fafafa; color: #222; font-family: 'Segoe UI', sans-serif; }
    .topbar { display:flex; justify-content:space-between; align-items:center; padding:1rem 2rem; background:#fff; box-shadow:0 2px 8px rgba(0,0,0,.06); position:sticky; top:0; z-index:10; flex-wrap:wrap; gap:1rem; }
    .brand { display:flex; align-items:center; gap:.75rem; } .brand img { height:40px; }
    nav { display:flex; gap:1rem; flex-wrap:wrap; } nav a { text-decoration:none; color:#444; font-weight:600; } nav a.active { color: var(--primary); }
    .footer { text-align:center; padding:2rem; background:#111; color:#eee; margin-top:2rem; }
  `,
})
export class PublicWebsiteShellComponent implements OnInit {
  readonly ctx = inject(PublicWebsiteContextService);
  private readonly route = inject(ActivatedRoute);

  ngOnInit(): void { this.ctx.loadFromRoute(this.route); }
}

@Component({
  selector: 'app-public-website-home',
  standalone: true,
  imports: [RouterLink],
  template: `
    @if (ctx.site(); as site) {
      <section class="hero" [style.background-image]="site.settings.bannerUrl ? 'url(' + site.settings.bannerUrl + ')' : ''">
        <div class="overlay">
          <h1>{{ site.settings.websiteTitle || site.gymName }}</h1>
          <p>{{ site.settings.websiteDescription }}</p>
          <a class="cta" [routerLink]="['/website', ctx.slug, 'contact']">Book Free Trial</a>
        </div>
      </section>
      @for (section of site.sections; track section.id) {
        <section class="block"><h2>{{ section.title }}</h2><p>{{ section.description }}</p></section>
      }
      <section class="block">
        <h2>Membership Plans</h2>
        <div class="cards">
          @for (plan of site.membershipPlans.slice(0, 3); track plan.id) {
            <article class="card"><h3>{{ plan.planName }}</h3><p>{{ plan.description }}</p><strong>₹{{ plan.price }}</strong></article>
          }
        </div>
      </section>
    }
  `,
  styles: `
    .hero { min-height:360px; background: linear-gradient(135deg, var(--primary,#1565c0), var(--secondary,#ff6f00)); background-size:cover; }
    .overlay { padding:4rem 2rem; color:#fff; text-align:center; background:rgba(0,0,0,.35); }
    .cta { display:inline-block; margin-top:1rem; padding:.75rem 1.5rem; background:#fff; color:#111; border-radius:999px; text-decoration:none; font-weight:700; }
    .block { padding:2rem; max-width:960px; margin:0 auto; }
    .cards { display:grid; grid-template-columns:repeat(auto-fit,minmax(200px,1fr)); gap:1rem; }
    .card { background:#fff; border-radius:12px; padding:1rem; box-shadow:0 4px 16px rgba(0,0,0,.08); }
  `,
})
export class PublicWebsiteHomeComponent {
  readonly ctx = inject(PublicWebsiteContextService);
}

@Component({
  selector: 'app-public-website-about',
  standalone: true,
  template: `@if (ctx.site(); as site) { <section class="page"><h2>About {{ site.gymName }}</h2><p>{{ site.settings.websiteDescription }}</p></section> }`,
  styles: `.page { max-width:960px; margin:2rem auto; padding:0 1rem; }`,
})
export class PublicWebsiteAboutComponent { readonly ctx = inject(PublicWebsiteContextService); }

@Component({
  selector: 'app-public-website-plans',
  standalone: true,
  template: `
    @if (ctx.site(); as site) {
      <section class="page"><h2>Membership Plans</h2>
        <div class="cards">
          @for (plan of site.membershipPlans; track plan.id) {
            <article class="card"><h3>{{ plan.planName }}</h3><p>{{ plan.description }}</p><p>{{ plan.durationInMonths }} months</p><strong>₹{{ plan.price }}</strong></article>
          }
        </div>
      </section>
    }`,
  styles: `.page { max-width:960px; margin:2rem auto; padding:0 1rem; } .cards { display:grid; grid-template-columns:repeat(auto-fit,minmax(220px,1fr)); gap:1rem; } .card { background:#fff; padding:1rem; border-radius:12px; box-shadow:0 4px 12px rgba(0,0,0,.08); }`,
})
export class PublicWebsitePlansComponent { readonly ctx = inject(PublicWebsiteContextService); }

@Component({
  selector: 'app-public-website-trainers',
  standalone: true,
  template: `
    @if (ctx.site(); as site) {
      <section class="page"><h2>Our Trainers</h2>
        <div class="cards">
          @for (t of site.trainers; track t.id) {
            <article class="card">
              @if (t.profileImageUrl) { <img [src]="t.profileImageUrl" alt="" /> }
              <h3>{{ t.fullName }}</h3><p>{{ t.specialization }}</p><p>{{ t.bio }}</p>
            </article>
          }
        </div>
      </section>
    }`,
  styles: `.page { max-width:960px; margin:2rem auto; padding:0 1rem; } .cards { display:grid; grid-template-columns:repeat(auto-fit,minmax(220px,1fr)); gap:1rem; } .card img { width:100%; height:140px; object-fit:cover; border-radius:8px; } .card { background:#fff; padding:1rem; border-radius:12px; }`,
})
export class PublicWebsiteTrainersComponent { readonly ctx = inject(PublicWebsiteContextService); }

@Component({
  selector: 'app-public-website-gallery',
  standalone: true,
  template: `
    @if (ctx.site(); as site) {
      <section class="page"><h2>Gallery</h2>
        <div class="grid">
          @for (g of site.gallery; track g.id) {
            <figure><img [src]="g.publicUrl" [alt]="g.caption ?? ''" /><figcaption>{{ g.caption }}</figcaption></figure>
          }
        </div>
      </section>
    }`,
  styles: `.page { max-width:960px; margin:2rem auto; padding:0 1rem; } .grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(180px,1fr)); gap:1rem; } img { width:100%; height:140px; object-fit:cover; border-radius:8px; }`,
})
export class PublicWebsiteGalleryPageComponent { readonly ctx = inject(PublicWebsiteContextService); }

@Component({
  selector: 'app-public-website-contact',
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <section class="contact">
      <h2>Contact Us</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-form-field><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
        <mat-form-field><mat-label>Mobile</mat-label><input matInput formControlName="mobileNumber" /></mat-form-field>
        <mat-form-field><mat-label>Email</mat-label><input matInput formControlName="email" /></mat-form-field>
        <mat-form-field><mat-label>Interested Plan</mat-label><input matInput formControlName="interestedPlan" /></mat-form-field>
        <mat-form-field class="full"><mat-label>Message</mat-label><textarea matInput formControlName="notes"></textarea></mat-form-field>
        <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">Send Enquiry</button>
      </form>
      <hr />
      <h3>Book Free Trial</h3>
      <form [formGroup]="trialForm" (ngSubmit)="bookTrial()">
        <mat-form-field><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
        <mat-form-field><mat-label>Mobile</mat-label><input matInput formControlName="mobileNumber" /></mat-form-field>
        <mat-form-field><mat-label>Preferred Date</mat-label><input matInput type="date" formControlName="preferredDate" /></mat-form-field>
        <mat-form-field><mat-label>Preferred Time</mat-label><input matInput type="time" formControlName="preferredTime" /></mat-form-field>
        <button mat-stroked-button color="accent" type="submit" [disabled]="trialForm.invalid">Book Trial</button>
      </form>
    </section>
  `,
  styles: `.contact { max-width:640px; margin:2rem auto; padding:0 1rem; display:grid; gap:1rem; } .full { width:100%; }`,
})
export class PublicWebsiteContactComponent {
  readonly ctx = inject(PublicWebsiteContextService);
  private readonly service = inject(WebsiteService);
  private readonly fb = inject(FormBuilder);
  form = this.fb.group({ name: ['', Validators.required], mobileNumber: ['', Validators.required], email: [''], interestedPlan: [''], notes: [''] });
  trialForm = this.fb.group({ name: ['', Validators.required], mobileNumber: ['', Validators.required], email: [''], preferredDate: ['', Validators.required], preferredTime: ['09:00', Validators.required] });

  submit(): void {
    if (this.form.invalid) return;
    this.service.submitLead({ websiteSlug: this.ctx.slug, ...this.form.getRawValue() } as never).subscribe();
  }

  bookTrial(): void {
    if (this.trialForm.invalid) return;
    const v = this.trialForm.getRawValue();
    this.service.bookTrial({ websiteSlug: this.ctx.slug, name: v.name!, mobileNumber: v.mobileNumber!, email: v.email ?? undefined, preferredDate: v.preferredDate!, preferredTime: v.preferredTime! }).subscribe();
  }
}
