import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  constructor(private router: Router) { }

  goToLogin() { this.router.navigate(['/login']); }
  goToRegister() {
    console.log('going to register');
    this.router.navigate(['/register']);
  }

}
