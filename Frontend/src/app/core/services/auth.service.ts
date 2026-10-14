import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, UserDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = `${environment.apiUrl}/auth`;

  private readonly TOKEN_KEY = 'taskmgr_token';
  private readonly REFRESH_TOKEN_KEY = 'taskmgr_refresh_token';
  private readonly USER_KEY = 'taskmgr_user';

  currentUserToken = signal<string | null>(this.getAccessToken());
  currentUser = signal<UserDto | null>(this.getStoredUser());

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getStoredUser(): UserDto | null {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  setSession(authResult: AuthResponseDto): void {
    localStorage.setItem(this.TOKEN_KEY, authResult.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResult.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(authResult.user));

    this.currentUserToken.set(authResult.token);
    this.currentUser.set(authResult.user);
  }

  clearSession(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);

    this.currentUserToken.set(null);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponseDto> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.clearSession();
      return throwError(() => new Error('No refresh token available.'));
    }

    return this.http
      .post<AuthResponseDto>(`${this.baseUrl}/refresh-token`, { refreshToken })
      .pipe(
        tap((res) => this.setSession(res)),
        catchError((err) => {
          this.clearSession();
          return throwError(() => err);
        })
      );
  }
}