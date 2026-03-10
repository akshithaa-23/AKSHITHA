export interface Company {
    id?: number | string;
    companyName: string;
    representativeName?: string;
    representativeEmail?: string;
    size?: number;
    activePolicy?: any;
    domain?: string;
    customerName?: string;
    agentName?: string;
    createdAt?: string | Date;
    [key: string]: any;
}
