import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreatePaymentRequest,
  Invoice,
  MonthlyRevenue,
  Payment,
  RevenueDashboard,
} from '../../shared/models/membership-payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/payments`;

  getAll(search?: string): Observable<ApiResponse<Payment[]>> {
    let url = this.base;
    if (search) url += `?search=${encodeURIComponent(search)}`;
    return this.http.get<ApiResponse<Payment[]>>(url);
  }

  getByMember(memberId: number): Observable<ApiResponse<Payment[]>> {
    return this.http.get<ApiResponse<Payment[]>>(`${this.base}/member/${memberId}`);
  }

  create(dto: CreatePaymentRequest): Observable<ApiResponse<Payment>> {
    return this.http.post<ApiResponse<Payment>>(this.base, dto);
  }

  getRevenueDashboard(): Observable<ApiResponse<RevenueDashboard>> {
    return this.http.get<ApiResponse<RevenueDashboard>>(`${this.base}/revenue/dashboard`);
  }

  getMonthlyRevenue(months = 12): Observable<ApiResponse<MonthlyRevenue[]>> {
    return this.http.get<ApiResponse<MonthlyRevenue[]>>(`${this.base}/revenue/monthly?months=${months}`);
  }

  generateInvoice(paymentId: number): Observable<ApiResponse<Invoice>> {
    return this.http.post<ApiResponse<Invoice>>(`${this.base}/${paymentId}/invoice`, {});
  }

  downloadInvoice(invoiceId: number): Observable<Blob> {
    return this.http.get(`${this.base}/invoices/${invoiceId}/download`, { responseType: 'blob' });
  }
}
