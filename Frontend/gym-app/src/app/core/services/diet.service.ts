import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  AssignDietPlanRequest,
  CreateDietPlanRequest,
  DietCategory,
  DietPlanDetail,
  DietPlanListItem,
  MemberDietPlanView,
} from '../../shared/models/diet.models';

@Injectable({ providedIn: 'root' })
export class DietService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/diet-plans`;

  getCategories(includeInactive = false): Observable<ApiResponse<DietCategory[]>> {
    const params = new HttpParams().set('includeInactive', String(includeInactive));
    return this.http.get<ApiResponse<DietCategory[]>>(`${this.base}/categories`, { params });
  }

  getPlans(includeInactive = false, categoryId?: number, search?: string): Observable<ApiResponse<DietPlanListItem[]>> {
    let params = new HttpParams().set('includeInactive', String(includeInactive));
    if (categoryId) params = params.set('categoryId', String(categoryId));
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<DietPlanListItem[]>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<DietPlanDetail>> {
    return this.http.get<ApiResponse<DietPlanDetail>>(`${this.base}/${id}`);
  }

  create(dto: CreateDietPlanRequest): Observable<ApiResponse<DietPlanDetail>> {
    return this.http.post<ApiResponse<DietPlanDetail>>(this.base, dto);
  }

  update(id: number, dto: CreateDietPlanRequest): Observable<ApiResponse<DietPlanDetail>> {
    return this.http.put<ApiResponse<DietPlanDetail>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  setActive(id: number, isActive: boolean): Observable<ApiResponse<unknown>> {
    const params = new HttpParams().set('isActive', String(isActive));
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/active`, null, { params });
  }

  clone(id: number, newPlanName?: string): Observable<ApiResponse<DietPlanDetail>> {
    return this.http.post<ApiResponse<DietPlanDetail>>(`${this.base}/${id}/clone`, { newPlanName });
  }

  assign(dto: AssignDietPlanRequest): Observable<ApiResponse<MemberDietPlanView>> {
    return this.http.post<ApiResponse<MemberDietPlanView>>(`${this.base}/assign`, dto);
  }

  getMemberDiet(memberId: number): Observable<ApiResponse<MemberDietPlanView>> {
    return this.http.get<ApiResponse<MemberDietPlanView>>(`${this.base}/members/${memberId}`);
  }

  getMyDiet(): Observable<ApiResponse<MemberDietPlanView>> {
    return this.http.get<ApiResponse<MemberDietPlanView>>(`${this.base}/members/me`);
  }

  downloadPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/pdf`, { responseType: 'blob' });
  }

  downloadExcel(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/excel`, { responseType: 'blob' });
  }
}
