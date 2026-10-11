import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, UserDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly TOKEN_KEY = 'tm_access_token';
  private readonly REFRESH_KEY = 'tm_refresh_token';
  private readonly USER_KEY = 'tm_user';

  private currentUserSignal = signal<UserDto | null>(this.loadStoredUser());
  readonly currentUser = computed(() => this.currentUserSignal());
  readonly isAuthenticated = computed(() => !!this.getToken());

  register(payload: { fullName: string; email: string; password: string }): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${environment.apiUrl}/auth/register`, payload).pipe(
      tap((res) => this.setSession(res))
    );
  }

  login(payload: { email: string; password: string }): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${environment.apiUrl}/auth/login`, payload).pipe(
      tap((res) => this.setSession(res))
    );
  }

  refreshToken(): Observable<AuthResponseDto> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<AuthResponseDto>(`${environment.apiUrl}/auth/refresh-token`, { refreshToken }).pipe(
      tap((res) => this.setSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSignal.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_KEY);
  }

  private setSession(auth: AuthResponseDto): void {
    localStorage.setItem(this.TOKEN_KEY, auth.token);
    localStorage.setItem(this.REFRESH_KEY, auth.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(auth.user));
    this.currentUserSignal.set(auth.user);
  }

  private loadStoredUser(): UserDto | null {
    const data = localStorage.getItem(this.USER_KEY);
    return data ? JSON.parse(data) : null;
  }
}