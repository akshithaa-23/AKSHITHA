import { Component, OnInit, signal, computed, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Auth } from '../../../core/services/auth';

interface Claim {
  id: number;
  employeeName: string;
  companyName: string;
  claimType: string;
  requestedAmount?: number;
  claimAmount?: number;
  amount?: number;
  status: string;
  description: string;
  filedDate?: string;
  createdAt?: string;
  documents?: any[];
  nomineeDetails?: string;
  accidentType?: string;
  accidentPercentage?: number;
  salary?: number;
  department?: string;
  displayId?: string;
  avatarColor?: string;
  avatarInitials?: string;
  displayAmount?: string;
  displayDate?: string;
  statusClass?: string;
}

@Component({
  selector: 'app-claims-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  activeSection = signal<'dashboard' | 'all-claims'>('dashboard');
  filterStatus = signal<'ALL' | 'Pending' | 'Approved' | 'Rejected'>('ALL');

  fullName = signal('');
  initials = signal('');
  readonly apiUrl = 'https://localhost:7173/api';

  allClaims = signal<Claim[]>([]);
  loading = signal(false);

  formattedClaims = computed(() => {
    return this.allClaims().map(c => ({
      ...c,
      displayId: this.formatClaimId(c.id),
      avatarColor: this.getAvatarColor(c.employeeName),
      avatarInitials: this.getInitials(c.employeeName),
      displayAmount: this.formatCurrency(this.getAmount(c)),
      displayDate: this.formatDate(c.filedDate || c.createdAt),
      statusClass: this.getStatusClass(c.status)
    }));
  });

  // Derived stats
  totalClaims = computed(() => this.formattedClaims().length);
  pendingClaims = computed(() => this.formattedClaims().filter(c => c.status === 'Pending').length);
  approvedClaims = computed(() => this.formattedClaims().filter(c => c.status === 'Approved').length);
  rejectedClaims = computed(() => this.formattedClaims().filter(c => c.status === 'Rejected').length);

  // Lists
  recentClaims = computed(() => {
    return [...this.formattedClaims()].sort((a, b) => {
      const dateA = new Date(a.filedDate || a.createdAt || 0).getTime();
      const dateB = new Date(b.filedDate || b.createdAt || 0).getTime();
      return dateB - dateA;
    });
  });

  filteredClaims = computed(() => {
    let list = this.recentClaims();
    const filter = this.filterStatus();
    if (filter !== 'ALL') {
      list = list.filter(c => c.status === filter);
    }
    return list;
  });

  // Modal State
  selectedClaim = signal<Claim | null>(null);
  managerMessage = signal('');
  submitting = signal(false);
  validationError = signal('');

  // Toast State
  toastMessage = signal('');
  toastTitle = signal('');
  toastType = signal<'success' | 'error'>('success');
  showToast = signal(false);

  constructor(
    private http: HttpClient,
    private authService: Auth,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.fullName.set(this.authService.getFullName() || 'Claims Manager');
    const nameParts = this.fullName().split(' ');
    this.initials.set(nameParts.map(w => w[0]).join('').toUpperCase().slice(0, 2));
    this.loadClaims();
  }

  loadClaims() {
    this.loading.set(true);
    this.http.get<Claim[]>(`${this.apiUrl}/Claim/manager`).subscribe({
      next: (data) => {
        this.allClaims.set(data || []);
      },
      error: (err) => {
        console.error('Failed to fetch claims', err);
        this.allClaims.set([]);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }

  navigate(section: 'dashboard' | 'all-claims') {
    this.activeSection.set(section);
  }

  setFilter(status: string) {
    this.filterStatus.set(status as 'ALL' | 'Pending' | 'Approved' | 'Rejected');
  }

  // --- Modal Logic ---
  openReview(claim: Claim) {
    this.selectedClaim.set(claim);
    this.managerMessage.set('');
    this.validationError.set('');
  }

  closeModal() {
    if (this.submitting()) return;
    this.selectedClaim.set(null);
  }

  processClaim(decision: 'Approved' | 'Rejected') {
    const claim = this.selectedClaim();
    if (!claim) return;

    if (!this.managerMessage().trim()) {
      this.validationError.set('Please add a note before deciding');
      return;
    }
    this.validationError.set('');
    this.submitting.set(true);

    this.http.put(`${this.apiUrl}/Claim/${claim.id}/process`, {
      decision: decision,
      note: this.managerMessage()
    }).subscribe({
      next: () => {
        // Optimistic update
        this.allClaims.update(claims =>
          claims.map(c => c.id === claim.id ? { ...c, status: decision } : c)
        );

        this.submitting.set(false);
        this.closeModal();

        if (decision === 'Approved') {
          this.triggerToast('Claim Approved', `[${this.formatClaimId(claim.id)}] has been approved. Payout processed.`, 'success');
        } else {
          this.triggerToast('Claim Rejected', `[${this.formatClaimId(claim.id)}] has been rejected.`, 'error');
        }

        setTimeout(() => {
          this.navigate('dashboard');
        }, 1500);
      },
      error: () => {
        this.submitting.set(false);
        this.triggerToast('Error', 'Failed to process claim. Please try again.', 'error');
      }
    });
  }

  // --- Helpers ---
  formatClaimId(id: number): string {
    return `CLM-${id.toString().padStart(3, '0')}`;
  }

  getAmount(claim: Claim): number {
    return claim.requestedAmount || claim.claimAmount || claim.amount || 0;
  }

  formatCurrency(val: number): string {
    if (!val && val !== 0) return '₹0';
    return '₹' + val.toLocaleString('en-IN');
  }

  formatDate(d?: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' });
  }

  getAvatarColor(name: string): string {
    if (!name) return '#94a3b8';
    const palette = ['#0EA5E9', '#10B981', '#8B5CF6', '#F97316', '#EC4899', '#14B8A6', '#F59E0B', '#6366F1'];
    let sum = 0;
    for (let i = 0; i < name.length; i++) {
      sum += name.charCodeAt(i);
    }
    return palette[sum % palette.length];
  }

  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
    return name.substring(0, 2).toUpperCase();
  }

  getStatusClass(status: string): string {
    if (status === 'Pending') return 'bg-[#FFF7ED] text-[#F97316]';
    if (status === 'Approved') return 'bg-[#F0FDF4] text-[#10B981]';
    if (status === 'Rejected') return 'bg-[#FEF2F2] text-[#EF4444]';
    return 'bg-gray-100 text-gray-600';
  }

  triggerToast(title: string, msg: string, type: 'success' | 'error') {
    this.toastTitle.set(title);
    this.toastMessage.set(msg);
    this.toastType.set(type);
    this.showToast.set(true);
    setTimeout(() => this.showToast.set(false), 4000);
  }

  logout() {
    this.authService.logout();
  }
}
