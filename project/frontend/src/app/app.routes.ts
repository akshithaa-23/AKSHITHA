import { Routes } from '@angular/router';
import { Landing } from './pages/landing/landing';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { authGuard } from './core/guards/auth-guard';


export const routes: Routes = [
  { path: '', component: Landing },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  
  // Admin routes
  {
    path: 'admin',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/admin/dashboard/dashboard').then(m => m.Dashboard) },
      { path: 'register-user', loadComponent: () => import('./pages/admin/register-user/register-user').then(m => m.RegisterUser) },
    ]
  },

  // Agent routes
  {
    path: 'agent',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/agent/dashboard/dashboard').then(m => m.Dashboard) },
    ]
  },

  // Customer routes — single dashboard component handles all sections internally
  {
    path: 'customer',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/customer/dashboard/dashboard').then(m => m.Dashboard) },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ]
  },

  // Claims Manager routes
  {
    path: 'claims-manager',
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', loadComponent: () => import('./pages/claims-manager/dashboard/dashboard').then(m => m.Dashboard) },
    ]
  },

  // Fallback
  { path: '**', redirectTo: '' }
];