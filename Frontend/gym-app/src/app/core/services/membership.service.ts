import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreateMembershipPlanRequest,
  Membership,
  MembershipPlan,
  RenewMembershipRequest,
  UpdateMembershipPlanRequest,
  CreateMembershipRequest,
} from '../../shared/models/membership-payment.models';

@Injectable({ providedIn: 'root' })
export class MembershipService {
  private readonly http = inject(HttpClient);
  private readonly plansBase = `${environment.apiUrl}/membership-plans`;
  private readonly base = `${environment.apiUrl}/memberships`;

  getPlans(includeInactive = false): Observable<ApiResponse<MembershipPlan[]>> {
    return this.http.get<ApiResponse<MembershipPlan[]>>(`${this.plansBase}?includeInactive=${includeInactive}`);
  }

  createPlan(dto: CreateMembershipPlanRequest): Observable<ApiResponse<MembershipPlan>> {
    return this.http.post<ApiResponse<MembershipPlan>>(this.plansBase, dto);
  }

  updatePlan(id: number, dto: UpdateMembershipPlanRequest): Observable<ApiResponse<MembershipPlan>> {
    return this.http.put<ApiResponse<MembershipPlan>>(`${this.plansBase}/${id}`, dto);
  }

  deletePlan(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.plansBase}/${id}`);
  }

  getAll(search?: string, includeInactive = false): Observable<ApiResponse<Membership[]>> {
    let url = `${this.base}?includeInactive=${includeInactive}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    return this.http.get<ApiResponse<Membership[]>>(url);
  }

  getExpired(): Observable<ApiResponse<Membership[]>> {
    return this.http.get<ApiResponse<Membership[]>>(`${this.base}/expired`);
  }

  create(dto: CreateMembershipRequest): Observable<ApiResponse<Membership>> {
    return this.http.post<ApiResponse<Membership>>(this.base, dto);
  }

  renew(id: number, dto: RenewMembershipRequest): Observable<ApiResponse<Membership>> {
    return this.http.post<ApiResponse<Membership>>(`${this.base}/${id}/renew`, dto);
  }

  cancel(id: number, notes?: string): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/${id}/cancel`, { notes });
  }
}
