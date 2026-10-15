import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, AuthResponseDto, LoginCredentials, RegistrationResponseDto } from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenKey = 'myportfolio_admin_token';
  private readonly emailKey = 'myportfolio_admin_email';
  private readonly expiresAtKey = 'myportfolio_admin_expires_at';
  private sessionRoles: string[] = [];

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
          this.sessionRoles = res.data.roles ?? [];
        }
      })
    );
  }

  register(data: { fullName: string; username: string; email: string; password: string }): Observable<ApiResponse<RegistrationResponseDto>> {
    return this.http.post<ApiResponse<RegistrationResponseDto>>(`${this.apiUrl}/api/auth/register`, data);
  }

  confirmEmail(email: string, token: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/api/auth/confirm-email`, { email, token });
  }

  confirmEmailOtp(email: string, code: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/api/auth/confirm-email-otp`, { email, code });
  }

  resendConfirmationEmail(email: string): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.apiUrl}/api/auth/resend-confirmation`, { email });
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.emailKey);
    localStorage.removeItem(this.expiresAtKey);
    this.sessionRoles = [];
    this.router.navigate(['/auth']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUserEmail(): string | null {
    return localStorage.getItem(this.emailKey);
  }

  getUsername(): string {
    const token = this.getToken();
    if (token) {
      try {
        const payloadPart = token.split('.')[1];
        if (payloadPart) {
          const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
          const payload = JSON.parse(decodeURIComponent(atob(base64.padEnd(Math.ceil(base64.length / 4) * 4, '='))
            .split('').map(character => `%${character.charCodeAt(0).toString(16).padStart(2, '0')}`).join('')));
          const username = payload['unique_name']
            || payload['name']
            || payload['preferred_username']
            || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
          if (typeof username === 'string' && username.trim()) return username.trim().toLowerCase();
        }
      } catch { }
    }

    return (this.getUserEmail() ?? '').split('@')[0].trim().toLowerCase();
  }

  getUserRoles(): string[] {
    if (this.sessionRoles.length) return [...this.sessionRoles];

    const token = this.getToken();
    if (token) {
      try {
        const base64Url = token.split('.')[1];
        if (base64Url) {
          const unpaddedBase64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
          const base64 = unpaddedBase64.padEnd(Math.ceil(unpaddedBase64.length / 4) * 4, '=');
          const payload = JSON.parse(decodeURIComponent(atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')));
          const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                         || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role']
                         || payload['role']
                         || payload['roles'];
          if (Array.isArray(roleClaim)) return roleClaim;
          if (typeof roleClaim === 'string') return [roleClaim];
          const mappedRoleClaims = Object.entries(payload)
            .filter(([claim]) => /(?:\/|\.)role$/i.test(claim))
            .flatMap(([, value]) => Array.isArray(value) ? value : [value])
            .filter((value): value is string => typeof value === 'string');
          if (mappedRoleClaims.length) return mappedRoleClaims;
        }
      } catch { }
    }

    return [];
  }

  isAdmin(): boolean {
    const roles = this.getUserRoles();
    return this.isAuthenticated() && roles.some(role => role.toLowerCase() === 'admin');
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
