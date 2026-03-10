import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { QuoteRequest } from '../models/QuoteRequest';

@Injectable({
    providedIn: 'root'
})
export class QuoteRequestService {
    private readonly apiUrl = 'https://localhost:7173/api/quoterequest';

    constructor(private http: HttpClient) { }

    getAgentRequests(): Observable<QuoteRequest[]> {
        return this.http.get<QuoteRequest[]>(`${this.apiUrl}/agent`);
    }

    submitDirectBuy(payload: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/direct-buy`, payload);
    }

    requestRecommendation(payload: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/recommendation`, payload);
    }
}
