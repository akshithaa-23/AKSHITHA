import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Payment } from '../models/Payment';

@Injectable({
    providedIn: 'root'
})
export class PaymentService {
    private readonly apiUrl = 'https://localhost:7173/api/payment';

    constructor(private http: HttpClient) { }

    getAgentCommissions(): Observable<Payment[]> {
        return this.http.get<Payment[]>(`${this.apiUrl}/agent`);
    }

    getMyPayments(): Observable<Payment[]> {
        return this.http.get<Payment[]>(`${this.apiUrl}/my`);
    }

    processPayment(payload: any): Observable<any> {
        return this.http.post<any>(this.apiUrl, payload);
    }
}
