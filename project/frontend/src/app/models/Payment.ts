export interface Payment {
    id?: number | string;
    invoiceNumber?: string;
    policyName?: string;
    companyName?: string;
    employeeCount?: number;
    amountPaid?: number;
    paymentMethod?: string;
    maskedCardNumber?: string;
    cardHolderName?: string;
    paidAt?: string;
    agentName?: string;
    commissionAmount?: number;
    commissionRate?: number;
    earnedAt?: string;
    [key: string]: any;
}
