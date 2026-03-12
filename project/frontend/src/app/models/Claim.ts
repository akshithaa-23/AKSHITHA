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
    accidentDate?: string;
    // Accident claim enriched fields
    daysSinceAccident?: number;
    claimDeadline?: string;
    firDocumentUrl?: string;
    hospitalReportUrl?: string;
    salary?: number;
    department?: string;
    claimsManagerName?: string;
    ageFactor?: number;
    frequencyFactor?: number;
    finalApprovedAmount?: number;
    employeeAge?: number;
    claimNumberInYear?: number;
    dateOfDeath?: string;
    causeOfDeath?: string;
    causeOfDeathDescription?: string;
    normalPayout?: number;
    adjustedPayout?: number;
    suicideExclusionFlag?: boolean;
    daysInCompany?: number;
}

