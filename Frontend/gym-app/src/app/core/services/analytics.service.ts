import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  AnalyticsDashboard,
  AttendanceAnalytics,
  DietAnalytics,
  MembershipAnalytics,
  RevenueAnalytics,
  TrainerAnalytics,
  WorkoutAnalytics,
} from '../../shared/models/analytics.models';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/analytics`;

  getDashboard(gymId?: string): Observable<ApiResponse<AnalyticsDashboard>> {
    return this.http.get<ApiResponse<AnalyticsDashboard>>(`${this.base}/dashboard`, {
      params: this.params(gymId),
    });
  }

  getRevenue(gymId?: string): Observable<ApiResponse<RevenueAnalytics>> {
    return this.http.get<ApiResponse<RevenueAnalytics>>(`${this.base}/revenue`, {
      params: this.params(gymId),
    });
  }

  getMembers(gymId?: string): Observable<ApiResponse<MembershipAnalytics>> {
    return this.http.get<ApiResponse<MembershipAnalytics>>(`${this.base}/members`, {
      params: this.params(gymId),
    });
  }

  getAttendance(gymId?: string): Observable<ApiResponse<AttendanceAnalytics>> {
    return this.http.get<ApiResponse<AttendanceAnalytics>>(`${this.base}/attendance`, {
      params: this.params(gymId),
    });
  }

  getTrainers(gymId?: string): Observable<ApiResponse<TrainerAnalytics>> {
    return this.http.get<ApiResponse<TrainerAnalytics>>(`${this.base}/trainers`, {
      params: this.params(gymId),
    });
  }

  getWorkouts(gymId?: string): Observable<ApiResponse<WorkoutAnalytics>> {
    return this.http.get<ApiResponse<WorkoutAnalytics>>(`${this.base}/workouts`, {
      params: this.params(gymId),
    });
  }

  getDiets(gymId?: string): Observable<ApiResponse<DietAnalytics>> {
    return this.http.get<ApiResponse<DietAnalytics>>(`${this.base}/diets`, {
      params: this.params(gymId),
    });
  }

  exportPdf(reportType = 'dashboard', gymId?: string): Observable<Blob> {
    return this.http.get(`${this.base}/export/pdf`, {
      params: this.exportParams(reportType, gymId),
      responseType: 'blob',
    });
  }

  exportExcel(reportType = 'dashboard', gymId?: string): Observable<Blob> {
    return this.http.get(`${this.base}/export/excel`, {
      params: this.exportParams(reportType, gymId),
      responseType: 'blob',
    });
  }

  downloadBlob(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  private params(gymId?: string): HttpParams {
    let p = new HttpParams();
    if (gymId) p = p.set('gymId', gymId);
    return p;
  }

  private exportParams(reportType: string, gymId?: string): HttpParams {
    let p = new HttpParams().set('reportType', reportType);
    if (gymId) p = p.set('gymId', gymId);
    return p;
  }
}
