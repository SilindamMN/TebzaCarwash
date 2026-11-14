import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

interface RegisterRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
}

interface AuthUser {
  id: number;
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
}

interface LoginResponse {
  newToken: string;
  userInfo: AuthUser;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authUrl = 'http://localhost:5163/api/auth';
  readonly currentUser = signal<AuthUser | null>(null);
  private token: string | null = null;

  constructor(private readonly http: HttpClient) {}

  get accessToken(): string | null {
    return this.token;
  }

  async register(details: RegisterRequest): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.authUrl}/register`, details, { responseType: 'text' }),
    );
  }

  async login(userName: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>(`${this.authUrl}/login`, { userName, password }),
    );
    this.token = response.newToken;
    this.currentUser.set(response.userInfo);
  }

  signOut(): void {
    this.token = null;
    this.currentUser.set(null);
  }

  getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) {
        return 'Unable to reach the server. Check your connection and try again.';
      }
      if (typeof error.error === 'string' && error.error.trim()) {
        return error.error;
      }
      if (typeof error.error?.message === 'string') {
        return error.error.message;
      }
      return error.status === 401
        ? 'Those login details do not match an account. Please try again.'
        : 'Something went wrong. Please try again.';
    }

    return 'Something went wrong. Please try again.';
  }
}
