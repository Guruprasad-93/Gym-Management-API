import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { SaasPlan } from '../../shared/models/saas.models';

export interface RegisterGymRequest {
  gymName: string;
  ownerName: string;
  mobile: string;
  email: string;
  address?: string;
  password?: string;
}

export interface RegisterGymResult {
  gymId: string;
  adminUserId: string;
  gymName: string;
  adminEmail: string;
  temporaryPassword?: string;
  remainingTrialDays: number;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class OnboardingService {
  private readonly http = inject(HttpClient);

  register(dto: RegisterGymRequest): Observable<ApiResponse<RegisterGymResult>> {
    return this.http.post<ApiResponse<RegisterGymResult>>(`${environment.apiUrl}/onboarding/register`, dto);
  }

  getPublicPlans(): Observable<ApiResponse<SaasPlan[]>> {
    return this.http.get<ApiResponse<SaasPlan[]>>(`${environment.apiUrl}/onboarding/plans`);
  }
}
