import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private apiUrl = 'https://localhost:7173/api';

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string) {
    return this.http.post<any>(`${this.apiUrl}/auth/login`, { email, password });
  }

  registerCustomer(fullName: string, email: string, password: string) {
    return this.http.post<any>(`${this.apiUrl}/auth/register-customer`, { fullName, email, password });
  }

  saveToken(token: string, role: string, fullName: string) {
    localStorage.setItem('token', token);
    localStorage.setItem('role', role);
    localStorage.setItem('fullName', fullName);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRole(): string | null {
    return localStorage.getItem('role');
  }

  getFullName(): string | null {
    return localStorage.getItem('fullName');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }

  navigateToDashboard(role: string) {
    switch (role) {
      case 'Admin': this.router.navigate(['/admin/dashboard']); break;
      case 'Agent': this.router.navigate(['/agent/dashboard']); break;
      case 'ClaimsManager': this.router.navigate(['/claims-manager/dashboard']); break;
      case 'Customer': this.router.navigate(['/customer/dashboard']); break;
      default: this.router.navigate(['/']);
    }
  }
}
