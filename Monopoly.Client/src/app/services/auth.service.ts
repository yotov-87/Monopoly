import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  token: string;
  username: string;
  expiresAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  register(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { username, password });
  }

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password });
  }

  saveToken(token: string): void {
    localStorage.setItem('authToken', token);
  }

  saveUsername(username: string): void {
    localStorage.setItem('username', username);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getUsername(): string | null {
    return localStorage.getItem('username');
  }

  removeToken(): void {
    localStorage.removeItem('authToken');
  }

  removeUsername(): void {
    localStorage.removeItem('username');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    this.removeToken();
    this.removeUsername();
  }
}
