import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Expense {
  id: number;
  //title: string;
  amount: number;
  budgetId: number;
  description?: string;
  employeeName?: string;
  employeeId?: string | number;
  managerEmail?: string;
  managerId?: string | number;
  status: number| string; // 0 = Pending, 1 = Approved, 2 = Rejected
}

@Injectable({
  providedIn: 'root'
}) 
export class ExpenseService {

  // Backend base URL (localhost is where the ASP.NET API runs during development)
  // All expense-related API calls in this service are built from this baseUrl.
  private baseUrl = 'http://localhost:5078/api';

  constructor(private http: HttpClient) {
    // DI: Angular injects HttpClient here (provided by HttpClientModule).
  }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: token ? `Bearer ${token}` : ''
    });
  }

  getAllExpenses(): Observable<Expense[]> {
    // Calls backend: GET /api/expense
    return this.http.get<Expense[]>(`${this.baseUrl}/expense`, {
      headers: this.getAuthHeaders()
    });
  }

  createExpense(data: Partial<Expense>): Observable<any> {
    // Calls backend: POST /api/expense/create
    // After calling this, the component typically reloads the expense list / resets the form.
    return this.http.post(`${this.baseUrl}/expense/create`, data, {
      headers: this.getAuthHeaders()
    });
  }

  updateExpense(id: number, data: Partial<Expense>): Observable<any> {
    // Calls backend: PATCH /api/expense/update/{id}
    return this.http.patch(`${this.baseUrl}/expense/update/${id}`, data, {
      headers: this.getAuthHeaders()
    });
  }

  updateExpenseStatus(id: number, status: number): Observable<any> {
    return this.updateExpense(id, { status });
  }

  approveOrRejectExpense(
    id: number,
    action: 'Approve' | 'Reject',
    comment: string = ''
  ): Observable<any> {
    // Calls backend: PATCH /api/expense/approve-reject/{id}
    return this.http.patch(
      `${this.baseUrl}/expense/approve-reject/${id}`,
      { action, comment },
      { headers: this.getAuthHeaders() }
    );
  }

  deleteExpense(id: number): Observable<any> {
    // Calls backend: PATCH /api/expense/delete/{id} (soft delete)
    return this.http.patch(`${this.baseUrl}/expense/delete/${id}`, {}, {
      headers: this.getAuthHeaders()
    });
  }
}
