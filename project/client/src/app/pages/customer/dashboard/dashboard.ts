import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Auth } from '../../../core/services/auth';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: [],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Dashboard implements OnInit {
  activeSection = 'home';
  fullName = '';
  initials = '';
  readonly apiUrl = 'https://localhost:7173/api';

  // Bell notifications
  notifications: { message: string; type: string; id: string }[] = [];
  visitedIds = new Set<string>();
  showNotificationPanel = false;

  // Data
  policies: any[] = [];
  myQuotes: any[] = [];
  rejectedQuotes: any[] = [];           // ← NEW: previously rejected quotes
  recommendations: any[] = [];
  ignoredRecommendations: any[] = [];   // ← NEW: previously ignored recommendations
  myCompany: any = null;
  myPayments: any[] = [];
  employees: any[] = [];

  // Loading flags
  loadingPolicies        = false;
  loadingQuotes          = false;
  loadingProfile         = false;
  loadingRecommendations = false;
  loadingEmployees       = false;
  submittingQuote        = false;
  submittingPayment      = false;
  submittingEmployee     = false;
  ignoringRecId: string | null = null;  // ← NEW: track which rec is being ignored

  // UI toggles
  showDirectBuyForm      = false;
  selectedPolicy: any    = null;
  showRecommendationForm = false;
  showPaymentModal       = false;
  selectedQuote: any     = null;
  showAddEmployeeForm    = false;
  showEditEmployeeForm   = false;
  editingEmployee: any   = null;
  employeeSearch         = '';

  // Toast
  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  showToast = false;

  // Success popup
  showSuccessModal       = false;
  successModalIcon       = '✅';
  successModalTitle      = '';
  successModalMessage    = '';
  successModalNavigateTo = 'home';

  // Invoice popup
  showInvoiceModal = false;
  invoiceData: any = null;

  // Forms
  directBuyForm = {
    companyName: '', industryType: '', numberOfEmployees: 0,
    location: '', contactName: '', contactEmail: '', contactPhone: ''
  };
  recommendationForm = {
    companyName: '', industryType: '', numberOfEmployees: 0,
    location: '', contactName: '', contactEmail: '', contactPhone: ''
  };
  paymentForm = {
    cardHolderName: '', cardNumber: '', expiry: '', cvv: '', paymentMethod: 'CreditCard'
  };
  addEmployeeForm = {
    employeeCode: '', fullName: '', email: '', gender: 'Male',
    salary: 0, coverageStartDate: '', nomineeName: '',
    nomineeRelationship: '', nomineePhone: ''
  };
  editEmployeeForm = {
    fullName: '', email: '', gender: 'Male', salary: 0,
    nomineeName: '', nomineeRelationship: '', nomineePhone: ''
  };

  // Static home data
  homeStats = [
    { icon: '👥', value: '12,000+', label: 'Employees Covered' },
    { icon: '🛡️', value: '500+',    label: 'Companies Trust Us' },
    { icon: '📈', value: '₹200 Cr+',label: 'Claims Settled' },
    { icon: '⭐', value: '4.8/5',   label: 'Customer Rating' }
  ];
  howSteps = [
    { num: '01', icon: '🛡️', title: 'Browse Plans',   desc: 'Explore our tiered insurance plans' },
    { num: '02', icon: '✅', title: 'Show Interest',   desc: "Click \"I'm Interested\" on any plan" },
    { num: '03', icon: '🕐', title: 'Get a Quote',     desc: 'Your agent sends a custom quote' },
    { num: '04', icon: '🏅', title: "You're Covered",  desc: 'Buy the plan and get instant coverage' }
  ];
  testimonials = [
    { quote: 'CorpSure made employee insurance effortless. The claims process is incredibly smooth.', name: 'Priya Sharma', role: 'HR Head, Nexus Innovations' },
    { quote: 'We switched to CorpSure last year and our employees love the coverage options.', name: 'Ravi Kumar', role: 'CEO, GlobalServer Inc.' },
    { quote: 'Transparent pricing, dedicated agent support, and fast claim settlements. Highly recommend.', name: 'Anita Desai', role: 'COO, Apex Digital' }
  ];
  features = [
    { icon: '❤️', title: 'Health Coverage', desc: 'Up to ₹10L health insurance per employee with cashless hospitalization at 5,000+ network hospitals.' },
    { icon: '🛡️', title: 'Life & Accident', desc: 'Up to 4x annual CTC life cover and ₹10L accident coverage to secure your employees\' families.' },
    { icon: '⚡', title: 'Instant Claims', desc: '90% of claims processed within 48 hours. Dedicated claims manager for every corporate account.' }
  ];

  constructor(private http: HttpClient, private authService: Auth, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.fullName = this.authService.getFullName() || 'Customer';
    this.initials = this.fullName.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
    try {
      const s = localStorage.getItem('cs_visited_notifs');
      if (s) JSON.parse(s).forEach((id: string) => this.visitedIds.add(id));
    } catch {}
    
    // Lazy-load policies & profile instead of calling on home init to improve perceived loading times
    
    // Quotes and recommendations are loaded early for bell notification counts
    this.loadQuotes();
    this.loadRecommendations();
  }

  // ── localStorage helpers ──────────────────────────────────
  private persistVisited() {
    try { localStorage.setItem('cs_visited_notifs', JSON.stringify([...this.visitedIds])); } catch {}
  }
  private markVisited(type: string) {
    this.notifications.filter(n => n.type === type).forEach(n => this.visitedIds.add(n.id));
    this.persistVisited();
  }

  // ── Notification counts ───────────────────────────────────
  get visibleNotifications() { return this.notifications.filter(n => !this.visitedIds.has(n.id)); }
  get totalNotifCount()  { return this.visibleNotifications.length; }
  get recNotifCount()    { return this.visibleNotifications.filter(n => n.type === 'recommendation').length; }
  get quoteNotifCount()  { return this.visibleNotifications.filter(n => n.type === 'quote').length; }

  // ── Navigation ────────────────────────────────────────────
  navigate(section: string) {
    this.activeSection = section;
    this.showDirectBuyForm = false; this.selectedPolicy = null;
    this.showNotificationPanel = false;
    this.showAddEmployeeForm = false; this.showEditEmployeeForm = false;
    
    // Lazy load triggers
    if (section === 'policies' && this.policies.length === 0) { this.loadPolicies(); }
    if (['profile', 'policies', 'recommendations', 'employees'].includes(section) && !this.myCompany) { this.loadMyProfile(); }
    
    if (section === 'recommendations') { this.markVisited('recommendation'); this.loadRecommendations(); }
    if (section === 'myquote')         { this.markVisited('quote');          this.loadQuotes(); }
    if (section === 'profile')         { this.loadMyProfile(); }
    if (section === 'employees' && this.employees.length === 0)       { this.loadEmployees(); }
    
    this.cdr.markForCheck();
  }
  toggleNotificationPanel() { this.showNotificationPanel = !this.showNotificationPanel; }
  onNotificationClick(n: any) {
    this.visitedIds.add(n.id); this.persistVisited();
    if (n.type === 'recommendation') this.navigate('recommendations');
    else this.navigate('myquote');
    this.showNotificationPanel = false;
  }

  // ── Modals ────────────────────────────────────────────────
  openSuccessModal(icon: string, title: string, msg: string, nav: string) {
    this.successModalIcon = icon; this.successModalTitle = title;
    this.successModalMessage = msg; this.successModalNavigateTo = nav;
    this.showSuccessModal = true;
    this.cdr.markForCheck();
  }
  closeSuccessModal() { this.showSuccessModal = false; this.navigate(this.successModalNavigateTo); }
  openInvoiceModal(data: any) { this.invoiceData = data; this.showInvoiceModal = true; this.cdr.markForCheck(); }
  closeInvoiceModal() { this.showInvoiceModal = false; this.cdr.markForCheck(); }
  viewPaymentInvoice(p: any) {
    this.openInvoiceModal({
      invoiceNumber: p.invoiceNumber, policyName: p.policyName,
      companyName: p.companyName || this.myCompany?.companyName || '',
      employeeCount: p.employeeCount, amountPaid: p.amountPaid,
      paymentMethod: p.paymentMethod, maskedCardNumber: p.maskedCardNumber,
      cardHolderName: p.cardHolderName, paidAt: p.paidAt, agentName: p.agentName
    });
  }

  // ── API Loaders ───────────────────────────────────────────
  loadMyProfile() {
    this.loadingProfile = true;
    this.myCompany = null;
    this.myPayments = [];

    let companyDone = false;
    let paymentsDone = false;
    const checkDone = () => { if (companyDone && paymentsDone) { this.loadingProfile = false; this.cdr.markForCheck(); } };

    this.http.get<any>(`${this.apiUrl}/company/my`).subscribe({
      next:     d  => { this.myCompany = d; },
      error:    () => { this.myCompany = null; this.cdr.markForCheck(); },
      complete: () => { companyDone = true; checkDone(); }
    });

    this.http.get<any[]>(`${this.apiUrl}/payment/my`).subscribe({
      next:     d  => { this.myPayments = d ?? []; },
      error:    () => { this.myPayments = []; this.cdr.markForCheck(); },
      complete: () => { paymentsDone = true; checkDone(); }
    });
  }

  loadPolicies() {
    this.loadingPolicies = true;
    this.http.get<any[]>(`${this.apiUrl}/policy`).subscribe({
      next:     d  => { this.policies = d ?? []; },
      error:    () => { this.policies = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingPolicies = false; this.cdr.markForCheck(); }
    });
  }

  loadQuotes() {
    this.loadingQuotes = true;
    this.http.get<any[]>(`${this.apiUrl}/quote/my`).subscribe({
      next: data => {
        const all = data ?? [];
        // Active quotes: Pending or Accepted
        this.myQuotes = all.filter(q => q.status === 'Pending' || q.status === 'Accepted');
        // Rejected quotes go to history table
        this.rejectedQuotes = all.filter(q => q.status === 'Rejected');
        // Bell notifications for active quotes
        this.myQuotes.forEach(q => {
          const id = `quote-${q.id}`;
          if (!this.notifications.find(n => n.id === id))
            this.notifications.push({ id, type: 'quote',
              message: q.status === 'Accepted'
                ? `Quote for ${q.policyName} accepted — ready to pay!`
                : `A new quote is ready for ${q.policyName}.`
            });
        });
      },
      error:    () => { this.myQuotes = []; this.rejectedQuotes = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingQuotes = false; this.cdr.markForCheck(); }
    });
  }

  loadRecommendations() {
    this.loadingRecommendations = true;
    this.http.get<any[]>(`${this.apiUrl}/recommendation/my`).subscribe({
      next: data => {
        const all = data ?? [];
        // Active (not ignored) recommendations
        this.recommendations = all.filter(r => r.status !== 'Ignored');
        // Ignored recommendations for history table
        this.ignoredRecommendations = all.filter(r => r.status === 'Ignored');
        // Bell notifications for active recommendations
        this.recommendations.forEach(r => {
          const id = `rec-${r.id}`;
          if (!this.notifications.find(n => n.id === id))
            this.notifications.push({ id, type: 'recommendation', message: 'Your agent sent a recommendation!' });
        });
      },
      error:    () => { this.recommendations = []; this.ignoredRecommendations = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingRecommendations = false; this.cdr.markForCheck(); }
    });
  }

  loadEmployees() {
    this.loadingEmployees = true;
    this.http.get<any[]>(`${this.apiUrl}/employee/my-company`).subscribe({
      next:     d  => { this.employees = d ?? []; },
      error:    () => { this.employees = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingEmployees = false; this.cdr.markForCheck(); }
    });
  }

  // ── Employee helpers ──────────────────────────────────────
  get filteredEmployees() {
    const s = this.employeeSearch.toLowerCase();
    if (!s) return this.employees;
    return this.employees.filter(e =>
      e.fullName?.toLowerCase().includes(s) ||
      e.employeeCode?.toLowerCase().includes(s) ||
      e.email?.toLowerCase().includes(s)
    );
  }
  get activeCount()   { return this.employees.filter(e => e.isActive).length; }
  get inactiveCount() { return this.employees.filter(e => !e.isActive).length; }

  openAddEmployee() {
    this.addEmployeeForm = {
      employeeCode: '', fullName: '', email: '', gender: 'Male',
      salary: 0, coverageStartDate: '', nomineeName: '',
      nomineeRelationship: '', nomineePhone: ''
    };
    this.showAddEmployeeForm = true;
  }
  cancelAddEmployee() { this.showAddEmployeeForm = false; }

  submitAddEmployee() {
    if (!this.addEmployeeForm.fullName || !this.addEmployeeForm.employeeCode || !this.addEmployeeForm.email) {
      this.showToastMsg('Name, Employee Code and Email are required.', 'error'); return;
    }
    this.submittingEmployee = true;
    this.http.post<any>(`${this.apiUrl}/employee`, this.addEmployeeForm).subscribe({
      next:     () => { this.showAddEmployeeForm = false; this.showToastMsg('Employee added!', 'success'); this.loadEmployees(); },
      error:    err => { this.showToastMsg(err?.error?.message || 'Failed to add.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingEmployee = false; this.cdr.markForCheck(); }
    });
  }

  openEditEmployee(emp: any) {
    this.editingEmployee = emp;
    this.editEmployeeForm = {
      fullName: emp.fullName, email: emp.email, gender: emp.gender ?? 'Male',
      salary: emp.salary, nomineeName: emp.nomineeName ?? '',
      nomineeRelationship: emp.nomineeRelationship ?? '', nomineePhone: emp.nomineePhone ?? ''
    };
    this.showEditEmployeeForm = true;
  }
  cancelEditEmployee() { this.showEditEmployeeForm = false; this.editingEmployee = null; }

  submitEditEmployee() {
    if (!this.editingEmployee) return;
    this.submittingEmployee = true;
    this.http.put<any>(`${this.apiUrl}/employee/${this.editingEmployee.id}`, this.editEmployeeForm).subscribe({
      next:     () => { this.showEditEmployeeForm = false; this.editingEmployee = null; this.showToastMsg('Employee updated!', 'success'); this.loadEmployees(); },
      error:    err => { this.showToastMsg(err?.error?.message || 'Failed to update.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingEmployee = false; this.cdr.markForCheck(); }
    });
  }

  deactivateEmployee(emp: any) {
    if (!confirm(`Deactivate ${emp.fullName}? They will lose coverage.`)) return;
    this.http.put<any>(`${this.apiUrl}/employee/${emp.id}/deactivate`, {}).subscribe({
      next:  () => { this.showToastMsg('Employee deactivated.', 'success'); this.loadEmployees(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); }
    });
  }

  // ── Direct Buy ────────────────────────────────────────────
  onInterestedClick(policy: any) {
    if (this.myCompany?.activePolicy) { this.showToastMsg('Your company already has an active policy.', 'error'); return; }
    this.selectedPolicy = policy;
    if (this.myCompany) {
      this.directBuyForm.companyName       = this.myCompany.companyName ?? '';
      this.directBuyForm.contactName       = this.myCompany.representativeName ?? '';
      this.directBuyForm.contactEmail      = this.myCompany.representativeEmail ?? '';
      this.directBuyForm.numberOfEmployees = this.myCompany.size ?? 0;
    }
    this.showDirectBuyForm = true;
  }
  cancelDirectBuy() { this.showDirectBuyForm = false; this.selectedPolicy = null; }
  get estimatedPremium(): number {
    return (this.selectedPolicy && this.directBuyForm.numberOfEmployees)
      ? this.directBuyForm.numberOfEmployees * this.selectedPolicy.premiumPerEmployee : 0;
  }
  submitDirectBuy() {
    if (!this.selectedPolicy) return;
    if (this.directBuyForm.numberOfEmployees < this.selectedPolicy.minEmployees) {
      this.showToastMsg(`Minimum ${this.selectedPolicy.minEmployees} employees required.`, 'error'); return;
    }
    this.submittingQuote = true;
    this.http.post<any>(`${this.apiUrl}/quoterequest/direct-buy`,
      { policyId: this.selectedPolicy.id, ...this.directBuyForm }).subscribe({
      next:     () => { this.showDirectBuyForm = false; this.selectedPolicy = null; this.loadMyProfile(); this.openSuccessModal('📋', 'Quote Request Sent!', "Your agent has received your request and will send a quote shortly. You'll be notified when it's ready.", 'home'); },
      error:    err => { this.showToastMsg(err?.error?.message || 'Failed to submit.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingQuote = false; this.cdr.markForCheck(); }
    });
  }

  // ── Recommendation ────────────────────────────────────────
  openRecommendationForm() {
    if (this.myCompany?.activePolicy) { this.showToastMsg('Already have an active policy.', 'error'); return; }
    if (this.myCompany) {
      this.recommendationForm.companyName       = this.myCompany.companyName ?? '';
      this.recommendationForm.contactName       = this.myCompany.representativeName ?? '';
      this.recommendationForm.contactEmail      = this.myCompany.representativeEmail ?? '';
      this.recommendationForm.numberOfEmployees = this.myCompany.size ?? 0;
    }
    this.showRecommendationForm = true;
  }
  submitRecommendation() {
    this.submittingQuote = true;
    this.http.post<any>(`${this.apiUrl}/quoterequest/recommendation`, this.recommendationForm).subscribe({
      next:     () => { this.showRecommendationForm = false; this.openSuccessModal('🎯', 'Recommendation Requested!', 'Your agent has been notified. They will review your profile and suggest the best plans.', 'home'); },
      error:    err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingQuote = false; this.cdr.markForCheck(); }
    });
  }

  // ── Ignore Recommendation ─────────────────────────────────
  ignoreRecommendation(rec: any) {
    this.ignoringRecId = rec.id;
    this.http.put<any>(`${this.apiUrl}/recommendation/${rec.id}/ignore`, {}).subscribe({
      next: () => {
        this.showToastMsg('Recommendation ignored.', 'success');
        // Move from active to ignored list locally for instant UI update
        this.recommendations = this.recommendations.filter(r => r.id !== rec.id);
        rec.status = 'Ignored';
        this.ignoredRecommendations = [rec, ...this.ignoredRecommendations];
        // Remove its bell notification
        this.notifications = this.notifications.filter(n => n.id !== `rec-${rec.id}`);
        this.visitedIds.delete(`rec-${rec.id}`);
        this.persistVisited();
        this.cdr.markForCheck();
      },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed to ignore.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.ignoringRecId = null; this.cdr.markForCheck(); }
    });
  }

  // ── Quote Actions ─────────────────────────────────────────
  acceptQuote(q: any) {
    this.http.put<any>(`${this.apiUrl}/quote/${q.id}/accept`, {}).subscribe({
      next:  () => { this.showToastMsg('Quote accepted! Click Buy Now to proceed.', 'success'); this.loadQuotes(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); }
    });
  }
  rejectQuote(q: any) {
    this.http.put<any>(`${this.apiUrl}/quote/${q.id}/reject`, {}).subscribe({
      next: () => {
        this.showToastMsg('Quote rejected.', 'success');
        // Move from active to rejected list locally
        this.myQuotes = this.myQuotes.filter(quote => quote.id !== q.id);
        q.status = 'Rejected';
        this.rejectedQuotes = [q, ...this.rejectedQuotes];
        // Remove its bell notification
        this.notifications = this.notifications.filter(n => n.id !== `quote-${q.id}`);
        this.visitedIds.delete(`quote-${q.id}`);
        this.persistVisited();
        this.cdr.markForCheck();
      },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); }
    });
  }
  openPayment(q: any) {
    this.selectedQuote = q;
    this.paymentForm = { cardHolderName: '', cardNumber: '', expiry: '', cvv: '', paymentMethod: 'CreditCard' };
    this.showPaymentModal = true;
  }
  closePaymentModal() { this.showPaymentModal = false; this.selectedQuote = null; }
  confirmPayment() {
    if (!this.selectedQuote) return;
    if (!this.paymentForm.cardHolderName || !this.paymentForm.cardNumber || !this.paymentForm.cvv) {
      this.showToastMsg('Please fill all card details.', 'error'); return;
    }
    this.submittingPayment = true;
    this.http.post<any>(`${this.apiUrl}/payment`, {
      quoteId: this.selectedQuote.id, paymentMethod: this.paymentForm.paymentMethod,
      cardHolderName: this.paymentForm.cardHolderName, cardNumber: this.paymentForm.cardNumber
    }).subscribe({
      next:     res => { this.showPaymentModal = false; this.loadMyProfile(); this.loadQuotes(); this.openInvoiceModal(res); },
      error:    err => { this.showToastMsg(err?.error?.message || 'Payment failed.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingPayment = false; this.cdr.markForCheck(); }
    });
  }

  // ── Helpers ───────────────────────────────────────────────
  showToastMsg(msg: string, type: 'success' | 'error') {
    this.toastMessage = msg; this.toastType = type; this.showToast = true;
    this.cdr.markForCheck();
    setTimeout(() => { this.showToast = false; this.cdr.markForCheck(); }, 4000);
  }
  formatCurrency(val: number): string {
    if (!val && val !== 0) return '₹0';
    return '₹' + val.toLocaleString('en-IN');
  }
  formatDate(d: string): string {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }
  logout() { this.authService.logout(); }
  getAgentInitials(name: string): string {
    if (!name) return '';
    return name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
  }
}