import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../../core/services/auth';

import { CompanyService } from '../../../services/company.service';
import { PaymentService } from '../../../services/payment.service';
import { PolicyService } from '../../../services/policy.service';
import { QuoteService } from '../../../services/quote.service';
import { RecommendationService } from '../../../services/recommendation.service';
import { EmployeeService } from '../../../services/employee.service';
import { ClaimService } from '../../../services/claim.service';
import { QuoteRequestService } from '../../../services/quote-request.service';

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

  // Bell notifications
  notifications: { message: string; type: string; id: string }[] = [];
  visitedIds = new Set<string>();
  showNotificationPanel = false;

  // Data
  policies: any[] = [];
  myQuotes: any[] = [];
  rejectedQuotes: any[] = [];
  recommendations: any[] = [];
  ignoredRecommendations: any[] = [];
  myCompany: any = null;
  myPayments: any[] = [];
  employees: any[] = [];

  // Claims
  myClaims: any[] = [];
  allowedTypes: any = null;
  allowedTypesError = false;
  loadingClaims = false;

  // Claims Wizard
  showClaimsWizard = false;
  wizardStep = 1;
  claimForm: any = {
    employeeId: null, employee: null,
    claimType: '', requestedAmount: null, reason: '',
    documentUrl: null, file: null, fileName: '', fileSize: '', uploading: false,
    accidentType: '', accidentPercentage: null
  };
  submittingClaim = false;
  claimSubmitError = '';

  // Loading flags
  loadingPolicies = false;
  loadingQuotes = false;
  loadingProfile = false;
  loadingRecommendations = false;
  loadingEmployees = false;
  submittingQuote = false;
  submittingPayment = false;
  submittingEmployee = false;
  ignoringRecId: string | null = null;

  // UI toggles
  showDirectBuyForm = false;
  selectedPolicy: any = null;
  showRecommendationForm = false;
  showPaymentModal = false;
  selectedQuote: any = null;
  showAddEmployeeForm = false;
  showEditEmployeeForm = false;
  editingEmployee: any = null;
  employeeSearch = '';

  // Toast
  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  showToast = false;

  // Success popup
  showSuccessModal = false;
  successModalIcon = '✅';
  successModalTitle = '';
  successModalMessage = '';
  successModalNavigateTo = 'home';

  // Invoice popup
  showInvoiceModal = false;
  invoiceData: any = null;
  isPrinting = false;

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
    salary: 0, nomineeName: '',
    nomineeRelationship: '', nomineePhone: ''
  };
  editEmployeeForm = {
    fullName: '', email: '', gender: 'Male', salary: 0,
    nomineeName: '', nomineeRelationship: '', nomineePhone: ''
  };

  // Static home data
  homeStats = [
    { icon: '👥', value: '12,000+', label: 'Employees Covered' },
    { icon: '🛡️', value: '500+', label: 'Companies Trust Us' },
    { icon: '📈', value: '₹200 Cr+', label: 'Claims Settled' },
    { icon: '⭐', value: '4.8/5', label: 'Customer Rating' }
  ];
  howSteps = [
    { num: '01', icon: '🛡️', title: 'Browse Plans', desc: 'Explore our tiered insurance plans' },
    { num: '02', icon: '✅', title: 'Show Interest', desc: "Click \"I'm Interested\" on any plan" },
    { num: '03', icon: '🕐', title: 'Get a Quote', desc: 'Your agent sends a custom quote' },
    { num: '04', icon: '🏅', title: "You're Covered", desc: 'Buy the plan and get instant coverage' }
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

  constructor(
    private authService: Auth,
    private cdr: ChangeDetectorRef,
    private companyService: CompanyService,
    private paymentService: PaymentService,
    private policyService: PolicyService,
    private quoteService: QuoteService,
    private recommendationService: RecommendationService,
    private employeeService: EmployeeService,
    private claimService: ClaimService,
    private quoteRequestService: QuoteRequestService
  ) { }

  ngOnInit() {
    this.fullName = this.authService.getFullName() || 'Customer';
    this.initials = this.fullName.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
    try {
      const s = localStorage.getItem('cs_visited_notifs');
      if (s) JSON.parse(s).forEach((id: string) => this.visitedIds.add(id));
    } catch { }

    this.loadQuotes();
    this.loadRecommendations();
  }

  // ── localStorage helpers ──────────────────────────────────
  private persistVisited() {
    try { localStorage.setItem('cs_visited_notifs', JSON.stringify([...this.visitedIds])); } catch { }
  }
  private markVisited(type: string) {
    this.notifications.filter(n => n.type === type).forEach(n => this.visitedIds.add(n.id));
    this.persistVisited();
  }

  // ── Notification counts ───────────────────────────────────
  get visibleNotifications() { return this.notifications.filter(n => !this.visitedIds.has(n.id)); }
  get totalNotifCount() { return this.visibleNotifications.length; }
  get recNotifCount() { return this.visibleNotifications.filter(n => n.type === 'recommendation').length; }
  get quoteNotifCount() { return this.visibleNotifications.filter(n => n.type === 'quote').length; }

  // ── Navigation ────────────────────────────────────────────
  navigate(section: string) {
    this.activeSection = section;
    this.showDirectBuyForm = false; this.selectedPolicy = null;
    this.showNotificationPanel = false;
    this.showAddEmployeeForm = false; this.showEditEmployeeForm = false;

    if (section === 'policies' && this.policies.length === 0) { this.loadPolicies(); }
    if (['profile', 'policies', 'recommendations', 'employees'].includes(section) && !this.myCompany) { this.loadMyProfile(); }

    if (section === 'recommendations') { this.markVisited('recommendation'); this.loadRecommendations(); }
    if (section === 'myquote') { this.markVisited('quote'); this.loadQuotes(); }
    if (section === 'profile') { this.loadMyProfile(); }
    if (section === 'claims') {
      this.showClaimsWizard = false;
      this.wizardStep = 1;
      this.loadAllowedTypes();
      this.loadMyClaims();
      if (this.employees.length === 0) this.loadEmployees();
    }
    if (section === 'employees' && this.employees.length === 0) { this.loadEmployees(); }

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
  downloadInvoicePDF() {
    this.isPrinting = true;
    this.cdr.markForCheck();
    setTimeout(() => {
      window.print();
      this.isPrinting = false;
      this.cdr.markForCheck();
    }, 500);
  }
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

    this.companyService.getMyCompany().subscribe({
      next: d => { this.myCompany = d; this.cdr.markForCheck(); },
      error: () => { this.myCompany = null; this.cdr.markForCheck(); },
      complete: () => { companyDone = true; checkDone(); }
    });

    this.paymentService.getMyPayments().subscribe({
      next: d => { this.myPayments = d ?? []; this.cdr.markForCheck(); },
      error: () => { this.myPayments = []; this.cdr.markForCheck(); },
      complete: () => { paymentsDone = true; checkDone(); }
    });
  }

  loadPolicies() {
    this.loadingPolicies = true;
    this.policyService.getPolicies().subscribe({
      next: d => { this.policies = d ?? []; this.cdr.markForCheck(); },
      error: () => { this.policies = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingPolicies = false; this.cdr.markForCheck(); }
    });
  }

  loadQuotes() {
    this.loadingQuotes = true;
    this.quoteService.getMyQuotes().subscribe({
      next: data => {
        const all = data ?? [];
        this.myQuotes = all.filter(q => q.status === 'Pending' || q.status === 'Accepted');
        this.rejectedQuotes = all.filter(q => q.status === 'Rejected');

        const activeQuoteIds = new Set(this.myQuotes.map(q => `quote-${q.id}`));
        this.notifications = this.notifications.filter(n => n.type !== 'quote' || activeQuoteIds.has(n.id));

        this.myQuotes.forEach(q => {
          const id = `quote-${q.id}`;
          if (!this.notifications.find(n => n.id === id))
            this.notifications.push({
              id, type: 'quote',
              message: q.status === 'Accepted'
                ? `Quote for ${q.policyName} accepted — ready to pay!`
                : `A new quote is ready for ${q.policyName}.`
            });
        });
      },
      error: () => { this.myQuotes = []; this.rejectedQuotes = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingQuotes = false; this.cdr.markForCheck(); }
    });
  }

  loadRecommendations() {
    this.loadingRecommendations = true;
    this.recommendationService.getMyRecommendations().subscribe({
      next: data => {
        const all = data ?? [];
        this.recommendations = all.filter(r => r.status !== 'Ignored');
        this.ignoredRecommendations = all.filter(r => r.status === 'Ignored');

        const activeRecIds = new Set(this.recommendations.map(r => `rec-${r.id}`));
        this.notifications = this.notifications.filter(n => n.type !== 'recommendation' || activeRecIds.has(n.id));

        this.recommendations.forEach(r => {
          const id = `rec-${r.id}`;
          if (!this.notifications.find(n => n.id === id))
            this.notifications.push({ id, type: 'recommendation', message: 'Your agent sent a recommendation!' });
        });

        this.loadingRecommendations = false;
        this.cdr.markForCheck();
      },
      error: () => { this.recommendations = []; this.ignoredRecommendations = []; this.loadingRecommendations = false; this.cdr.markForCheck(); },
      complete: () => { this.cdr.markForCheck(); }
    });
  }

  loadEmployees() {
    this.loadingEmployees = true;
    this.employeeService.getMyCompanyEmployees().subscribe({
      next: d => { this.employees = d ?? []; this.cdr.markForCheck(); },
      error: () => { this.employees = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingEmployees = false; this.cdr.markForCheck(); }
    });
  }

  loadAllowedTypes() {
    this.allowedTypesError = false;
    this.claimService.getAllowedTypes().subscribe({
      next: d => { this.allowedTypes = d; this.cdr.markForCheck(); },
      error: () => { this.allowedTypes = null; this.allowedTypesError = true; this.cdr.markForCheck(); }
    });
  }

  loadMyClaims() {
    this.loadingClaims = true;
    this.claimService.getMyClaims().subscribe({
      next: d => { this.myClaims = d ?? []; this.cdr.markForCheck(); },
      error: () => { this.myClaims = []; this.cdr.markForCheck(); },
      complete: () => { this.loadingClaims = false; this.cdr.markForCheck(); }
    });
  }

  // ── Policy Grouping Helpers ─────────────────────────────────
  get essentialPolicies() { return this.policies.filter(p => (p.name || '').toLowerCase().includes('essential')); }
  get enhancedPolicies() { return this.policies.filter(p => (p.name || '').toLowerCase().includes('enhanced')); }
  get enterprisePolicies() { return this.policies.filter(p => (p.name || '').toLowerCase().includes('enterprise')); }
  get otherPolicies() {
    return this.policies.filter(p => {
      const n = (p.name || '').toLowerCase();
      return !n.includes('essential') && !n.includes('enhanced') && !n.includes('enterprise');
    });
  }

  // ── Employee helpers ──────────────────────────────────────
  get maxSalaryCap(): number {
    const activePolicy = this.myCompany?.activePolicy;
    return (activePolicy && activePolicy.lifeCoverageMultiplier)
      ? (activePolicy.maxLifeCoverageLimit || 10000000)
      : 10000000;
  }
  get filteredEmployees() {
    const s = this.employeeSearch.toLowerCase();
    if (!s) return this.employees;
    return this.employees.filter(e =>
      e.fullName?.toLowerCase().includes(s) ||
      e.employeeCode?.toLowerCase().includes(s) ||
      e.email?.toLowerCase().includes(s)
    );
  }
  get activeCount() { return this.employees.filter(e => e.isActive).length; }
  get inactiveCount() { return this.employees.filter(e => !e.isActive).length; }

  openAddEmployee() {
    this.addEmployeeForm = {
      employeeCode: '', fullName: '', email: '', gender: 'Male',
      salary: 0, nomineeName: '',
      nomineeRelationship: '', nomineePhone: ''
    };
    this.showAddEmployeeForm = true;
  }
  cancelAddEmployee() { this.showAddEmployeeForm = false; }

  submitAddEmployee() {
    if (!this.addEmployeeForm.fullName || !this.addEmployeeForm.employeeCode || !this.addEmployeeForm.email) {
      this.showToastMsg('Name, Employee Code and Email are required.', 'error'); return;
    }

    const activePolicy = this.myCompany?.activePolicy;
    const maxCap = (activePolicy && activePolicy.lifeCoverageMultiplier)
      ? (activePolicy.maxLifeCoverageLimit || 10000000)
      : 10000000;

    if (this.addEmployeeForm.salary > maxCap) {
      this.showToastMsg(`Salary cannot exceed the maximum life insurance cap of ₹${maxCap.toLocaleString('en-IN')}`, 'error');
      return;
    }

    this.submittingEmployee = true;
    this.employeeService.addEmployee(this.addEmployeeForm).subscribe({
      next: () => { this.showAddEmployeeForm = false; this.showToastMsg('Employee added!', 'success'); this.loadEmployees(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed to add.', 'error'); this.cdr.markForCheck(); },
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

    const activePolicy = this.myCompany?.activePolicy;
    const maxCap = (activePolicy && activePolicy.lifeCoverageMultiplier)
      ? (activePolicy.maxLifeCoverageLimit || 10000000)
      : 10000000;

    if (this.editEmployeeForm.salary > maxCap) {
      this.showToastMsg(`Salary cannot exceed the maximum life insurance cap of ₹${maxCap.toLocaleString('en-IN')}`, 'error');
      return;
    }

    this.submittingEmployee = true;
    this.employeeService.updateEmployee(this.editingEmployee.id, this.editEmployeeForm).subscribe({
      next: () => { this.showEditEmployeeForm = false; this.editingEmployee = null; this.showToastMsg('Employee updated!', 'success'); this.loadEmployees(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed to update.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingEmployee = false; this.cdr.markForCheck(); }
    });
  }

  deactivateEmployee(emp: any) {
    if (!confirm(`Deactivate ${emp.fullName}? They will lose coverage.`)) return;
    this.employeeService.deactivateEmployee(emp.id).subscribe({
      next: () => { this.showToastMsg('Employee deactivated.', 'success'); this.loadEmployees(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); }
    });
  }

  // ── Direct Buy ────────────────────────────────────────────
  onInterestedClick(policy: any) {
    this.selectedPolicy = policy;
    if (this.myCompany) {
      this.directBuyForm.companyName = this.myCompany.companyName ?? '';
      this.directBuyForm.contactName = this.myCompany.representativeName ?? '';
      this.directBuyForm.contactEmail = this.myCompany.representativeEmail ?? '';
      this.directBuyForm.numberOfEmployees = this.myCompany.size ?? 0;
    }
    this.showDirectBuyForm = true;
    this.cdr.markForCheck();
    setTimeout(() => {
      document.getElementById('direct-buy-form')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 100);
  }
  cancelDirectBuy() { this.showDirectBuyForm = false; this.selectedPolicy = null; this.cdr.markForCheck(); }
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
    this.cdr.markForCheck();
    this.quoteRequestService.submitDirectBuy({ policyId: this.selectedPolicy.id, ...this.directBuyForm }).subscribe({
      next: () => {
        this.showDirectBuyForm = false;
        this.selectedPolicy = null;

        this.notifications = this.notifications.filter(n => n.type !== 'recommendation');
        this.persistVisited();

        this.loadMyProfile();
        this.openSuccessModal('📋', 'Quote Request Sent!', "Your agent has received your request and will send a quote shortly. You'll be notified when it's ready.", 'home');
      },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed to submit.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingQuote = false; this.cdr.markForCheck(); }
    });
  }

  // ── Recommendation ────────────────────────────────────────
  openRecommendationForm() {
    if (this.myCompany) {
      this.recommendationForm.companyName = this.myCompany.companyName ?? '';
      this.recommendationForm.contactName = this.myCompany.representativeName ?? '';
      this.recommendationForm.contactEmail = this.myCompany.representativeEmail ?? '';
      this.recommendationForm.numberOfEmployees = this.myCompany.size ?? 0;
    }
    this.showRecommendationForm = true;
    this.cdr.markForCheck();
  }
  submitRecommendation() {
    if (this.recommendationForm.numberOfEmployees < 10) {
      this.showToastMsg('Minimum 10 employees required.', 'error');
      return;
    }
    this.submittingQuote = true;
    this.cdr.markForCheck();
    this.quoteRequestService.requestRecommendation(this.recommendationForm).subscribe({
      next: () => {
        this.showRecommendationForm = false;
        this.loadMyProfile();
        this.openSuccessModal('🎯', 'Recommendation Requested!', 'Your agent has been notified. They will review your profile and suggest the best plans.', 'home');
      },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingQuote = false; this.cdr.markForCheck(); }
    });
  }



  // ── Quote Actions ─────────────────────────────────────────
  acceptQuote(q: any) {
    this.quoteService.acceptQuote(q.id).subscribe({
      next: () => { this.showToastMsg('Quote accepted! Click Buy Now to proceed.', 'success'); this.loadQuotes(); },
      error: err => { this.showToastMsg(err?.error?.message || 'Failed.', 'error'); this.cdr.markForCheck(); }
    });
  }
  rejectQuote(q: any) {
    this.quoteService.rejectQuote(q.id).subscribe({
      next: () => {
        this.showToastMsg('Quote rejected.', 'success');
        this.myQuotes = this.myQuotes.filter(quote => quote.id !== q.id);
        q.status = 'Rejected';
        this.rejectedQuotes = [q, ...this.rejectedQuotes];
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
    const cardDigits = this.paymentForm.cardNumber.replace(/\D/g, '');
    if (cardDigits.length !== 12) {
      this.showToastMsg('Card number must be 12 digits.', 'error'); return;
    }
    if (this.paymentForm.cvv.length !== 3) {
      this.showToastMsg('CVV must be 3 digits.', 'error'); return;
    }
    if (this.isCardExpired(this.paymentForm.expiry)) {
      this.showToastMsg('Card must not be expired.', 'error'); return;
    }
    this.submittingPayment = true;
    this.paymentService.processPayment({
      quoteId: this.selectedQuote.id, paymentMethod: this.paymentForm.paymentMethod,
      cardHolderName: this.paymentForm.cardHolderName, cardNumber: this.paymentForm.cardNumber
    }).subscribe({
      next: res => { this.showPaymentModal = false; this.loadMyProfile(); this.loadQuotes(); this.openInvoiceModal(res); },
      error: err => { this.showToastMsg(err?.error?.message || 'Payment failed.', 'error'); this.cdr.markForCheck(); },
      complete: () => { this.submittingPayment = false; this.cdr.markForCheck(); }
    });
  }

  // ── Claims Wizard ─────────────────────────────────────────
  startClaimWizard() {
    if (this.allowedTypesError || !this.allowedTypes) return;
    this.claimForm = {
      employeeId: null, employee: null, claimType: '', requestedAmount: null, reason: '',
      documentUrl: null, file: null, fileName: '', fileSize: '', uploading: false,
      accidentType: '', accidentPercentage: null
    };
    this.claimSubmitError = '';
    this.showClaimsWizard = true;
    this.wizardStep = 1;
  }
  closeClaimWizard() { this.showClaimsWizard = false; }
  goToStep(step: number) { this.wizardStep = step; }

  selectClaimEmployee(emp: any) {
    this.claimForm.employeeId = emp.id;
    this.claimForm.employee = emp;
  }

  selectClaimType(type: string) {
    if (!this.allowedTypes?.allowedClaimTypes?.includes(type)) return;
    this.claimForm.claimType = type;
  }

  get remainingHealthCoverage(): number {
    if (!this.claimForm.employee || !this.allowedTypes) return 0;
    const employeeClaims = this.myClaims.filter(c => c.employeeId === this.claimForm.employee.id && c.claimType === 'Health' && c.status === 'Approved');
    const spent = employeeClaims.reduce((sum, c) => sum + c.claimAmount, 0);
    return Math.max(0, this.allowedTypes.healthCoverage - spent);
  }

  get isHealthAmountExceeded(): boolean {
    if (this.claimForm.claimType !== 'Health' || !this.claimForm.requestedAmount) return false;
    return this.claimForm.requestedAmount > this.remainingHealthCoverage;
  }

  get rawLifePayout(): number {
    if (!this.claimForm.employee || !this.allowedTypes || !this.allowedTypes.lifeCoverageMultiplier) return 0;
    return this.claimForm.employee.salary * this.allowedTypes.lifeCoverageMultiplier;
  }

  get cappedLifePayout(): number {
    const raw = this.rawLifePayout;
    const max = this.allowedTypes?.maxLifeCoverageLimit;
    return max ? Math.min(raw, max) : raw;
  }

  get calculatedAccidentPayout(): number {
    if (!this.allowedTypes || !this.allowedTypes.accidentCoverage) return 0;
    if (this.claimForm.accidentType === 'Complete') return this.allowedTypes.accidentCoverage;
    if (this.claimForm.accidentType === 'Partial' && this.claimForm.accidentPercentage) {
      return this.allowedTypes.accidentCoverage * (this.claimForm.accidentPercentage / 100);
    }
    return 0;
  }

  onClaimFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    const ext = file.name.split('.').pop()?.toLowerCase();
    if (!['pdf', 'jpg', 'jpeg', 'png'].includes(ext)) {
      this.showToastMsg('Only PDF, JPG, PNG allowed', 'error');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.showToastMsg('File must be under 5MB', 'error');
      return;
    }
    this.claimForm.file = file;
    this.claimForm.fileName = file.name;
    this.claimForm.fileSize = (file.size / 1024 / 1024).toFixed(2) + ' MB';
  }
  removeClaimFile() {
    this.claimForm.file = null;
    this.claimForm.fileName = '';
    this.claimForm.fileSize = '';
    this.claimForm.documentUrl = null;
  }

  submitClaim() {
    if (this.submittingClaim) return;
    this.submittingClaim = true;
    this.claimSubmitError = '';

    if (this.claimForm.file && !this.claimForm.documentUrl) {
      this.claimForm.uploading = true;
      const formData = new FormData();
      formData.append('file', this.claimForm.file);

      this.claimService.uploadDocument(formData).subscribe({
        next: res => {
          this.claimForm.documentUrl = res.fileUrl;
          this.claimForm.uploading = false;
          this._executeClaimSubmission();
        },
        error: err => {
          this.claimForm.uploading = false;
          this.submittingClaim = false;
          this.claimSubmitError = err?.error?.message || 'Failed to upload document.';
          this.cdr.markForCheck();
        }
      });
    } else {
      this._executeClaimSubmission();
    }
  }

  private _executeClaimSubmission() {
    let payload: any = { employeeId: this.claimForm.employeeId, documentUrl: this.claimForm.documentUrl };
    let submitObs;

    if (this.claimForm.claimType === 'Health') {
      payload.requestedAmount = this.claimForm.requestedAmount;
      submitObs = this.claimService.submitHealthClaim(payload);
    } else if (this.claimForm.claimType === 'TermLife') {
      submitObs = this.claimService.submitTermLifeClaim(payload);
    } else if (this.claimForm.claimType === 'Accident') {
      payload.accidentType = this.claimForm.accidentType;
      if (payload.accidentType === 'Partial') {
        payload.accidentPercentage = this.claimForm.accidentPercentage;
      }
      submitObs = this.claimService.submitAccidentClaim(payload);
    }

    if (submitObs) {
      submitObs.subscribe({
        next: res => {
          this.openSuccessModal('✅', 'Claim Submitted!', `Claim submitted successfully! A claims manager will review your submission.`, 'claims');
          this.showClaimsWizard = false;
          this.loadMyClaims();
        },
        error: err => {
          this.claimSubmitError = err?.error?.message || 'Failed to submit claim.';
          this.submittingClaim = false;
          this.cdr.markForCheck();
        },
        complete: () => {
          this.submittingClaim = false;
          this.cdr.markForCheck();
        }
      });
    }
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

  getExpiryDate(d: string | Date | undefined | null): string {
    if (!d) return '—';
    const date = new Date(d);
    date.setFullYear(date.getFullYear() + 1);
    return date.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }
  logout() { this.authService.logout(); }
  getAgentInitials(name: string): string {
    if (!name) return '';
    return name.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2);
  }

  isCardExpired(expiryDate: string): boolean {
    if (!expiryDate) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const exp = new Date(expiryDate);
    return exp <= today;
  }
}