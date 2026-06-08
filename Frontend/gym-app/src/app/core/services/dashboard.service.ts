import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';

export interface DashboardStats {
  totalGyms: number;
  activeGyms: number;
  totalMembers: number;
  activeMembers: number;
  membersWithTrainer: number;
  totalRevenue: number;
  expiredMemberships: number;
  activeMemberships: number;
  pendingRenewals: number;
  monthlyRevenue: number;
  totalTrainers: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);

  getStats(): Observable<ApiResponse<DashboardStats>> {
    return this.http.get<ApiResponse<DashboardStats>>(`${environment.apiUrl}/dashboard/stats`);
  }
}
