import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  // Backend base URL (localhost)
  // This service groups all Admin report endpoints under /api/report/*
  private baseUrl = 'http://localhost:5078/api';

  constructor(private http: HttpClient) {
    // DI: HttpClient injected by Angular
  }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: token ? `Bearer ${token}` : ''
    });
  }

  getDepartmentReport(): Observable<any[]> {
    // Calls backend: GET /api/report/department
    return this.http.get<any[]>(`${this.baseUrl}/report/department`, {
      headers: this.getAuthHeaders()
    }).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getBudgetReport(): Observable<any[]> {
    // Calls backend: GET /api/report/budget
    return this.http.get<any[]>(`${this.baseUrl}/report/budget`, {
      headers: this.getAuthHeaders()
    }).pipe(
      catchError(err => throwError(() => err))
    );
  }

  getSummaryReport(): Observable<any[]> {
    // Calls backend: GET /api/report/summary
    return this.http.get<any[]>(`${this.baseUrl}/report/summary`, {
      headers: this.getAuthHeaders()
    }).pipe(
      catchError(err => throwError(() => err))
    );
  }
}

