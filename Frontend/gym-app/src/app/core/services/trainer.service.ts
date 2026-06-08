import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { Member } from '../../shared/models/member.models';
import { PagedRequest, PagedResult } from '../../shared/models/paged.models';
import {
  AssignMembersRequest,
  CreateTrainerRequest,
  Trainer,
  TrainerDashboard,
  UpdateTrainerRequest,
} from '../../shared/models/trainer.models';

@Injectable({ providedIn: 'root' })
export class TrainerService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/trainers`;

  getPaged(
    gymId: string | null,
    paging: PagedRequest,
    includeInactive = false
  ): Observable<ApiResponse<PagedResult<Trainer>>> {
    let params = new HttpParams()
      .set('pageNumber', String(paging.pageNumber ?? 1))
      .set('pageSize', String(paging.pageSize ?? 10))
      .set('sortColumn', paging.sortColumn ?? 'UserName')
      .set('sortDirection', paging.sortDirection ?? 'asc')
      .set('includeInactive', String(includeInactive));
    if (paging.search) params = params.set('search', paging.search);
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<PagedResult<Trainer>>>(this.base, { params });
  }

  getMe(): Observable<ApiResponse<Trainer>> {
    return this.http.get<ApiResponse<Trainer>>(`${this.base}/me`);
  }

  getById(id: number): Observable<ApiResponse<Trainer>> {
    return this.http.get<ApiResponse<Trainer>>(`${this.base}/${id}`);
  }

  getDashboard(id: number): Observable<ApiResponse<TrainerDashboard>> {
    return this.http.get<ApiResponse<TrainerDashboard>>(`${this.base}/${id}/dashboard`);
  }

  getMembers(id: number, search?: string): Observable<ApiResponse<Member[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<Member[]>>(`${this.base}/${id}/members`, { params });
  }

  getUnassignedMembers(id: number, search?: string): Observable<ApiResponse<Member[]>> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<Member[]>>(`${this.base}/${id}/unassigned-members`, { params });
  }

  create(dto: CreateTrainerRequest): Observable<ApiResponse<Trainer>> {
    return this.http.post<ApiResponse<Trainer>>(this.base, dto);
  }

  update(id: number, dto: UpdateTrainerRequest): Observable<ApiResponse<Trainer>> {
    return this.http.put<ApiResponse<Trainer>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  assignMembers(id: number, dto: AssignMembersRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/${id}/assign-members`, dto);
  }

  removeMemberAssignment(memberId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/members/${memberId}/assignment`);
  }
}
