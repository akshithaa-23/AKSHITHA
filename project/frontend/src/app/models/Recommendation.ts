export interface Recommendation {
    id?: number | string;
    quoteRequestId?: number | string;
    agentMessage?: string;
    numberOfEmployees?: number;
    status?: string;
    createdAt?: string;
    [key: string]: any;
}
