import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../../core/services/auth';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  fullName = '';
  role = '';
  activeTab = 'All';
  searchQuery = '';
  activeMenu = 'dashboard';
  companiesTab = 'all';

  allUsers: any[] = [];
  filteredUsers: any[] = [];
  policies: any[] = [];
  companies: any[] = [];
  agentGroups: any[] = [];

  stats = {
    agents: 0,
    claimsManagers: 0,
    customers: 0,
    activePolicies: 0,
    total: 0
  };

  // Register User form (moved inline — no separate route)
  registerForm = { fullName: '', email: '', password: '', role: 'Agent' };
  registerLoading = false;
  registerError = '';
  showPassword = false;

  // Policy modal
  showPolicyModal = false;
  editingPolicy: any = null;
  policyLoading = false;
  policyForm = {
    name: '',
    healthCoverage: null as number | null,
    lifeCoverageMultiplier: null as number | null,
    maxLifeCoverageLimit: null as number | null,
    accidentCoverage: null as number | null,
    premiumPerEmployee: null as number | null,
    minEmployees: null as number | null,
    durationYears: 1,
    isPopular: false
  };

  // Success popup
  showSuccessPopup = false;
  successPopupMessage = '';

  readonly API = 'https://localhost:7173/api';

  // NOTE: Router removed — Register User is now inline, no navigation needed
  constructor(private authService: Auth, private http: HttpClient) {}

  ngOnInit() {
    this.fullName = this.authService.getFullName() || 'Admin';
    this.role = this.authService.getRole() || 'Admin';
    this.loadUsers();
    this.loadPolicies();
    this.loadCompanies();
  }

  // ── NAVIGATION ─────────────────────────────────────────────────

  setMenu(menu: string) {
    this.activeMenu = menu;
    this.registerError = '';
  }

  // ── USERS ──────────────────────────────────────────────────────

  loadUsers() {
    this.http.get<any[]>(`${this.API}/Admin/users`).subscribe({
      next: (users) => {
        this.allUsers = users;
        this.filteredUsers = users;
        this.stats.agents = users.filter(u => u.role === 'Agent').length;
        this.stats.claimsManagers = users.filter(u => u.role === 'ClaimsManager').length;
        this.stats.customers = users.filter(u => u.role === 'Customer').length;
        this.stats.total = users.length;
      },
      error: (err) => console.error('Failed to load users', err)
    });
  }

  filterByTab(tab: string) {
    this.activeTab = tab;
    this.applyFilter();
  }

  applyFilter() {
    let users = this.allUsers;
    if (this.activeTab !== 'All') {
      const roleMap: any = {
        'Agents': 'Agent',
        'Claims Managers': 'ClaimsManager',
        'Customers': 'Customer'
      };
      users = users.filter(u => u.role === roleMap[this.activeTab]);
    }
    if (this.searchQuery) {
      users = users.filter(u =>
        u.fullName.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        u.email.toLowerCase().includes(this.searchQuery.toLowerCase())
      );
    }
    this.filteredUsers = users;
  }

  // ── REGISTER USER (inline, no separate route) ──────────────────

  onRegisterUser() {
    this.registerError = '';
    if (!this.registerForm.fullName || !this.registerForm.email || !this.registerForm.password) {
      this.registerError = 'Please fill in all fields.';
      return;
    }
    this.registerLoading = true;
    this.http.post(`${this.API}/Admin/register-user`, this.registerForm).subscribe({
      next: () => {
        this.registerLoading = false;
        const registeredRole = this.registerForm.role;
        // Reset form
        this.registerForm = { fullName: '', email: '', password: '', role: 'Agent' };
        this.loadUsers();
        // Show popup then redirect to dashboard
        this.showPopupAndRedirect(`${registeredRole} registered successfully!`, 'dashboard');
      },
      error: (err) => {
        this.registerLoading = false;
        this.registerError = err.error?.message || 'Registration failed. Please try again.';
      }
    });
  }

  // ── POLICIES ────────────────────────────────────────────────────

  loadPolicies() {
    this.http.get<any[]>(`${this.API}/Policy/all`).subscribe({
      next: (data) => {
        this.policies = data;
        this.stats.activePolicies = data.filter(p => p.isActive).length;
      },
      error: (err) => console.error('Failed to load policies', err)
    });
  }

  openAddPolicy() {
    this.editingPolicy = null;
    this.policyForm = {
      name: '', healthCoverage: null, lifeCoverageMultiplier: null,
      maxLifeCoverageLimit: null, accidentCoverage: null,
      premiumPerEmployee: null, minEmployees: null,
      durationYears: 1, isPopular: false
    };
    this.showPolicyModal = true;
  }

  openEditPolicy(policy: any) {
    this.editingPolicy = policy;
    this.policyForm = {
      name: policy.name,
      healthCoverage: policy.healthCoverage,
      lifeCoverageMultiplier: policy.lifeCoverageMultiplier,
      maxLifeCoverageLimit: policy.maxLifeCoverageLimit,
      accidentCoverage: policy.accidentCoverage,
      premiumPerEmployee: policy.premiumPerEmployee,
      minEmployees: policy.minEmployees,
      durationYears: policy.durationYears,
      isPopular: policy.isPopular
    };
    this.showPolicyModal = true;
  }

  closePolicyModal() {
    this.showPolicyModal = false;
    this.editingPolicy = null;
  }

  savePolicy() {
    if (!this.policyForm.name || !this.policyForm.healthCoverage ||
        !this.policyForm.premiumPerEmployee || !this.policyForm.minEmployees) {
      alert('Please fill in all required fields.');
      return;
    }
    this.policyLoading = true;

    if (this.editingPolicy) {
      const dto = { ...this.policyForm, isActive: this.editingPolicy.isActive };
      this.http.put(`${this.API}/Policy/${this.editingPolicy.id}`, dto).subscribe({
        next: () => {
          this.policyLoading = false;
          this.closePolicyModal();
          this.loadPolicies();
          // Show popup and redirect to dashboard
          this.showPopupAndRedirect('Policy updated successfully!', 'dashboard');
        },
        error: (err) => {
          this.policyLoading = false;
          alert(err.error?.message || 'Failed to update policy.');
        }
      });
    } else {
      this.http.post(`${this.API}/Policy`, this.policyForm).subscribe({
        next: () => {
          this.policyLoading = false;
          this.closePolicyModal();
          this.loadPolicies();
          // Show popup and redirect to dashboard
          this.showPopupAndRedirect('Policy created successfully!', 'dashboard');
        },
        error: (err) => {
          this.policyLoading = false;
          alert(err.error?.message || 'Failed to create policy.');
        }
      });
    }
  }

  deactivatePolicy(policy: any) {
    if (!confirm(`Deactivate "${policy.name}"?`)) return;
    this.http.delete(`${this.API}/Policy/${policy.id}`).subscribe({
      next: () => { this.loadPolicies(); this.showPopup(`"${policy.name}" deactivated.`); },
      error: () => alert('Failed to deactivate.')
    });
  }

  activatePolicy(policy: any) {
    const dto = { ...policy, isActive: true };
    this.http.put(`${this.API}/Policy/${policy.id}`, dto).subscribe({
      next: () => { this.loadPolicies(); this.showPopup(`"${policy.name}" activated.`); },
      error: () => alert('Failed to activate.')
    });
  }

  // ── COMPANIES & AGENTS ─────────────────────────────────────────

  loadCompanies() {
    this.http.get<any[]>(`${this.API}/Company/all`).subscribe({
      next: (data) => { this.companies = data; },
      error: (err) => console.error('Failed to load companies', err)
    });
    this.http.get<any[]>(`${this.API}/Company/by-agent`).subscribe({
      next: (data) => { this.agentGroups = data; },
      error: (err) => console.error('Failed to load agent groups', err)
    });
  }

  // ── SUCCESS POPUP ──────────────────────────────────────────────

  showPopup(msg: string) {
    this.successPopupMessage = msg;
    this.showSuccessPopup = true;
    setTimeout(() => { this.showSuccessPopup = false; }, 3000);
  }

  // Show popup then navigate to a menu after 2 seconds
  showPopupAndRedirect(msg: string, redirectTo: string) {
    this.successPopupMessage = msg;
    this.showSuccessPopup = true;
    setTimeout(() => {
      this.showSuccessPopup = false;
      this.activeMenu = redirectTo;
    }, 2000);
  }

  // ── HELPERS ────────────────────────────────────────────────────

  getInitials(name: string): string {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }

  getAvatarColor(role: string): string {
    switch (role) {
      case 'Agent': return 'bg-blue-600';
      case 'ClaimsManager': return 'bg-green-600';
      case 'Customer': return 'bg-orange-500';
      default: return 'bg-gray-500';
    }
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Agent': return 'bg-blue-100 text-blue-700';
      case 'ClaimsManager': return 'bg-green-100 text-green-700';
      case 'Customer': return 'bg-orange-100 text-orange-700';
      default: return 'bg-gray-100 text-gray-700';
    }
  }

  getRoleDisplay(role: string): string {
    switch (role) {
      case 'ClaimsManager': return 'Claims Manager';
      default: return role;
    }
  }

  formatCurrency(val: number | null | undefined): string {
    if (val == null) return 'N/A';
    return '₹' + val.toLocaleString('en-IN');
  }

  logout() {
    this.authService.logout();
  }
}