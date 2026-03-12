export interface Employee {
    id?: number | string;
    employeeCode?: string;
    fullName?: string;
    email?: string;
    gender?: string;
    salary?: number;
    nomineeName?: string;
    nomineeRelationship?: string;
    nomineePhone?: string;
    isActive?: boolean;
    hasPendingClaim?: boolean;
    [key: string]: any;
}
