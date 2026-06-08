import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  UpdateWhiteLabelDomain,
  UpsertWhiteLabelSettings,
  WhiteLabelEmailTemplate,
  WhiteLabelLoginBranding,
  WhiteLabelMobileSettings,
  WhiteLabelPlatformDashboard,
  WhiteLabelPreview,
  WhiteLabelSettings,
} from '../../shared/models/white-label.models';

@Injectable({ providedIn: 'root' })
export class WhiteLabelService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/white-label`;
  private readonly publicBase = `${environment.apiUrl}/public/white-label`;
  private readonly platformBase = `${environment.apiUrl}/platform/white-label`;

  getSettings(): Observable<ApiResponse<WhiteLabelSettings>> {
    return this.http.get<ApiResponse<WhiteLabelSettings>>(`${this.base}/settings`);
  }

  upsertSettings(dto: UpsertWhiteLabelSettings): Observable<ApiResponse<WhiteLabelSettings>> {
    return this.http.put<ApiResponse<WhiteLabelSettings>>(`${this.base}/settings`, dto);
  }

  enable(): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/settings/enable`, {});
  }

  disable(): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/settings/disable`, {});
  }

  updateDomain(dto: UpdateWhiteLabelDomain): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/domain`, dto);
  }

  getEmailTemplates(): Observable<ApiResponse<WhiteLabelEmailTemplate[]>> {
    return this.http.get<ApiResponse<WhiteLabelEmailTemplate[]>>(`${this.base}/email-templates`);
  }

  createEmailTemplate(dto: Omit<WhiteLabelEmailTemplate, 'id' | 'gymId'>): Observable<ApiResponse<WhiteLabelEmailTemplate>> {
    return this.http.post<ApiResponse<WhiteLabelEmailTemplate>>(`${this.base}/email-templates`, dto);
  }

  updateEmailTemplate(id: number, dto: Omit<WhiteLabelEmailTemplate, 'id' | 'gymId'>): Observable<ApiResponse<WhiteLabelEmailTemplate>> {
    return this.http.put<ApiResponse<WhiteLabelEmailTemplate>>(`${this.base}/email-templates/${id}`, dto);
  }

  deleteEmailTemplate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/email-templates/${id}`);
  }

  getMobileSettings(): Observable<ApiResponse<WhiteLabelMobileSettings>> {
    return this.http.get<ApiResponse<WhiteLabelMobileSettings>>(`${this.base}/mobile-settings`);
  }

  upsertMobileSettings(dto: Partial<WhiteLabelMobileSettings>): Observable<ApiResponse<WhiteLabelMobileSettings>> {
    return this.http.put<ApiResponse<WhiteLabelMobileSettings>>(`${this.base}/mobile-settings`, dto);
  }

  getPreview(): Observable<ApiResponse<WhiteLabelPreview>> {
    return this.http.get<ApiResponse<WhiteLabelPreview>>(`${this.base}/preview`);
  }

  getLoginBranding(query: { gymId?: string; subDomain?: string; customDomain?: string }): Observable<ApiResponse<WhiteLabelLoginBranding>> {
    let params = new HttpParams();
    if (query.gymId) params = params.set('gymId', query.gymId);
    if (query.subDomain) params = params.set('subDomain', query.subDomain);
    if (query.customDomain) params = params.set('customDomain', query.customDomain);
    return this.http.get<ApiResponse<WhiteLabelLoginBranding>>(`${this.publicBase}/login-branding`, { params });
  }

  getPlatformDashboard(): Observable<ApiResponse<WhiteLabelPlatformDashboard>> {
    return this.http.get<ApiResponse<WhiteLabelPlatformDashboard>>(`${this.platformBase}/dashboard`);
  }
}
