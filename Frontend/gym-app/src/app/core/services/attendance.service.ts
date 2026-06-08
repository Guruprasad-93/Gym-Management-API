import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  AttendanceDashboard,
  AttendanceQuery,
  AttendanceStatus,
  DailyAttendanceReport,
  MemberAttendance,
  MonthlyAttendanceReport,
  TrainerAttendance,
} from '../../shared/models/attendance.models';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/attendance`;

  getStatuses(): Observable<ApiResponse<AttendanceStatus[]>> {
    return this.http.get<ApiResponse<AttendanceStatus[]>>(`${this.base}/statuses`);
  }

  getDashboard(): Observable<ApiResponse<AttendanceDashboard>> {
    return this.http.get<ApiResponse<AttendanceDashboard>>(`${this.base}/dashboard`);
  }

  getToday(search?: string): Observable<ApiResponse<MemberAttendance[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<MemberAttendance[]>>(`${this.base}/today`, { params });
  }

  getPaged(query: AttendanceQuery): Observable<ApiResponse<PagedResult<MemberAttendance>>> {
    let params = this.queryParams(query);
    return this.http.get<ApiResponse<PagedResult<MemberAttendance>>>(this.base, { params });
  }

  getMemberHistory(memberId: number, query: AttendanceQuery): Observable<ApiResponse<PagedResult<MemberAttendance>>> {
    let params = this.queryParams(query);
    return this.http.get<ApiResponse<PagedResult<MemberAttendance>>>(`${this.base}/members/${memberId}/history`, { params });
  }

  checkIn(memberId: number, notes?: string): Observable<ApiResponse<MemberAttendance>> {
    return this.http.post<ApiResponse<MemberAttendance>>(`${this.base}/check-in`, { memberId, notes });
  }

  checkOut(memberId: number, notes?: string): Observable<ApiResponse<MemberAttendance>> {
    return this.http.post<ApiResponse<MemberAttendance>>(`${this.base}/check-out`, { memberId, notes });
  }

  mark(dto: {
    memberId: number;
    attendanceDate: string;
    attendanceStatusId: number;
    notes?: string;
  }): Observable<ApiResponse<MemberAttendance>> {
    return this.http.post<ApiResponse<MemberAttendance>>(`${this.base}/mark`, dto);
  }

  getDailyReport(date?: string): Observable<ApiResponse<DailyAttendanceReport>> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get<ApiResponse<DailyAttendanceReport>>(`${this.base}/reports/daily`, { params });
  }

  getMonthlyReport(year?: number, month?: number): Observable<ApiResponse<MonthlyAttendanceReport>> {
    let params = new HttpParams();
    if (year) params = params.set('year', String(year));
    if (month) params = params.set('month', String(month));
    return this.http.get<ApiResponse<MonthlyAttendanceReport>>(`${this.base}/reports/monthly`, { params });
  }

  downloadDailyPdf(date?: string): Observable<Blob> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get(`${this.base}/reports/daily/export/pdf`, { params, responseType: 'blob' });
  }

  downloadDailyExcel(date?: string): Observable<Blob> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    return this.http.get(`${this.base}/reports/daily/export/excel`, { params, responseType: 'blob' });
  }

  downloadMonthlyPdf(year?: number, month?: number): Observable<Blob> {
    let params = new HttpParams();
    if (year) params = params.set('year', String(year));
    if (month) params = params.set('month', String(month));
    return this.http.get(`${this.base}/reports/monthly/export/pdf`, { params, responseType: 'blob' });
  }

  downloadMonthlyExcel(year?: number, month?: number): Observable<Blob> {
    let params = new HttpParams();
    if (year) params = params.set('year', String(year));
    if (month) params = params.set('month', String(month));
    return this.http.get(`${this.base}/reports/monthly/export/excel`, { params, responseType: 'blob' });
  }

  downloadMemberHistoryExcel(memberId: number, query: AttendanceQuery): Observable<Blob> {
    const params = this.queryParams(query);
    return this.http.get(`${this.base}/members/${memberId}/history/export/excel`, { params, responseType: 'blob' });
  }

  getTrainerAttendance(trainerId?: number, from?: string, to?: string, page = 1, size = 10): Observable<ApiResponse<PagedResult<TrainerAttendance>>> {
    let params = new HttpParams().set('pageNumber', String(page)).set('pageSize', String(size));
    if (trainerId) params = params.set('trainerId', String(trainerId));
    if (from) params = params.set('fromDate', from);
    if (to) params = params.set('toDate', to);
    return this.http.get<ApiResponse<PagedResult<TrainerAttendance>>>(`${this.base}/trainers`, { params });
  }

  trainerCheckIn(trainerId: number, notes?: string): Observable<ApiResponse<TrainerAttendance>> {
    return this.http.post<ApiResponse<TrainerAttendance>>(`${this.base}/trainers/check-in`, { trainerId, notes });
  }

  trainerCheckOut(trainerId: number): Observable<ApiResponse<TrainerAttendance>> {
    return this.http.post<ApiResponse<TrainerAttendance>>(`${this.base}/trainers/check-out`, { trainerId });
  }

  private queryParams(query: AttendanceQuery): HttpParams {
    let params = new HttpParams();
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);
    if (query.memberId) params = params.set('memberId', String(query.memberId));
    if (query.statusId) params = params.set('statusId', String(query.statusId));
    if (query.search) params = params.set('search', query.search);
    if (query.pageNumber) params = params.set('pageNumber', String(query.pageNumber));
    if (query.pageSize) params = params.set('pageSize', String(query.pageSize));
    if (query.sortColumn) params = params.set('sortColumn', query.sortColumn);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    return params;
  }
}
