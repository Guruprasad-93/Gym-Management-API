import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  PushCampaignHistory,
  PushNotificationAnalytics,
  SendPushCampaign,
} from '../../shared/models/mobile-push.models';

@Injectable({ providedIn: 'root' })
export class MobilePushService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/mobile/admin`;

  getAnalytics(fromDate?: string, toDate?: string): Observable<ApiResponse<PushNotificationAnalytics>> {
    const params: Record<string, string> = {};
    if (fromDate) params['fromDate'] = fromDate;
    if (toDate) params['toDate'] = toDate;
    return this.http.get<ApiResponse<PushNotificationAnalytics>>(`${this.base}/analytics`, { params });
  }

  getCampaigns(pageNumber = 1, pageSize = 20): Observable<ApiResponse<PagedResult<PushCampaignHistory>>> {
    return this.http.get<ApiResponse<PagedResult<PushCampaignHistory>>>(`${this.base}/campaigns`, {
      params: { pageNumber, pageSize },
    });
  }

  sendCampaign(dto: SendPushCampaign): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/send`, dto);
  }
}
