import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  AssignTrainerRequest,
  CreateMemberRequest,
  Member,
  MemberDetails,
  UpdateMemberRequest,
} from '../../shared/models/member.models';
import { PagedRequest, PagedResult } from '../../shared/models/paged.models';

@Injectable({ providedIn: 'root' })
export class MemberService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/members`;

  getPaged(
    gymId: string | null,
    paging: PagedRequest,
    includeInactive = false
  ): Observable<ApiResponse<PagedResult<Member>>> {
    let params = new HttpParams()
      .set('pageNumber', String(paging.pageNumber ?? 1))
      .set('pageSize', String(paging.pageSize ?? 10))
      .set('sortColumn', paging.sortColumn ?? 'FullName')
      .set('sortDirection', paging.sortDirection ?? 'asc')
      .set('includeInactive', String(includeInactive));
    if (paging.search) params = params.set('search', paging.search);
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<PagedResult<Member>>>(this.base, { params });
  }

  getMe(): Observable<ApiResponse<Member>> {
    return this.http.get<ApiResponse<Member>>(`${this.base}/me`);
  }

  getById(id: number): Observable<ApiResponse<Member>> {
    return this.http.get<ApiResponse<Member>>(`${this.base}/${id}`);
  }

  getDetails(id: number): Observable<ApiResponse<MemberDetails>> {
    return this.http.get<ApiResponse<MemberDetails>>(`${this.base}/${id}/details`);
  }

  create(dto: CreateMemberRequest): Observable<ApiResponse<Member>> {
    return this.http.post<ApiResponse<Member>>(this.base, dto);
  }

  update(id: number, dto: UpdateMemberRequest): Observable<ApiResponse<Member>> {
    return this.http.put<ApiResponse<Member>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  activate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/deactivate`, {});
  }

  assignTrainer(id: number, dto: AssignTrainerRequest): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/${id}/assign-trainer`, dto);
  }

  removeTrainerAssignment(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}/trainer-assignment`);
  }
}
