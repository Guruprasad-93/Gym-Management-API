import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  Branch,
  BranchAnalytics,
  BranchAnnouncement,
  BranchDashboardItem,
  BranchTarget,
  BranchTransfer,
  CreateBranchAnnouncementDto,
  CreateBranchDto,
  TransferMemberDto,
  TransferTrainerDto,
  UpsertBranchTargetDto,
} from '../../shared/models/branch.models';

@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/branches`;

  getPaged(pageNumber = 1, pageSize = 20, search?: string): Observable<ApiResponse<PagedResult<Branch>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<PagedResult<Branch>>>(this.base, { params });
  }

  getList(): Observable<ApiResponse<Branch[]>> {
    return this.http.get<ApiResponse<Branch[]>>(`${this.base}/list`);
  }

  create(dto: CreateBranchDto): Observable<ApiResponse<Branch>> {
    return this.http.post<ApiResponse<Branch>>(this.base, dto);
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/deactivate`, {});
  }

  getDashboard(branchId?: number): Observable<ApiResponse<BranchDashboardItem[]>> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);
    return this.http.get<ApiResponse<BranchDashboardItem[]>>(`${this.base}/dashboard`, { params });
  }

  getAnalytics(months = 6): Observable<ApiResponse<BranchAnalytics>> {
    return this.http.get<ApiResponse<BranchAnalytics>>(`${this.base}/analytics`, { params: { months } });
  }

  getTransfers(pageNumber = 1, entityType?: string): Observable<ApiResponse<PagedResult<BranchTransfer>>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', 20);
    if (entityType) params = params.set('entityType', entityType);
    return this.http.get<ApiResponse<PagedResult<BranchTransfer>>>(`${this.base}/transfers`, { params });
  }

  transferMember(dto: TransferMemberDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.base}/transfers/members`, dto);
  }

  transferTrainer(dto: TransferTrainerDto): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.base}/transfers/trainers`, dto);
  }

  getTargets(branchId?: number, targetMonth?: string): Observable<ApiResponse<BranchTarget[]>> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId);
    if (targetMonth) params = params.set('targetMonth', targetMonth);
    return this.http.get<ApiResponse<BranchTarget[]>>(`${this.base}/targets`, { params });
  }

  upsertTarget(dto: UpsertBranchTargetDto): Observable<ApiResponse<BranchTarget>> {
    return this.http.post<ApiResponse<BranchTarget>>(`${this.base}/targets`, dto);
  }

  getAnnouncements(): Observable<ApiResponse<BranchAnnouncement[]>> {
    return this.http.get<ApiResponse<BranchAnnouncement[]>>(`${this.base}/announcements`);
  }

  createAnnouncement(dto: CreateBranchAnnouncementDto): Observable<ApiResponse<BranchAnnouncement>> {
    return this.http.post<ApiResponse<BranchAnnouncement>>(`${this.base}/announcements`, dto);
  }
}
