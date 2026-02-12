import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, of } from 'rxjs';
import { SignalRService } from './signalr.service';

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  role: string;
}
export interface CurrentUser {
  userName: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private token = signal<string | null>(localStorage.getItem('token'));
  private role = signal<string | null>(localStorage.getItem('role'));
  private signalR = inject(SignalRService);
  
  currentUser = computed<CurrentUser | null>(() => {
    const u = localStorage.getItem('userName');
    const r = this.role();
    return u && r ? { userName: u, role: r } : null;
  });
  isAdmin$ = () => of(this.role() === 'Admin');

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Auto-connect SignalR if already logged in
    if (this.token()) {
      this.signalR.startConnection(this.token()!).catch(console.error);
    }
  }

  login(userName: string, password: string): Observable<LoginResponse> {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    return this.http.post<LoginResponse>(`${apiUrl}/api/v1/auth/login`, { userName, password }).pipe(
      tap((res) => {
        localStorage.setItem('token', res.accessToken);
        if (res.refreshToken) localStorage.setItem('refreshToken', res.refreshToken);
        localStorage.setItem('role', res.role);
        localStorage.setItem('userName', userName);
        this.token.set(res.accessToken);
        this.role.set(res.role);
        
        // Start SignalR connection
        this.signalR.startConnection(res.accessToken).catch(console.error);
      })
    );
  }

  logout(): void {
    this.signalR.stopConnection().catch(console.error);
    localStorage.clear();
    this.token.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.token();
  }

  isAuthenticated(): boolean {
    return !!this.token();
  }
}
