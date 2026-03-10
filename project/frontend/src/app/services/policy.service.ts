import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Policy } from '../models/Policy';

@Injectable({
    providedIn: 'root'
})
export class PolicyService {
    private readonly apiUrl = 'https://localhost:7173/api/Policy';

    constructor(private http: HttpClient) { }

    getAllPolicies(): Observable<Policy[]> {
        return this.http.get<Policy[]>(`${this.apiUrl}/all`);
    }

    getPolicies(): Observable<Policy[]> {
        return this.http.get<Policy[]>(this.apiUrl);
    }

    createPolicy(policy: Policy): Observable<any> {
        return this.http.post(this.apiUrl, policy);
    }

    updatePolicy(id: any, policy: Policy): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, policy);
    }

    deletePolicy(id: any): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }
}
