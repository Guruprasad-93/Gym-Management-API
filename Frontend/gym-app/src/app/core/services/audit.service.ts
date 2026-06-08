import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import { AuditDashboard, AuditLog, AuditSearchQuery } from '../../shared/models/audit.models';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/audit-logs`;

  getDashboard(fromDate?: string, toDate?: string): Observable<ApiResponse<AuditDashboard>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<AuditDashboard>>(`${this.base}/dashboard`, { params });
  }

  search(query: AuditSearchQuery): Observable<ApiResponse<PagedResult<AuditLog>>> {
    return this.http.get<ApiResponse<PagedResult<AuditLog>>>(this.base, { params: this.toParams(query) });
  }

  downloadPdf(query: AuditSearchQuery): Observable<Blob> {
    return this.http.get(`${this.base}/export/pdf`, { params: this.toParams(query), responseType: 'blob' });
  }

  downloadExcel(query: AuditSearchQuery): Observable<Blob> {
    return this.http.get(`${this.base}/export/excel`, { params: this.toParams(query), responseType: 'blob' });
  }

  private toParams(query: AuditSearchQuery): HttpParams {
    let params = new HttpParams();
    if (query.userId) params = params.set('userId', query.userId);
    if (query.entityName) params = params.set('entityName', query.entityName);
    if (query.actionType) params = params.set('actionType', query.actionType);
    if (query.entityId) params = params.set('entityId', query.entityId);
    if (query.search) params = params.set('search', query.search);
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);
    if (query.pageNumber) params = params.set('pageNumber', String(query.pageNumber));
    if (query.pageSize) params = params.set('pageSize', String(query.pageSize));
    return params;
  }
}
