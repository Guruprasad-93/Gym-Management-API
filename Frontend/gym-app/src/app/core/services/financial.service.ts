import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { PagedResult } from '../../shared/models/paged.models';
import {
  CreateExpenseRequest,
  Expense,
  ExpenseCategory,
  ExpenseSearchQuery,
  FinancialDashboard,
  GeneratePayrollRequest,
  Payroll,
  ProfitLossSummary,
  TrainerCommission,
  UpdatePayrollRequest,
} from '../../shared/models/financial.models';

@Injectable({ providedIn: 'root' })
export class ExpenseService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/expenses`;

  getCategories(gymId?: string): Observable<ApiResponse<ExpenseCategory[]>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<ExpenseCategory[]>>(`${this.base}/categories`, { params });
  }

  getPaged(query: ExpenseSearchQuery): Observable<ApiResponse<PagedResult<Expense>>> {
    let params = new HttpParams()
      .set('pageNumber', String(query.pageNumber ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));
    if (query.search) params = params.set('search', query.search);
    if (query.categoryId) params = params.set('categoryId', String(query.categoryId));
    if (query.fromDate) params = params.set('fromDate', query.fromDate);
    if (query.toDate) params = params.set('toDate', query.toDate);
    if (query.gymId) params = params.set('gymId', query.gymId);
    return this.http.get<ApiResponse<PagedResult<Expense>>>(this.base, { params });
  }

  getById(id: number, gymId?: string): Observable<ApiResponse<Expense>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<Expense>>(`${this.base}/${id}`, { params });
  }

  create(dto: CreateExpenseRequest): Observable<ApiResponse<Expense>> {
    return this.http.post<ApiResponse<Expense>>(this.base, dto);
  }

  update(id: number, dto: CreateExpenseRequest): Observable<ApiResponse<Expense>> {
    return this.http.put<ApiResponse<Expense>>(`${this.base}/${id}`, dto);
  }

  delete(id: number, gymId?: string): Observable<ApiResponse<unknown>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`, { params });
  }

  exportPdf(fromDate?: string, toDate?: string): Observable<Blob> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get(`${this.base}/export/pdf`, { params, responseType: 'blob' });
  }

  exportExcel(fromDate?: string, toDate?: string): Observable<Blob> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get(`${this.base}/export/excel`, { params, responseType: 'blob' });
  }
}

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/payroll`;

  getPaged(query: { gymId?: string; salaryMonth?: string; status?: string; pageNumber?: number; pageSize?: number }): Observable<ApiResponse<PagedResult<Payroll>>> {
    let params = new HttpParams()
      .set('pageNumber', String(query.pageNumber ?? 1))
      .set('pageSize', String(query.pageSize ?? 10));
    if (query.gymId) params = params.set('gymId', query.gymId);
    if (query.salaryMonth) params = params.set('salaryMonth', query.salaryMonth);
    if (query.status) params = params.set('status', query.status);
    return this.http.get<ApiResponse<PagedResult<Payroll>>>(this.base, { params });
  }

  generate(dto: GeneratePayrollRequest): Observable<ApiResponse<{ generatedCount: number; salaryMonth: string }>> {
    return this.http.post<ApiResponse<{ generatedCount: number; salaryMonth: string }>>(`${this.base}/generate`, dto);
  }

  approve(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/${id}/approve`, {});
  }

  pay(id: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/${id}/pay`, {});
  }

  update(id: number, dto: UpdatePayrollRequest): Observable<ApiResponse<Payroll>> {
    return this.http.put<ApiResponse<Payroll>>(`${this.base}/${id}`, dto);
  }

  getCommissions(fromDate?: string, toDate?: string): Observable<ApiResponse<TrainerCommission[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<TrainerCommission[]>>(`${this.base}/commissions`, { params });
  }

  getMyCommissions(): Observable<ApiResponse<TrainerCommission[]>> {
    return this.http.get<ApiResponse<TrainerCommission[]>>(`${this.base}/commissions/me`);
  }

  exportPdf(salaryMonth?: string): Observable<Blob> {
    let params = new HttpParams();
    if (salaryMonth) params = params.set('salaryMonth', salaryMonth);
    return this.http.get(`${this.base}/export/pdf`, { params, responseType: 'blob' });
  }
}

@Injectable({ providedIn: 'root' })
export class FinancialService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/financial`;

  getDashboard(gymId?: string): Observable<ApiResponse<FinancialDashboard>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<FinancialDashboard>>(`${this.base}/dashboard`, { params });
  }

  getProfitLoss(gymId?: string, fromDate?: string, toDate?: string): Observable<ApiResponse<ProfitLossSummary>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<ProfitLossSummary>>(`${this.base}/profit-loss`, { params });
  }

  exportPdf(): Observable<Blob> {
    return this.http.get(`${this.base}/export/pdf`, { responseType: 'blob' });
  }

  exportExcel(): Observable<Blob> {
    return this.http.get(`${this.base}/export/excel`, { responseType: 'blob' });
  }
}
