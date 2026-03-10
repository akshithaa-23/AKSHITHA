import { Component, OnInit, ChangeDetectorRef, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../../core/services/auth';

import { PolicyService } from '../../../services/policy.service';
import { QuoteRequestService } from '../../../services/quote-request.service';
import { QuoteService } from '../../../services/quote.service';
import { RecommendationService } from '../../../services/recommendation.service';
import { PaymentService } from '../../../services/payment.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  activeSection = 'dashboard';
  fullName = '';
  initials = '';

  // Data
  allRequests = signal<any[]>([]);
  quotes = signal<any[]>([]);
  recommendations = signal<any[]>([]);
  commissions = signal<any[]>([]);

  // Formatted Data computed signals
  formattedRequests = computed(() => this.allRequests().map(r => ({
    ...r,
    displayType: this.getDisplayType(r.requestType),
    typeClass: this.getTypeClass(r.requestType),
    displayStatus: this.getDisplayStatus(r.status, r.requestType),
    statusClass: this.getStatusClass(r.status, r.requestType)
  })));

  activeRequests = computed(() => this.formattedRequests().filter(r => r.status === 'Pending' || r.status === 'Assigned'));
  recentRequests = computed(() => this.formattedRequests().slice(0, 5));

  formattedQuotes = computed(() => this.quotes().map(q => ({
    ...q,
    displayDate: this.formatDate(q.createdAt || q.sentAt),
    displayStatus: this.getDisplayStatus(q.status, 'Quote'),
    statusClass: this.getStatusClass(q.status, 'Quote'),
    displayPremium: this.formatCurrency(q.totalPremium)
  })));

  formattedRecommendations = computed(() => this.recommendations().map(rec => ({
    ...rec,
    displayDate: this.formatDate(rec.createdAt),
    autoTierLabel: this.getTierLabel(rec.numberOfEmployees || 0)
  })));

  formattedCommissions = computed(() => this.commissions().map(c => ({
    ...c,
    displayAmount: this.formatCurrency(c.amountPaid),
    displayCommission: this.formatCurrency(c.commissionAmount),
    displayDate: this.formatDate(c.earnedAt),
    rateColor: this.getRateColor(c.commissionRate)
  })));

  // Loading
  loadingRequests = signal(false);
  loadingQuotes = signal(false);
  loadingRecommendations = signal(false);
  loadingCommissions = signal(false);

  // Dashboard stats (derived)
  statCustomers = computed(() => new Set(this.allRequests().map(r => r.customerName)).size);
  statPending = computed(() => this.activeRequests().length);
  statQuotesSent = computed(() => this.quotes().length);
  statCommission = computed(() => this.commissions().reduce((s: number, c: any) => s + (c.commissionAmount || 0), 0));

  showSuccessModal = false;
  successModalTitle = '';
  successModalMessage = '';

  // Commission breakdown
  commEssential = computed(() => this.commissions().filter(c => c.commissionRate == 5).reduce((s: number, c: any) => s + (c.commissionAmount || 0), 0));
  commEnhanced = computed(() => this.commissions().filter(c => c.commissionRate == 7).reduce((s: number, c: any) => s + (c.commissionAmount || 0), 0));
  commEnterprise = computed(() => this.commissions().filter(c => c.commissionRate == 10).reduce((s: number, c: any) => s + (c.commissionAmount || 0), 0));

  // Send Quote flow
  selectedRequest: any = null;
  sendQuoteForm = { employeeCount: 0, policyId: 0 as number | string };
  availablePolicies: any[] = [];
  submittingQuote = false;

  // Send Recommendation flow
  selectedRecRequest: any = null;
  recommendationMessage = '';
  submittingRecommendation = false;
  autoTierLabel = '';

  // Toast
  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  showToast = false;

  constructor(
    private authService: Auth,
    private cdr: ChangeDetectorRef,
    private policyService: PolicyService,
    private quoteRequestService: QuoteRequestService,
    private quoteService: QuoteService,
    private recommendationService: RecommendationService,
    private paymentService: PaymentService
  ) { }

  ngOnInit() {
    this.fullName = this.authService.getFullName() || 'Agent';
    this.initials = this.fullName.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
    this.loadAll();
  }

  loadAll() {
    this.loadPolicies();
    this.loadRequests();
    this.loadQuotes();
    this.loadRecommendations();
    this.loadCommissions();
  }

  navigate(section: string) {
    this.activeSection = section;
    if (section === 'requests') this.loadRequests();
    if (section === 'quotes') this.loadQuotes();
    if (section === 'recommendations') this.loadRecommendations();
    if (section === 'commissions') this.loadCommissions();
  }

  loadRequests() {
    this.loadingRequests.set(true);
    this.quoteRequestService.getAgentRequests().subscribe({
      next: data => {
        this.allRequests.set(data ?? []);
      },
      error: () => { this.allRequests.set([]); this.loadingRequests.set(false); },
      complete: () => { this.loadingRequests.set(false); }
    });
  }

  loadQuotes() {
    this.loadingQuotes.set(true);
    this.quoteService.getAgentQuotes().subscribe({
      next: data => {
        this.quotes.set(data ?? []);
      },
      error: () => { this.quotes.set([]); this.loadingQuotes.set(false); },
      complete: () => { this.loadingQuotes.set(false); }
    });
  }

  loadRecommendations() {
    this.loadingRecommendations.set(true);
    this.recommendationService.getAgentRecommendations().subscribe({
      next: data => { this.recommendations.set(data ?? []); },
      error: () => { this.recommendations.set([]); this.loadingRecommendations.set(false); },
      complete: () => { this.loadingRecommendations.set(false); }
    });
  }

  loadCommissions() {
    this.loadingCommissions.set(true);
    this.paymentService.getAgentCommissions().subscribe({
      next: data => {
        this.commissions.set(data ?? []);
      },
      error: () => { this.commissions.set([]); this.loadingCommissions.set(false); },
      complete: () => { this.loadingCommissions.set(false); }
    });
  }

  loadPolicies() {
    this.policyService.getPolicies().subscribe({
      next: data => { this.availablePolicies = data ?? []; },
      error: () => { }
    });
  }

  getTierLabel(employees: number): string {
    if (employees <= 80) return 'Essential';
    if (employees <= 200) return 'Enhanced';
    return 'Enterprise';
  }

  // ==== QUOTE ACTIONS ====
  openSendQuote(req: any) {
    this.selectedRequest = req;
    this.sendQuoteForm = {
      employeeCount: req.numberOfEmployees || 0,
      policyId: req.policyId || ''
    };
    this.activeSection = 'sendquote';
  }

  cancelSendQuote() {
    this.selectedRequest = null;
    this.navigate('requests');
  }

  submitSendQuote() {
    if (!this.sendQuoteForm.policyId || !this.sendQuoteForm.employeeCount) {
      this.showToastMsg('Please select a policy and enter employee count.', 'error'); return;
    }
    this.submittingQuote = true;
    this.quoteService.sendQuote({
      quoteRequestId: this.selectedRequest.id,
      policyId: +this.sendQuoteForm.policyId,
      employeeCount: this.sendQuoteForm.employeeCount
    }).subscribe({
      next: res => {
        const companyName = this.selectedRequest.companyName;
        this.allRequests.update(reqs => reqs.filter(r => r.id !== this.selectedRequest.id));
        this.selectedRequest = null;
        this.navigate('dashboard');
        this.openSuccessModal('Quote Sent Successfully!', `The quote has been sent to ${companyName}.`);
      },
      error: err => {
        this.showToastMsg(err?.error?.message || '✕ Failed to send. Please try again.', 'error');
        this.submittingQuote = false;
        this.cdr.markForCheck();
      },
      complete: () => {
        this.submittingQuote = false;
        this.cdr.markForCheck();
      }
    });
  }

  get requestedPolicyName(): string {
    if (!this.selectedRequest?.policyId) return 'Not specified';
    const policy = this.availablePolicies.find(p => p.id === this.selectedRequest.policyId);
    return policy ? policy.name : 'Unknown Policy';
  }

  get estimatedQuotePremium(): number {
    const policy = this.availablePolicies.find(p => p.id === +this.sendQuoteForm.policyId);
    if (!policy || !this.sendQuoteForm.employeeCount) return 0;
    return policy.premiumPerEmployee * this.sendQuoteForm.employeeCount;
  }

  get estimatedQuotePremiumPerEmployee(): number {
    const policy = this.availablePolicies.find(p => p.id === +this.sendQuoteForm.policyId);
    if (!policy) return 0;
    return policy.premiumPerEmployee;
  }

  // ==== RECOMMENDATION ACTIONS ====
  autoRecommendedPolicies: any[] = [];
  openSendRecommendation(req: any) {
    this.selectedRecRequest = req;
    this.recommendationMessage = '';
    this.autoTierLabel = req.autoTierLabel || this.getTierLabel(req.numberOfEmployees || 0);

    let pIds: number[] = [];
    if ((req.numberOfEmployees || 0) <= 80) pIds = [1, 2, 3];
    else if ((req.numberOfEmployees || 0) <= 200) pIds = [4, 5, 6];
    else pIds = [7, 8, 9];

    this.autoRecommendedPolicies = this.availablePolicies.filter(p => pIds.includes(p.id));

    this.activeSection = 'sendrecommendation';
  }

  cancelRecommendation() {
    this.selectedRecRequest = null;
    this.navigate('requests');
  }

  submitRecommendation() {
    if (!this.recommendationMessage.trim()) {
      this.showToastMsg('Please add a message to the customer.', 'error');
      return;
    }

    this.submittingRecommendation = true;
    this.recommendationService.sendRecommendation({
      quoteRequestId: this.selectedRecRequest.id,
      agentMessage: this.recommendationMessage
    }).subscribe({
      next: res => {
        const companyName = this.selectedRecRequest.companyName;
        this.allRequests.update(reqs => reqs.filter(r => r.id !== this.selectedRecRequest.id));
        this.selectedRecRequest = null;
        this.navigate('dashboard');
        this.openSuccessModal('Recommendation Sent!', `The recommendation has been sent to ${companyName}.`);
      },
      error: err => {
        this.showToastMsg(err?.error?.message || '✕ Failed to send. Please try again.', 'error');
        this.submittingRecommendation = false;
        this.cdr.markForCheck();
      },
      complete: () => {
        this.submittingRecommendation = false;
        this.cdr.markForCheck();
      }
    });
  }

  openSuccessModal(title: string, message: string) {
    this.successModalTitle = title;
    this.successModalMessage = message;
    this.showSuccessModal = true;
    this.cdr.markForCheck();
  }

  closeSuccessModal() {
    this.showSuccessModal = false;
    this.selectedRequest = null;
    this.selectedRecRequest = null;
    this.navigate('dashboard');
    this.cdr.markForCheck();
  }

  showToastMsg(msg: string, type: 'success' | 'error') {
    this.toastMessage = msg; this.toastType = type; this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3000);
  }

  // ==== FORMATTERS ====
  formatCurrency(val: number): string {
    if (!val && val !== 0) return '₹0';
    return '₹' + val.toLocaleString('en-IN');
  }

  formatDate(d: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  // UI mapping logic matching requirements
  getDisplayType(type: string): string {
    return type === 'DirectBuy' ? 'Quote' : type;
  }

  getDisplayStatus(status: string, type?: string): string {
    if (type === 'DirectBuy' || type === 'Quote') {
      if (status === 'QuoteSent' || status === 'Sent') return 'Sent';
      if (status === 'Completed' || status === 'Paid') return 'Purchased';
      if (status === 'Accepted') return 'Accepted';
      if (status === 'Assigned') return 'Pending';
    } else if (type === 'Recommendation') {
      if (status === 'Assigned') return 'Pending';
    }
    return status;
  }

  getStatusClass(status: string, type?: string): string {
    const dispStatus = this.getDisplayStatus(status, type);

    if (dispStatus === 'Pending' || dispStatus === 'Assigned') return 'badge-pending';
    if (dispStatus === 'Sent') return 'badge-quote';
    if (dispStatus === 'Purchased' || dispStatus === 'Completed') return 'badge-completed';
    if (dispStatus === 'Accepted') return 'badge-assigned';
    if (status === 'RecommendationSent') return 'badge-recommendation-sent';

    return 'badge-pending';
  }

  getTypeClass(type: string): string {
    return (type === 'DirectBuy' || type === 'Quote')
      ? 'badge-quote'
      : 'badge-recommendation';
  }

  getRateColor(rate: number): string {
    if (rate == 5) return 'text-orange-500';
    if (rate == 7) return 'text-blue-500';
    if (rate == 10) return 'text-green-500';
    return 'text-slate-600';
  }

  logout() { this.authService.logout(); }
}