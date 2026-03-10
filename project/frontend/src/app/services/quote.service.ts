import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Quote } from '../models/Quote';

@Injectable({
    providedIn: 'root'
})
export class QuoteService {
    private readonly apiUrl = 'https://localhost:7173/api/quote';

    constructor(private http: HttpClient) { }

    getAgentQuotes(): Observable<Quote[]> {
        return this.http.get<Quote[]>(`${this.apiUrl}/agent`);
    }

    getMyQuotes(): Observable<Quote[]> {
        return this.http.get<Quote[]>(`${this.apiUrl}/my`);
    }

    sendQuote(payload: any): Observable<any> {
        return this.http.post<any>(this.apiUrl, payload);
    }

    acceptQuote(id: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/accept`, {});
    }

    rejectQuote(id: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/reject`, {});
    }
}
