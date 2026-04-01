import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
}

export interface RefreshRequest {
  token: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:5001/api/v1.0/auth';

  constructor(private http: HttpClient) { }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.token);
        localStorage.setItem('auth_refresh', response.refreshToken);
        localStorage.setItem('auth_expires', response.expiresAt);
      })
    );
  }

  refreshToken(): Observable<LoginResponse> {
    const token = this.getToken();
    const refreshToken = localStorage.getItem('auth_refresh');
    if (!token || !refreshToken) {
      throw new Error('No token or refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, { token, refreshToken }).pipe(
      tap(response => {
        localStorage.setItem('auth_token', response.token);
        localStorage.setItem('auth_refresh', response.refreshToken);
        localStorage.setItem('auth_expires', response.expiresAt);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_refresh');
    localStorage.removeItem('auth_expires');
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  getUserRoles(): string[] {
    const token = this.getToken();
    if (!token) return [];

    try {
      const payloadBase64 = token.split('.')[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);

      const roleClaim = payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (!roleClaim) return [];

      if (Array.isArray(roleClaim)) {
        return roleClaim;
      }

      if (typeof roleClaim === 'string') {
        return roleClaim.split(',').map((x: string) => x.trim());
      }

      return [];
    } catch {
      return [];
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token;
  }
}
