import { Injectable, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { WebsiteService } from '../../core/services/website.service';
import { PublicWebsite } from '../../shared/models/website.models';

@Injectable()
export class PublicWebsiteContextService {
  private readonly service = inject(WebsiteService);
  readonly site = signal<PublicWebsite | null>(null);
  slug = '';

  loadFromRoute(route: ActivatedRoute): void {
    let r: ActivatedRoute | null = route;
    while (r) {
      const slug = r.snapshot.paramMap.get('gymSlug');
      if (slug) {
        this.slug = slug;
        break;
      }
      r = r.parent;
    }
    if (!this.slug) return;
    this.service.getPublicSite(this.slug).subscribe({
      next: (res) => { if (res.success && res.data) this.site.set(res.data); },
    });
  }
}
