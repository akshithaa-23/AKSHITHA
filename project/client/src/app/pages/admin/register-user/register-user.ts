import { CommonModule } from '@angular/common';
import { Component, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../../core/services/auth';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register-user',
  standalone : true,
  imports: [CommonModule,FormsModule],
  templateUrl: './register-user.html',
  styleUrl: './register-user.css',
})
export class RegisterUser {
  fullName = '';
  email = '';
  password = '';
  role = 'Agent';
  showPassword = false;
  isLoading = false;
  successMessage = '';
  errorMessage = '';

  fullName2 = '';

  constructor(
    private authService: Auth,
    private router: Router,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  get initials(): string {
    return this.authService.getFullName()?.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || 'A';
  }

  get adminName(): string {
    return this.authService.getFullName() || 'Admin';
  }

  get adminRole(): string {
    return this.authService.getRole() || 'Admin';
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.fullName || !this.email || !this.password) {
      this.errorMessage = 'Please fill in all fields.';
      return;
    }

    this.isLoading = true;

    this.http.post('https://localhost:7173/api/Admin/register-user', {
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      role: this.role
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = `${this.role} registered successfully!`;
        this.fullName = '';
        this.email = '';
        this.password = '';
        this.role = 'Agent';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  goToDashboard() {
    this.router.navigate(['/admin/dashboard']);
  }

  logout() {
    this.authService.logout();
  }
}