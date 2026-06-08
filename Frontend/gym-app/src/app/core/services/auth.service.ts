import { HttpClient } from '@angular/common/http';
import { Injectable, Injector, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';



import { Observable, switchMap, tap } from 'rxjs';



import { environment } from '../../../environments/environment';



import { ApiResponse } from '../../shared/models/api-response';



import { AuthUser, LoginRequest, LoginResponse, SessionPermissions } from '../models/auth.models';







const USER_KEY = 'gym_auth_user';



const MUST_CHANGE_KEY = 'gym_must_change_password';



const TOKEN_KEY = 'gym_auth_token';



const REFRESH_TOKEN_KEY = 'gym_refresh_token';







export interface ForgotPasswordResult {



  resetToken?: string;



  resetLink?: string;



}







@Injectable({ providedIn: 'root' })



export class AuthService {

  private readonly injector = inject(Injector);
  private readonly router = inject(Router);

  private get http(): HttpClient {
    return this.injector.get(HttpClient);
  }







  private readonly userSignal = signal<AuthUser | null>(this.loadUser());



  private readonly mustChangePasswordSignal = signal(



    sessionStorage.getItem(MUST_CHANGE_KEY) === 'true'



  );







  readonly user = this.userSignal.asReadonly();



  readonly mustChangePassword = this.mustChangePasswordSignal.asReadonly();







  readonly isAuthenticated = computed(() => !!this.userSignal());














  fetchCsrfToken(): Observable<ApiResponse<{ token: string }>> {



    return this.http.get<ApiResponse<{ token: string }>>(`${environment.apiUrl}/auth/csrf`);



  }







  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {

    if (environment.useCookieAuth) {

      return this.fetchCsrfToken().pipe(

        switchMap(() =>

          this.http.post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request)

        ),

        tap((response) => {

          if (response.success && response.data) {

            this.persistSession(response.data);

          }

        })

      );

    }

    return this.http

      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/login`, request)

      .pipe(

        tap((response) => {

          if (response.success && response.data) {

            this.persistSession(response.data);

          }

        })

      );

  }







  refreshToken(): Observable<ApiResponse<LoginResponse>> {



    const body = environment.useCookieAuth ? {} : { refreshToken: localStorage.getItem(REFRESH_TOKEN_KEY) };



    return this.http



      .post<ApiResponse<LoginResponse>>(`${environment.apiUrl}/auth/refresh`, body)



      .pipe(



        tap((response) => {



          if (response.success && response.data) {



            this.persistSession(response.data);



          }



        })



      );



  }







  validateToken(): Observable<ApiResponse<unknown>> {

    return this.http.get<ApiResponse<unknown>>(`${environment.apiUrl}/auth/validate`);

  }



  refreshPermissions(): Observable<ApiResponse<SessionPermissions>> {

    return this.http

      .get<ApiResponse<SessionPermissions>>(`${environment.apiUrl}/auth/session`)

      .pipe(

        tap((response) => {

          if (response.success && response.data) {

            this.updatePermissions(response.data);

          }

        })

      );

  }



  changePassword(dto: {



    currentPassword: string;



    newPassword: string;



  }): Observable<ApiResponse<unknown>> {



    return this.http.post<ApiResponse<unknown>>(`${environment.apiUrl}/auth/change-password`, {



      currentPassword: dto.currentPassword,



      newPassword: dto.newPassword,



    });



  }







  logout(): void {



    if (this.userSignal()) {



      this.http.post(`${environment.apiUrl}/auth/logout`, {}).subscribe({ error: () => undefined });



    }



    this.clearSession();



    this.router.navigate(['/auth/login']);



  }







  clearSession(): void {



    if (!environment.useCookieAuth) {



      localStorage.removeItem(TOKEN_KEY);



      localStorage.removeItem(REFRESH_TOKEN_KEY);



    }



    sessionStorage.removeItem(USER_KEY);



    sessionStorage.removeItem(MUST_CHANGE_KEY);



    this.userSignal.set(null);



    this.mustChangePasswordSignal.set(false);



  }







  getToken(): string | null {



    if (environment.useCookieAuth || !this.isAuthenticated()) {



      return null;



    }



    return localStorage.getItem(TOKEN_KEY);



  }







  hasPermission(permission: string): boolean {



    const user = this.userSignal();



    return user?.permissions.includes(permission) ?? false;



  }







  hasRole(role: string): boolean {



    const user = this.userSignal();



    return user?.roles.includes(role) ?? false;



  }







  forgotPassword(email: string): Observable<ApiResponse<ForgotPasswordResult>> {



    return this.http.post<ApiResponse<ForgotPasswordResult>>(



      `${environment.apiUrl}/auth/forgot-password`,



      { email }



    );



  }







  resetPassword(dto: {



    email: string;



    token: string;



    newPassword: string;



  }): Observable<ApiResponse<unknown>> {



    return this.http.post<ApiResponse<unknown>>(`${environment.apiUrl}/auth/reset-password`, dto);



  }







  private persistSession(data: LoginResponse): void {



    if (!environment.useCookieAuth) {



      localStorage.setItem(TOKEN_KEY, data.token);



      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);



    }



    const mustChange = !!data.mustChangePassword;



    sessionStorage.setItem(MUST_CHANGE_KEY, String(mustChange));



    this.mustChangePasswordSignal.set(mustChange);







    const user: AuthUser = {



      userId: data.userId,



      name: data.fullName,



      email: data.email,



      gymId: data.gymId,



      roles: data.roles,



      permissions: data.permissions,



      expiresAt: data.expiresAt,



      mustChangePassword: mustChange,



    };



    sessionStorage.setItem(USER_KEY, JSON.stringify(user));



    this.userSignal.set(user);



  }







  private updatePermissions(session: SessionPermissions): void {
    const current = this.userSignal();
    if (!current) return;
    const user: AuthUser = {
      ...current,
      name: session.fullName,
      email: session.email,
      gymId: session.gymId,
      roles: session.roles,
      permissions: session.permissions,
    };
    sessionStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userSignal.set(user);
  }

  private loadUser(): AuthUser | null {



    const raw = sessionStorage.getItem(USER_KEY);



    if (!raw) return null;



    try {



      const user = JSON.parse(raw) as AuthUser;



      if (sessionStorage.getItem(MUST_CHANGE_KEY) === 'true') {



        user.mustChangePassword = true;



      }



      return user;



    } catch {



      return null;



    }



  }



}

