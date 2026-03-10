export interface Claim {
    id?: number | string;
    employeeId?: number | string;
    employeeCode?: string;
    employeeName?: string;
    companyName?: string;
    claimType: string;
    requestedAmount?: number;
    claimAmount?: number;
    amount?: number;
    status: string;
    description?: string;
    filedDate?: string;
    createdAt?: string;
    processedAt?: string | Date;
    documentUrl?: string;
    nomineeDetails?: string;
    accidentType?: string;
    accidentPercentage?: number;
    salary?: number;
    department?: string;
    claimsManagerName?: string;
}
