import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap, finalize } from 'rxjs';
import { LoginDto, RegisterDto, LoginResponse } from '../models';
import { environment } from '../../../environments/environment';

export type { LoginDto, RegisterDto };

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY  = 'auth_token';
  private readonly EXPIRY_KEY = 'auth_token_expiry';
  private readonly base = `${environment.apiUrl}/auth`;

  private readonly _token = signal<string | null>(this.loadToken());

  readonly isLoggedIn = computed(() => !!this._token());
  readonly token = this._token.asReadonly();

  readonly currentUserEmail = computed(() => {
    const token = this._token();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (payload.sub ?? null) as string | null;
    } catch {
      return null;
    }
  });

  constructor(private http: HttpClient, private router: Router) {}

  login(dto: LoginDto) {
    return this.http.post<LoginResponse>(`${this.base}/Login`, dto).pipe(
      tap(res => this.storeToken(res.access_token, res.expires_in))
    );
  }

  register(dto: RegisterDto) {
    return this.http.post<void>(`${this.base}/Register`, dto);
  }

  logout() {
    this.http.post<void>(`${this.base}/Logout`, {}).pipe(
      finalize(() => this.clearLocal())
    ).subscribe({ error: () => {} });
  }

  clearLocal() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.EXPIRY_KEY);
    this._token.set(null);
    this.router.navigate(['/login']);
  }

  private storeToken(token: string, expiresInSeconds: number) {
    const expiresAt = Date.now() + expiresInSeconds * 1000;
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.EXPIRY_KEY, String(expiresAt));
    this._token.set(token);
  }

  private loadToken(): string | null {
    const token  = localStorage.getItem(this.TOKEN_KEY);
    const expiry = localStorage.getItem(this.EXPIRY_KEY);
    if (!token || !expiry) return null;
    if (Date.now() > Number(expiry)) {
      localStorage.removeItem(this.TOKEN_KEY);
      localStorage.removeItem(this.EXPIRY_KEY);
      return null;
    }
    return token;
  }
}
