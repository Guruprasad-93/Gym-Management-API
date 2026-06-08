import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  AssignTrainerToLeadRequest,
  ConvertLeadRequest,
  ConvertLeadResult,
  CreateFollowUpRequest,
  CreateLeadRequest,
  Lead,
  LeadAnalytics,
  LeadDashboard,
  LeadDetail,
  LeadFollowUp,
  LeadSearchQuery,
  LeadTrial,
  RecordTrialFeedbackRequest,
  ScheduleTrialRequest,
  UpdateLeadRequest,
} from '../../shared/models/lead.models';

@Injectable({ providedIn: 'root' })
export class LeadService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/leads`;

  getPaged(query: LeadSearchQuery): Observable<ApiResponse<PagedResult<Lead>>> {
    let params = new HttpParams()
      .set('pageNumber', String(query.pageNumber ?? 1))
      .set('pageSize', String(query.pageSize ?? 10))
      .set('sortColumn', query.sortColumn ?? 'CreatedDate')
      .set('sortDirection', query.sortDirection ?? 'DESC');
    if (query.search) params = params.set('search', query.search);
    if (query.status) params = params.set('status', query.status);
    if (query.leadSource) params = params.set('leadSource', query.leadSource);
    if (query.gymId) params = params.set('gymId', query.gymId);
    return this.http.get<ApiResponse<PagedResult<Lead>>>(this.base, { params });
  }

  getById(id: number, gymId?: string): Observable<ApiResponse<LeadDetail>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<LeadDetail>>(`${this.base}/${id}`, { params });
  }

  getDashboard(gymId?: string): Observable<ApiResponse<LeadDashboard>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<LeadDashboard>>(`${this.base}/dashboard`, { params });
  }

  getAnalytics(gymId?: string): Observable<ApiResponse<LeadAnalytics>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<LeadAnalytics>>(`${this.base}/analytics`, { params });
  }

  getPendingFollowUps(gymId?: string): Observable<ApiResponse<LeadFollowUp[]>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<LeadFollowUp[]>>(`${this.base}/followups/pending`, { params });
  }

  getTodaysTrials(gymId?: string): Observable<ApiResponse<LeadTrial[]>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<LeadTrial[]>>(`${this.base}/trials/today`, { params });
  }

  create(dto: CreateLeadRequest): Observable<ApiResponse<Lead>> {
    return this.http.post<ApiResponse<Lead>>(this.base, dto);
  }

  update(id: number, dto: UpdateLeadRequest): Observable<ApiResponse<Lead>> {
    return this.http.put<ApiResponse<Lead>>(`${this.base}/${id}`, dto);
  }

  updateStatus(id: number, status: string, gymId?: string): Observable<ApiResponse<Lead>> {
    return this.http.patch<ApiResponse<Lead>>(`${this.base}/${id}/status`, { status, gymId });
  }

  delete(id: number, gymId?: string): Observable<ApiResponse<unknown>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`, { params });
  }

  assignTrainer(dto: AssignTrainerToLeadRequest): Observable<ApiResponse<Lead>> {
    return this.http.post<ApiResponse<Lead>>(`${this.base}/assign-trainer`, dto);
  }

  scheduleTrial(dto: ScheduleTrialRequest): Observable<ApiResponse<LeadTrial>> {
    return this.http.post<ApiResponse<LeadTrial>>(`${this.base}/schedule-trial`, dto);
  }

  createFollowUp(dto: CreateFollowUpRequest): Observable<ApiResponse<LeadFollowUp>> {
    return this.http.post<ApiResponse<LeadFollowUp>>(`${this.base}/followup`, dto);
  }

  recordTrialFeedback(dto: RecordTrialFeedbackRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/trial-feedback`, dto);
  }

  convertToMember(dto: ConvertLeadRequest): Observable<ApiResponse<ConvertLeadResult>> {
    return this.http.post<ApiResponse<ConvertLeadResult>>(`${this.base}/convert-member`, dto);
  }

  exportPdf(reportType: string, query: LeadSearchQuery = {}): Observable<Blob> {
    let params = this.buildExportParams(reportType, query);
    return this.http.get(`${this.base}/export/pdf`, { params, responseType: 'blob' });
  }

  exportExcel(reportType: string, query: LeadSearchQuery = {}): Observable<Blob> {
    let params = this.buildExportParams(reportType, query);
    return this.http.get(`${this.base}/export/excel`, { params, responseType: 'blob' });
  }

  private buildExportParams(reportType: string, query: LeadSearchQuery): HttpParams {
    let params = new HttpParams().set('reportType', reportType);
    if (query.search) params = params.set('search', query.search);
    if (query.status) params = params.set('status', query.status);
    if (query.leadSource) params = params.set('leadSource', query.leadSource);
    if (query.gymId) params = params.set('gymId', query.gymId);
    return params;
  }
}
