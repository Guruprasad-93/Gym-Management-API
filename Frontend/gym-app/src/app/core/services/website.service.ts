import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  GymWebsiteGalleryItem,
  GymWebsitePage,
  GymWebsiteSection,
  GymWebsiteSettings,
  GymWebsiteTestimonial,
  PublicTrialBooking,
  PublicWebsite,
  PublicWebsiteLead,
  WebsiteAnalyticsOverview,
  WebsiteLeadCapture,
} from '../../shared/models/website.models';

@Injectable({ providedIn: 'root' })
export class WebsiteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/website`;
  private readonly publicBase = `${environment.apiUrl}/public/website`;

  getSettings(): Observable<ApiResponse<GymWebsiteSettings>> {
    return this.http.get<ApiResponse<GymWebsiteSettings>>(`${this.base}/settings`);
  }

  upsertSettings(dto: Partial<GymWebsiteSettings>): Observable<ApiResponse<GymWebsiteSettings>> {
    return this.http.put<ApiResponse<GymWebsiteSettings>>(`${this.base}/settings`, dto);
  }

  publish(): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/settings/publish`, {});
  }

  unpublish(): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/settings/unpublish`, {});
  }

  getPages(): Observable<ApiResponse<GymWebsitePage[]>> {
    return this.http.get<ApiResponse<GymWebsitePage[]>>(`${this.base}/pages`);
  }

  createPage(dto: Partial<GymWebsitePage>): Observable<ApiResponse<GymWebsitePage>> {
    return this.http.post<ApiResponse<GymWebsitePage>>(`${this.base}/pages`, dto);
  }

  deletePage(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/pages/${id}`);
  }

  getSections(): Observable<ApiResponse<GymWebsiteSection[]>> {
    return this.http.get<ApiResponse<GymWebsiteSection[]>>(`${this.base}/sections`);
  }

  createSection(dto: Partial<GymWebsiteSection>): Observable<ApiResponse<GymWebsiteSection>> {
    return this.http.post<ApiResponse<GymWebsiteSection>>(`${this.base}/sections`, dto);
  }

  getGallery(): Observable<ApiResponse<GymWebsiteGalleryItem[]>> {
    return this.http.get<ApiResponse<GymWebsiteGalleryItem[]>>(`${this.base}/gallery`);
  }

  addGalleryItem(dto: { fileId: number; caption?: string; displayOrder?: number }): Observable<ApiResponse<GymWebsiteGalleryItem>> {
    return this.http.post<ApiResponse<GymWebsiteGalleryItem>>(`${this.base}/gallery`, dto);
  }

  deleteGalleryItem(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/gallery/${id}`);
  }

  getTestimonials(): Observable<ApiResponse<GymWebsiteTestimonial[]>> {
    return this.http.get<ApiResponse<GymWebsiteTestimonial[]>>(`${this.base}/testimonials`);
  }

  createTestimonial(dto: Partial<GymWebsiteTestimonial>): Observable<ApiResponse<GymWebsiteTestimonial>> {
    return this.http.post<ApiResponse<GymWebsiteTestimonial>>(`${this.base}/testimonials`, dto);
  }

  getLeads(pageNumber = 1, pageSize = 20): Observable<ApiResponse<PagedResult<WebsiteLeadCapture>>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<WebsiteLeadCapture>>>(`${this.base}/leads`, { params });
  }

  getAnalytics(days = 30): Observable<ApiResponse<WebsiteAnalyticsOverview>> {
    return this.http.get<ApiResponse<WebsiteAnalyticsOverview>>(`${this.base}/analytics`, { params: { days } });
  }

  getPublicSite(gymSlug: string): Observable<ApiResponse<PublicWebsite>> {
    return this.http.get<ApiResponse<PublicWebsite>>(`${this.publicBase}/${gymSlug}`);
  }

  submitLead(dto: PublicWebsiteLead): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.publicBase}/lead`, dto);
  }

  bookTrial(dto: PublicTrialBooking): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.publicBase}/trial-booking`, dto);
  }
}
