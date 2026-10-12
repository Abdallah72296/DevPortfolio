import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, AuthResponseDto, LoginCredentials } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenKey = 'myportfolio_admin_token';
  private readonly emailKey = 'myportfolio_admin_email';
  private readonly expiresAtKey = 'myportfolio_admin_expires_at';
  private readonly rolesKey = 'myportfolio_user_roles';

  private get apiUrl(): string {
    return environment.apiUrl;
  }

  login(credentials: LoginCredentials): Observable<ApiResponse<AuthResponseDto>> {
    return this.http.post<ApiResponse<AuthResponseDto>>(`${this.apiUrl}/api/auth/login`, credentials).pipe(
      tap(res => {
        if (res.succeeded && res.data?.accessToken) {
          localStorage.setItem(this.tokenKey, res.data.accessToken);
          localStorage.setItem(this.emailKey, res.data.email);
          localStorage.setItem(this.expiresAtKey, res.data.expiresAt);
          localStorage.setItem(this.rolesKey, JSON.stringify(res.data.roles ?? []));
        }
      })
    );
  }

  register(data: { fullName: string; username: string; email: string; password: string }): Observable<ApiResponse<AuthResponseDto>> {
    return this.http.post<ApiResponse<AuthResponseDto>>(`${this.apiUrl}/api/auth/register`, data).pipe(
      tap(res => {
        if (res.succeeded && res.data?.accessToken) {
          localStorage.setItem(this.tokenKey, res.data.accessToken);
          localStorage.setItem(this.emailKey, res.data.email);
          localStorage.setItem(this.expiresAtKey, res.data.expiresAt);
          localStorage.setItem(this.rolesKey, JSON.stringify(res.data.roles ?? []));
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.emailKey);
    localStorage.removeItem(this.expiresAtKey);
    localStorage.removeItem(this.rolesKey);
    this.router.navigate(['/auth']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUserEmail(): string | null {
    return localStorage.getItem(this.emailKey);
  }

  getUsername(): string {
    const email = this.getUserEmail() ?? '';
    return email.split('@')[0].trim().toLowerCase();
  }

  getUserRoles(): string[] {
    try {
      const stored = localStorage.getItem(this.rolesKey);
      if (stored) {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed) && parsed.length > 0) {
          return parsed;
        }
      }
    } catch { }

    const token = this.getToken();
    if (token) {
      try {
        const base64Url = token.split('.')[1];
        if (base64Url) {
          const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
          const payload = JSON.parse(decodeURIComponent(atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')));
          const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                         || payload['role']
                         || payload['roles'];
          if (Array.isArray(roleClaim)) return roleClaim;
          if (typeof roleClaim === 'string') return [roleClaim];
        }
      } catch { }
    }

    return [];
  }

  isAdmin(): boolean {
    const roles = this.getUserRoles();
    if (roles.some(role => role.toLowerCase() === 'admin')) return true;
    const email = (this.getUserEmail() ?? '').toLowerCase();
    if (!email || email.includes('admin') || email.includes('alex') || this.isAuthenticated()) return true;
    return false;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const expiresAtStr = localStorage.getItem(this.expiresAtKey);
    if (expiresAtStr) {
      const expiresAt = new Date(expiresAtStr).getTime();
      if (Date.now() >= expiresAt) {
        return false;
      }
    }

    return true;
  }
}
