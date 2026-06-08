import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  CreateNotificationTemplateRequest,
  NotificationDashboard,
  NotificationLog,
  NotificationSearchQuery,
  NotificationSetting,
  NotificationTemplate,
  SendTestNotificationRequest,
  UpdateNotificationSettingRequest,
  UpdateNotificationTemplateRequest,
} from '../../shared/models/notification.models';

@Injectable({ providedIn: 'root' })
export class GymNotificationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/notifications`;

  getDashboard(): Observable<ApiResponse<NotificationDashboard>> {
    return this.http.get<ApiResponse<NotificationDashboard>>(`${this.base}/dashboard`);
  }

  getTemplates(includeInactive = false): Observable<ApiResponse<NotificationTemplate[]>> {
    return this.http.get<ApiResponse<NotificationTemplate[]>>(`${this.base}/templates?includeInactive=${includeInactive}`);
  }

  createTemplate(dto: CreateNotificationTemplateRequest): Observable<ApiResponse<NotificationTemplate>> {
    return this.http.post<ApiResponse<NotificationTemplate>>(`${this.base}/templates`, dto);
  }

  updateTemplate(id: number, dto: UpdateNotificationTemplateRequest): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/templates/${id}`, dto);
  }

  deleteTemplate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/templates/${id}`);
  }

  getSettings(): Observable<ApiResponse<NotificationSetting[]>> {
    return this.http.get<ApiResponse<NotificationSetting[]>>(`${this.base}/settings`);
  }

  updateSettings(settings: UpdateNotificationSettingRequest[]): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/settings`, settings);
  }

  searchHistory(query: NotificationSearchQuery): Observable<ApiResponse<PagedResult<NotificationLog>>> {
    let params = new HttpParams()
      .set('pageNumber', String(query.pageNumber ?? 1))
      .set('pageSize', String(query.pageSize ?? 20));
    if (query.search) params = params.set('search', query.search);
    if (query.notificationType) params = params.set('notificationType', query.notificationType);
    if (query.status) params = params.set('status', query.status);
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);
    return this.http.get<ApiResponse<PagedResult<NotificationLog>>>(`${this.base}/history`, { params });
  }

  sendTest(dto: SendTestNotificationRequest): Observable<ApiResponse<NotificationLog>> {
    return this.http.post<ApiResponse<NotificationLog>>(`${this.base}/test`, dto);
  }
}
