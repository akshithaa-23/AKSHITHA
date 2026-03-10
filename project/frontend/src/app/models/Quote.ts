export interface Quote {
    id?: number | string;
    quoteRequestId?: number | string;
    policyId?: number | string;
    policyName?: string;
    employeeCount?: number;
    totalPremium?: number;
    status?: string;
    createdAt?: string;
    sentAt?: string;
    [key: string]: any;
}
