import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { CreateGymRequest, Gym, UpdateGymRequest } from '../../shared/models/gym.models';

@Injectable({ providedIn: 'root' })
export class GymService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/gyms`;

  getAll(): Observable<ApiResponse<Gym[]>> {
    return this.http.get<ApiResponse<Gym[]>>(this.base);
  }

  getById(id: string): Observable<ApiResponse<Gym>> {
    return this.http.get<ApiResponse<Gym>>(`${this.base}/${id}`);
  }

  create(dto: CreateGymRequest): Observable<ApiResponse<Gym>> {
    return this.http.post<ApiResponse<Gym>>(this.base, dto);
  }

  update(id: string, dto: UpdateGymRequest): Observable<ApiResponse<Gym>> {
    return this.http.put<ApiResponse<Gym>>(`${this.base}/${id}`, dto);
  }

  delete(id: string): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  activate(id: string): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/${id}/deactivate`, {});
  }
}
