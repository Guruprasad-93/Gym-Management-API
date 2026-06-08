import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  GymBranding,
  GymSubscription,
  GymUsage,
  SaasPaymentOrder,
  SaasPlan,
  SaasPlatformDashboard,
  UpdateGymBranding,
} from '../../shared/models/saas.models';

@Injectable({ providedIn: 'root' })
export class SaasSubscriptionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/saas`;

  getSubscription(gymId?: string): Observable<ApiResponse<GymSubscription>> {
    return this.http.get<ApiResponse<GymSubscription>>(`${this.base}/subscription`, { params: this.params(gymId) });
  }

  getUsage(gymId?: string): Observable<ApiResponse<GymUsage>> {
    return this.http.get<ApiResponse<GymUsage>>(`${this.base}/usage`, { params: this.params(gymId) });
  }

  getPlans(): Observable<ApiResponse<SaasPlan[]>> {
    return this.http.get<ApiResponse<SaasPlan[]>>(`${this.base}/plans`);
  }

  createPaymentOrder(saasPlanId: number, billingCycle: string, gymId?: string): Observable<ApiResponse<SaasPaymentOrder>> {
    return this.http.post<ApiResponse<SaasPaymentOrder>>(
      `${this.base}/payments/order`,
      { saasPlanId, billingCycle },
      { params: this.params(gymId) }
    );
  }

  verifyPayment(payload: {
    saasPaymentId: number;
    razorpayOrderId: string;
    razorpayPaymentId: string;
    razorpaySignature: string;
  }, gymId?: string): Observable<ApiResponse<GymSubscription>> {
    return this.http.post<ApiResponse<GymSubscription>>(`${this.base}/payments/verify`, payload, {
      params: this.params(gymId),
    });
  }

  cancel(cancelAtPeriodEnd = true, gymId?: string): Observable<ApiResponse<GymSubscription>> {
    let p = this.params(gymId).set('cancelAtPeriodEnd', String(cancelAtPeriodEnd));
    return this.http.post<ApiResponse<GymSubscription>>(`${this.base}/subscription/cancel`, {}, { params: p });
  }

  getPlatformDashboard(): Observable<ApiResponse<SaasPlatformDashboard>> {
    return this.http.get<ApiResponse<SaasPlatformDashboard>>(`${this.base}/platform/dashboard`);
  }

  getBranding(gymId?: string): Observable<ApiResponse<GymBranding>> {
    return this.http.get<ApiResponse<GymBranding>>(`${this.base}/branding`, { params: this.params(gymId) });
  }

  updateBranding(dto: UpdateGymBranding, gymId?: string): Observable<ApiResponse<GymBranding>> {
    return this.http.put<ApiResponse<GymBranding>>(`${this.base}/branding`, dto, { params: this.params(gymId) });
  }

  private params(gymId?: string): HttpParams {
    let p = new HttpParams();
    if (gymId) p = p.set('gymId', gymId);
    return p;
  }
}
