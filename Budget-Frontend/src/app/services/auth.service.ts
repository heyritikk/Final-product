import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  // Local storage is used to persist session across refreshes.
  // (token/email/role/userId are written after login and read by API services for Authorization headers)
  private storage = localStorage;

  getToken(): string | null {
    return this.storage.getItem('token');
  }

  getRole(): string | null {
    const storedRole = this.storage.getItem('role');
    if (storedRole) {
      return storedRole;
    }

    // Fallback: try to read role from JWT if not explicitly stored
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const payloadPart = token.split('.')[1];
      const decoded = JSON.parse(atob(payloadPart));
      return decoded['role'] || decoded['Role'] || null;
    } catch {
      return null;
    }
  }

  getEmail(): string | null {
    return this.storage.getItem('email');
  }

  getUserId(): string | null {
    return this.storage.getItem('userId');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  setSession(token: string, email: string, role: string, userId: number, name: string ) {
    // Called after successful login API response.
    // Stores JWT + user info for later API calls and role-based routing/guards.
    localStorage.setItem('token', token);
    localStorage.setItem('email', email);
    localStorage.setItem('role', role);
    
    localStorage.setItem('userId', userId.toString());
    
    
    localStorage.setItem('name', name);
    if(userId)
      {
        this.storage.setItem('userId', userId.toString());
      } // Store name as well for display purposes
  }

  clearSession() {
    this.storage.removeItem('token');
    this.storage.removeItem('email');
    this.storage.removeItem('role');
    this.storage.removeItem('userId');
  }

  logout() {
    this.clearSession();
  }
}

