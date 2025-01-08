import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ForgotPasswordRequest, LoginRequest, LoginWithTwoFactorRequest, RegisterRequest, ResetPasswordRequest } from '../models/identity.model';
import { catchError, map, of } from 'rxjs';

const ACCOUNTS_API = `${environment.apiUrl}/api/v1/accounts`;

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(
    private http: HttpClient
  ) { }

  register(request: RegisterRequest) {
    return this.http.post<{
      id: string,
      email: string
    }>(`${ACCOUNTS_API}/register`, request);
  }

  isAuthorized() {
    return this.http.get<void>(`${ACCOUNTS_API}/isAuthorized`, {
      observe: 'response'
    }).pipe(map(() => true), catchError(() => of(false)));
  }

  login(request: LoginRequest) {
    return this.http.post(`${ACCOUNTS_API}/login`, request);
  }

  logout() {
    return this.http.post(`${ACCOUNTS_API}/logout`, null);
  }

  externalLogin(provider: string, returnUrl: string = '') {
    location.href = `${environment.apiUrl}/challenge/externalLogin?provider=${provider}&returnUrl=${returnUrl}`;
  }

  externalCallback() {
    return this.http.get(`${ACCOUNTS_API}/externalCallback`);
  }

  confirmEmail(request: { email: string, code: string }) {
    return this.http.post(`${ACCOUNTS_API}/confirmEmail`, request);
  }

  resendEmailConfirmation(email: string) {
    return this.http.post(`${ACCOUNTS_API}/resendEmailConfirmation`, {
      email
    });
  }

  loginWith2fa(request: LoginWithTwoFactorRequest) {
    return this.http.post(`${ACCOUNTS_API}/loginWith2fa`, request);
  }

  forgotPassword(request: ForgotPasswordRequest) {
    return this.http.post(`${ACCOUNTS_API}/forgotPassword`, request);
  }

  resetPassword(request: ResetPasswordRequest) {
    return this.http.post(`${ACCOUNTS_API}/resetPassword`, request);
  }
}
