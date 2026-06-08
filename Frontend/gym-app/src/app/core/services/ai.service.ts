import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  AiAnalytics,
  AiDashboard,
  AiInsight,
  AiRecommendation,
  LeadScore,
  MemberRiskScore,
} from '../../shared/models/ai.models';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/ai`;

  getDashboard(): Observable<ApiResponse<AiDashboard>> {
    return this.http.get<ApiResponse<AiDashboard>>(`${this.base}/dashboard`);
  }

  getRecommendations(pageNumber = 1, pageSize = 20, memberId?: number): Observable<ApiResponse<PagedResult<AiRecommendation>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (memberId) params = params.set('memberId', memberId);
    return this.http.get<ApiResponse<PagedResult<AiRecommendation>>>(`${this.base}/recommendations`, { params });
  }

  acceptRecommendation(recommendationId: number): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/recommendations/accept`, { recommendationId });
  }

  getMemberRisk(pageNumber = 1, pageSize = 20, churnRisk?: string): Observable<ApiResponse<PagedResult<MemberRiskScore>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (churnRisk) params = params.set('churnRisk', churnRisk);
    return this.http.get<ApiResponse<PagedResult<MemberRiskScore>>>(`${this.base}/member-risk`, { params });
  }

  getLeadScoring(pageNumber = 1, pageSize = 20, scoreCategory?: string): Observable<ApiResponse<PagedResult<LeadScore>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (scoreCategory) params = params.set('scoreCategory', scoreCategory);
    return this.http.get<ApiResponse<PagedResult<LeadScore>>>(`${this.base}/lead-scoring`, { params });
  }

  getBusinessInsights(pageNumber = 1, pageSize = 20): Observable<ApiResponse<PagedResult<AiInsight>>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<ApiResponse<PagedResult<AiInsight>>>(`${this.base}/business-insights`, { params });
  }

  getAnalytics(): Observable<ApiResponse<AiAnalytics>> {
    return this.http.get<ApiResponse<AiAnalytics>>(`${this.base}/analytics`);
  }
}
