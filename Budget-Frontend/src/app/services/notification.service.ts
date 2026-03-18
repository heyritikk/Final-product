// import { Injectable } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
// import { Observable } from 'rxjs';
// import { HttpHeaders } from '@angular/common/http';
 
// export interface Notification {
//   id: number;
//   message: string;
//   userId: number;
//   createdAt: Date;
// }
 
// @Injectable({
//   providedIn: 'root'
// })
// export class NotificationService {
 
//   private baseUrl = 'https://localhost:7268/api/notifications';
 
//   constructor(private http: HttpClient) {}
 
//   getNotifications(userId: number): Observable<Notification[]> {
//     const token = localStorage.getItem('token');
//     const headers = new HttpHeaders({
//       Authorization: token ? `Bearer ${token}` : ''
//     });
//     return this.http.get<Notification[]>(`${this.baseUrl}`, { headers });
//   }
// }
 import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AppNotification {
 notificationId: number;
 toUserId: number;
 type: string | number;
 message: string;
 status: 'Read' | 'Unread' | string | number;
 createdDate: string;
}

export interface NotificationsResponse {
 status: number;
 unreadCount: number;
 data: AppNotification[];
}

@Injectable({
 providedIn: 'root'
})
export class NotificationService {
 // Backend notifications endpoint (localhost)
 private baseUrl = 'http://localhost:5078/api/notifications';

 constructor(private http: HttpClient) {
  // DI: HttpClient injected by Angular
 }

 private getAuthHeaders(): HttpHeaders {
 const token = localStorage.getItem('token');
 return new HttpHeaders({
 Authorization: token ? `Bearer ${token}` : ''
 });
 }

 getNotifications(): Observable<NotificationsResponse> {
 // Calls backend: GET /api/notifications
 // Components typically use this to render list + unreadCount.
 return this.http.get<NotificationsResponse>(this.baseUrl, {
 headers: this.getAuthHeaders()
 });
 }

 markAsRead(notificationId: number): Observable<{ status: number; message: string }> {
 // Calls backend: PATCH /api/notifications/read/{notificationId}
 return this.http.patch<{ status: number; message: string }>(
 `${this.baseUrl}/read/${notificationId}`,
 {},
 { headers: this.getAuthHeaders() }
 );
 }
}