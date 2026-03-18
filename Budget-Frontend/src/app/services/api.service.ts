import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * API Service - Matches backend exactly:
 * - POST /api/users/register-employee → { name, email, password }
 * - POST /api/users/login → { email, password } → { token, userId, email, role }
 * - GET /api/users/verify?token=xxx → verification
 */
export interface Department {
  departmentId: number;
  departmentName: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  departmentId?: number | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  role: string;
  name: string;
}

export interface UserSummary {
  userId: string;
  name: string;
  email: string;
  role: string;
}

export interface ForgotPasswordEmailRequest {
email: string;
}
 
export interface ResetPasswordRequest {
email: string;
newPassword: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = 'http://localhost:5078';

  constructor(private http: HttpClient) { }

  registerEmployee(data: RegisterRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/users/register-employee`, data);
  }

  registerManager(data: RegisterRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/users/register-manager`, data);
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/users/login`, data);
  }

  verifyEmail(token: string): Observable<string> {
    return this.http.get(`${this.apiUrl}/users/verify`, { params: { token } }) as Observable<string>;
  }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/users/departments`);
  }

  getManagers(): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(`${this.apiUrl}/users/managers`);
  }

  /**
   * Use when displaying HTTP errors. Handles ProgressEvent (network/CORS failures)
   * so the UI shows a clear message instead of "[object ProgressEvent]".
   */
  getErrorMessage(error: any, fallback: string = 'Something went wrong.'): string {
    if (error?.status === 0 || (error?.error && error.error instanceof ProgressEvent)) {
      return 'Cannot reach server. Is the backend running at ' + this.apiUrl.replace('/api', '') + '?';
    }
    const msg = error?.error?.message ?? error?.error ?? error?.message;
    if (typeof msg === 'string') return msg;
    return fallback;
  }
  validateForgotPasswordEmail(data: ForgotPasswordEmailRequest): Observable<{ message: string }> {
return this.http.post<{ message: string }>(`${this.apiUrl}/users/forgot-password/validate-email`, data);
}
 
resetPassword(data: ResetPasswordRequest): Observable<{ message: string }> {
return this.http.post<{ message: string }>(`${this.apiUrl}/users/forgot-password/reset`, data);
}

}
