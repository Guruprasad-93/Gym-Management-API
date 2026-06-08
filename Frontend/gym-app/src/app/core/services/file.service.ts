import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import { MemberFile, StoredFile, TrainerFile, UploadFileRequest } from '../../shared/models/file.models';

@Injectable({ providedIn: 'root' })
export class FileService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/files`;

  upload(file: File, request: UploadFileRequest): Observable<ApiResponse<StoredFile>> {
    const form = new FormData();
    form.append('file', file, file.name);
    form.append('fileCategory', request.fileCategory);
    if (request.gymId) form.append('gymId', request.gymId);
    if (request.memberId != null) form.append('memberId', String(request.memberId));
    if (request.trainerId != null) form.append('trainerId', String(request.trainerId));
    if (request.dietPlanId != null) form.append('dietPlanId', String(request.dietPlanId));
    if (request.assignedDietPlanId != null) form.append('assignedDietPlanId', String(request.assignedDietPlanId));
    if (request.workoutPlanId != null) form.append('workoutPlanId', String(request.workoutPlanId));
    if (request.assignedWorkoutPlanId != null) form.append('assignedWorkoutPlanId', String(request.assignedWorkoutPlanId));
    if (request.notes) form.append('notes', request.notes);
    if (request.takenAt) form.append('takenAt', request.takenAt);
    return this.http.post<ApiResponse<StoredFile>>(`${this.base}/upload`, form);
  }

  delete(fileId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${fileId}`);
  }

  getMemberFiles(memberId: number, category?: string): Observable<ApiResponse<MemberFile[]>> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    return this.http.get<ApiResponse<MemberFile[]>>(`${this.base}/members/${memberId}`, { params });
  }

  getTrainerFiles(trainerId: number, category?: string): Observable<ApiResponse<TrainerFile[]>> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    return this.http.get<ApiResponse<TrainerFile[]>>(`${this.base}/trainers/${trainerId}`, { params });
  }

  getGymLogo(gymId?: string | null): Observable<ApiResponse<StoredFile | null>> {
    let params = new HttpParams();
    if (gymId) params = params.set('gymId', gymId);
    return this.http.get<ApiResponse<StoredFile | null>>(`${this.base}/gym/logo`, { params });
  }

  contentUrl(publicUrl: string): string {
    if (publicUrl.startsWith('http')) return publicUrl;
    return publicUrl.startsWith('/') ? publicUrl : `/${publicUrl}`;
  }
}
