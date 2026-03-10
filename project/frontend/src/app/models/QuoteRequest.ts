export interface QuoteRequest {
    id?: number | string;
    companyName?: string;
    numberOfEmployees?: number;
    policyId?: number | string;
    requestType?: string;
    status?: string;
    customerName?: string;
    autoTierLabel?: string;
    [key: string]: any;
}
