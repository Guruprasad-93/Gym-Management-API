import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreatePrivilegeRequest,
  Privilege,
  RolePermissionMatrix,
} from '../../shared/models/role.models';

@Injectable({ providedIn: 'root' })
export class PrivilegeService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/privileges`;
  private readonly matrixBase = `${environment.apiUrl}/role-privileges`;

  getAll(): Observable<ApiResponse<Privilege[]>> {
    return this.http.get<ApiResponse<Privilege[]>>(this.base);
  }

  create(dto: CreatePrivilegeRequest): Observable<ApiResponse<Privilege>> {
    return this.http.post<ApiResponse<Privilege>>(this.base, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  getMatrix(): Observable<ApiResponse<RolePermissionMatrix>> {
    return this.http.get<ApiResponse<RolePermissionMatrix>>(`${this.matrixBase}/matrix`);
  }

  assign(roleId: number, privilegeId: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(this.matrixBase, { roleId, privilegeId });
  }

  remove(roleId: number, privilegeId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(
      `${this.matrixBase}/role/${roleId}/privilege/${privilegeId}`
    );
  }
}
