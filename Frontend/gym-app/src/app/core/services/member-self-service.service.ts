import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response';
import {
  CreateMemberFeedbackDto,
  CreateMemberGoalDto,
  CreateMemberProgressDto,
  CreateProgressPhotoDto,
  DietComplianceSummary,
  DietTracking,
  MemberFeedback,
  MemberGoal,
  MemberProgressEntry,
  MemberProgressPhoto,
  MemberQrCode,
  MemberSelfServiceAnalytics,
  MemberSelfServiceDashboard,
  ProgressTrend,
  ReferralStats,
  UpdateGoalProgressDto,
  UpdateMemberGoalDto,
  UpsertDietTrackingDto,
  UpsertWaterIntakeDto,
  UpsertWorkoutTrackingDto,
  WaterIntake,
  WorkoutTracking,
} from '../../shared/models/member-self-service.models';

@Injectable({ providedIn: 'root' })
export class MemberSelfServiceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/member`;

  getDashboard(): Observable<ApiResponse<MemberSelfServiceDashboard>> {
    return this.http.get<ApiResponse<MemberSelfServiceDashboard>>(`${this.base}/dashboard`);
  }

  getAnalytics(): Observable<ApiResponse<MemberSelfServiceAnalytics>> {
    return this.http.get<ApiResponse<MemberSelfServiceAnalytics>>(`${this.base}/analytics`);
  }

  getGoals(status?: string): Observable<ApiResponse<MemberGoal[]>> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ApiResponse<MemberGoal[]>>(`${this.base}/goals`, { params });
  }

  createGoal(dto: CreateMemberGoalDto): Observable<ApiResponse<MemberGoal>> {
    return this.http.post<ApiResponse<MemberGoal>>(`${this.base}/goals`, dto);
  }

  updateGoal(goalId: number, dto: UpdateMemberGoalDto): Observable<ApiResponse<unknown>> {
    return this.http.put<ApiResponse<unknown>>(`${this.base}/goals/${goalId}`, dto);
  }

  updateGoalProgress(goalId: number, dto: UpdateGoalProgressDto): Observable<ApiResponse<unknown>> {
    return this.http.patch<ApiResponse<unknown>>(`${this.base}/goals/${goalId}/progress`, dto);
  }

  completeGoal(goalId: number): Observable<ApiResponse<unknown>> {
    return this.http.post<ApiResponse<unknown>>(`${this.base}/goals/${goalId}/complete`, {});
  }

  getProgressTrends(fromDate?: string, toDate?: string): Observable<ApiResponse<ProgressTrend>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<ProgressTrend>>(`${this.base}/progress`, { params });
  }

  createProgress(dto: CreateMemberProgressDto): Observable<ApiResponse<MemberProgressEntry>> {
    return this.http.post<ApiResponse<MemberProgressEntry>>(`${this.base}/progress`, dto);
  }

  getProgressPhotos(): Observable<ApiResponse<MemberProgressPhoto[]>> {
    return this.http.get<ApiResponse<MemberProgressPhoto[]>>(`${this.base}/progress/photos`);
  }

  linkProgressPhoto(dto: CreateProgressPhotoDto): Observable<ApiResponse<MemberProgressPhoto>> {
    return this.http.post<ApiResponse<MemberProgressPhoto>>(`${this.base}/progress/photos`, dto);
  }

  getWorkouts(fromDate?: string, toDate?: string): Observable<ApiResponse<WorkoutTracking[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<WorkoutTracking[]>>(`${this.base}/workouts`, { params });
  }

  upsertWorkout(dto: UpsertWorkoutTrackingDto): Observable<ApiResponse<WorkoutTracking>> {
    return this.http.post<ApiResponse<WorkoutTracking>>(`${this.base}/workouts`, dto);
  }

  getWorkoutStreak(): Observable<ApiResponse<number>> {
    return this.http.get<ApiResponse<number>>(`${this.base}/workouts/streak`);
  }

  getDiets(fromDate?: string, toDate?: string): Observable<ApiResponse<DietTracking[]>> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<ApiResponse<DietTracking[]>>(`${this.base}/diets`, { params });
  }

  getDietCompliance(): Observable<ApiResponse<DietComplianceSummary>> {
    return this.http.get<ApiResponse<DietComplianceSummary>>(`${this.base}/diets/compliance`);
  }

  upsertDiet(dto: UpsertDietTrackingDto): Observable<ApiResponse<DietTracking>> {
    return this.http.post<ApiResponse<DietTracking>>(`${this.base}/diets`, dto);
  }

  getTodayWater(): Observable<ApiResponse<WaterIntake | null>> {
    return this.http.get<ApiResponse<WaterIntake | null>>(`${this.base}/water`);
  }

  upsertWater(dto: UpsertWaterIntakeDto): Observable<ApiResponse<WaterIntake>> {
    return this.http.post<ApiResponse<WaterIntake>>(`${this.base}/water`, dto);
  }

  getReferrals(): Observable<ApiResponse<ReferralStats>> {
    return this.http.get<ApiResponse<ReferralStats>>(`${this.base}/referrals`);
  }

  getFeedback(): Observable<ApiResponse<MemberFeedback[]>> {
    return this.http.get<ApiResponse<MemberFeedback[]>>(`${this.base}/feedback`);
  }

  submitFeedback(dto: CreateMemberFeedbackDto): Observable<ApiResponse<MemberFeedback>> {
    return this.http.post<ApiResponse<MemberFeedback>>(`${this.base}/feedback`, dto);
  }

  getQrCode(): Observable<ApiResponse<MemberQrCode>> {
    return this.http.get<ApiResponse<MemberQrCode>>(`${this.base}/qr-code`);
  }

  exportProgressPdf(): Observable<Blob> {
    return this.http.get(`${this.base}/progress/export/pdf`, { responseType: 'blob' });
  }

  exportGoalsPdf(): Observable<Blob> {
    return this.http.get(`${this.base}/goals/export/pdf`, { responseType: 'blob' });
  }
}
