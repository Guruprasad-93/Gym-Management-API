import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  AssignWorkoutPlanRequest,
  Exercise,
  ExerciseCategory,
  MemberWorkoutView,
  UpdateWorkoutProgressRequest,
  WorkoutPlanDetail,
  WorkoutPlanExercise,
  WorkoutPlanExerciseInput,
  WorkoutPlanListItem,
} from '../../shared/models/workout.models';

@Injectable({ providedIn: 'root' })
export class WorkoutService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/workout-plans`;

  getCategories(includeInactive = false): Observable<ApiResponse<ExerciseCategory[]>> {
    return this.http.get<ApiResponse<ExerciseCategory[]>>(`${this.base}/exercise-categories`, {
      params: new HttpParams().set('includeInactive', String(includeInactive)),
    });
  }

  getExercises(includeInactive = false, categoryId?: number, search?: string): Observable<ApiResponse<Exercise[]>> {
    let params = new HttpParams().set('includeInactive', String(includeInactive));
    if (categoryId) params = params.set('categoryId', String(categoryId));
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<Exercise[]>>(`${this.base}/exercises`, { params });
  }

  createExercise(dto: Partial<Exercise>): Observable<ApiResponse<Exercise>> {
    return this.http.post<ApiResponse<Exercise>>(`${this.base}/exercises`, dto);
  }

  updateExercise(id: number, dto: Partial<Exercise>): Observable<ApiResponse<Exercise>> {
    return this.http.put<ApiResponse<Exercise>>(`${this.base}/exercises/${id}`, dto);
  }

  deleteExercise(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/exercises/${id}`);
  }

  getPlans(includeInactive = false, search?: string): Observable<ApiResponse<WorkoutPlanListItem[]>> {
    let params = new HttpParams().set('includeInactive', String(includeInactive));
    if (search) params = params.set('search', search);
    return this.http.get<ApiResponse<WorkoutPlanListItem[]>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<WorkoutPlanDetail>> {
    return this.http.get<ApiResponse<WorkoutPlanDetail>>(`${this.base}/${id}`);
  }

  create(dto: {
    planName: string;
    description?: string;
    goal?: string;
    durationWeeks?: number;
    isActive: boolean;
    exercises: WorkoutPlanExerciseInput[];
  }): Observable<ApiResponse<WorkoutPlanDetail>> {
    return this.http.post<ApiResponse<WorkoutPlanDetail>>(this.base, dto);
  }

  update(id: number, dto: unknown): Observable<ApiResponse<WorkoutPlanDetail>> {
    return this.http.put<ApiResponse<WorkoutPlanDetail>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  clone(id: number, newPlanName?: string): Observable<ApiResponse<WorkoutPlanDetail>> {
    return this.http.post<ApiResponse<WorkoutPlanDetail>>(`${this.base}/${id}/clone`, { newPlanName });
  }

  assign(dto: AssignWorkoutPlanRequest): Observable<ApiResponse<MemberWorkoutView>> {
    return this.http.post<ApiResponse<MemberWorkoutView>>(`${this.base}/assign`, dto);
  }

  getMemberWorkout(memberId: number): Observable<ApiResponse<MemberWorkoutView>> {
    return this.http.get<ApiResponse<MemberWorkoutView>>(`${this.base}/members/${memberId}`);
  }

  getMyWorkout(): Observable<ApiResponse<MemberWorkoutView>> {
    return this.http.get<ApiResponse<MemberWorkoutView>>(`${this.base}/members/me`);
  }

  updateProgress(dto: UpdateWorkoutProgressRequest): Observable<ApiResponse<WorkoutPlanExercise>> {
    return this.http.post<ApiResponse<WorkoutPlanExercise>>(`${this.base}/progress`, dto);
  }

  downloadPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/pdf`, { responseType: 'blob' });
  }

  downloadExcel(id: number): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/export/excel`, { responseType: 'blob' });
  }
}
