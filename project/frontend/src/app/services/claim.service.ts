import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Claim } from '../models/Claim';

@Injectable({
    providedIn: 'root'
})
export class ClaimService {
    private readonly apiUrl = 'https://localhost:7173/api/claim';

    constructor(private http: HttpClient) { }

    getAllClaims(): Observable<Claim[]> {
        return this.http.get<Claim[]>(`${this.apiUrl}/all`);
    }

    getManagerClaims(): Observable<Claim[]> {
        return this.http.get<Claim[]>(`${this.apiUrl}/manager`);
    }

    getMyClaims(): Observable<Claim[]> {
        return this.http.get<Claim[]>(`${this.apiUrl}/my`);
    }

    getAllowedTypes(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/allowed-types`);
    }

    processClaim(id: any, decision: string, note: string): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/process`, { decision, note });
    }

    uploadDocument(formData: FormData): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/upload-document`, formData);
    }

    submitHealthClaim(payload: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/health`, payload);
    }

    submitTermLifeClaim(payload: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/term-life`, payload);
    }

    submitAccidentClaim(payload: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/accident`, payload);
    }
}
