import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreateGymAdminRequest,
  CreateGymAdminResult,
  GymAdmin,
  ResendTemporaryPasswordResult,
  UpdateGymAdminRequest,
} from '../../shared/models/gym-admin.models';
import { PagedRequest, PagedResult } from '../../shared/models/paged.models';

@Injectable({ providedIn: 'root' })
export class GymAdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/gym-admins`;

  getAll(gymId: string | null, paging: PagedRequest): Observable<ApiResponse<PagedResult<GymAdmin>>> {
    let params = new HttpParams()
      .set('pageNumber', String(paging.pageNumber ?? 1))
      .set('pageSize', String(paging.pageSize ?? 10))
      .set('sortColumn', paging.sortColumn ?? 'Name')
      .set('sortDirection', paging.sortDirection ?? 'asc');
    if (paging.search) params = params.set('search', paging.search);
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<PagedResult<GymAdmin>>>(this.base, { params });
  }

  getById(userId: string): Observable<ApiResponse<GymAdmin>> {
    return this.http.get<ApiResponse<GymAdmin>>(`${this.base}/${userId}`);
  }

  create(dto: CreateGymAdminRequest): Observable<ApiResponse<CreateGymAdminResult>> {
    return this.http.post<ApiResponse<CreateGymAdminResult>>(this.base, dto);
  }

  update(userId: string, dto: UpdateGymAdminRequest): Observable<ApiResponse<GymAdmin>> {
    return this.http.put<ApiResponse<GymAdmin>>(`${this.base}/${userId}`, dto);
  }

  activate(userId: string): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${userId}/activate`, {});
  }

  deactivate(userId: string): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${userId}/deactivate`, {});
  }

  resendTemporaryPassword(userId: string): Observable<ApiResponse<ResendTemporaryPasswordResult>> {
    return this.http.post<ApiResponse<ResendTemporaryPasswordResult>>(
      `${this.base}/${userId}/resend-temporary-password`,
      {}
    );
  }
}
