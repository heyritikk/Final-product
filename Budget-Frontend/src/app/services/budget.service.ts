import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Budget {
  budgetId: number;
  title: string;
  amountAllocated: number;
  departmentId: number;
  managerId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BudgetService {

  // Backend runs on http://localhost:5078 (see launchSettings)
  baseUrl = 'http://localhost:5078/api';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: token ? `Bearer ${token}` : ''
    });
  }

  getAllBudgets(): Observable<any> {
    return this.http.get<Budget[]>(this.baseUrl + '/budget', {
      headers: this.getAuthHeaders()
    });
  }

  createBudget(data: Partial<Budget>): Observable<any> {
    return this.http.post(this.baseUrl + '/budget/create', data, {
      headers: this.getAuthHeaders()
    });
  }

  updateBudget(id: number, data: Partial<Budget>): Observable<any> {
    return this.http.patch(this.baseUrl + '/budget/update/' + id, data, {
      headers: this.getAuthHeaders()
    });
  }

  deleteBudget(budgetId:number): Observable<any>  {
    return this.http.patch(this.baseUrl + '/budget/delete/' + budgetId, {}, {
      headers: this.getAuthHeaders()
    });
  }
}
