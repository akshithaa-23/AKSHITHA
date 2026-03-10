import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Recommendation } from '../models/Recommendation';

@Injectable({
    providedIn: 'root'
})
export class RecommendationService {
    private readonly apiUrl = 'https://localhost:7173/api/recommendation';

    constructor(private http: HttpClient) { }

    getAgentRecommendations(): Observable<Recommendation[]> {
        return this.http.get<Recommendation[]>(`${this.apiUrl}/agent`);
    }

    getMyRecommendations(): Observable<Recommendation[]> {
        return this.http.get<Recommendation[]>(`${this.apiUrl}/customer`);
    }

    sendRecommendation(payload: any): Observable<any> {
        return this.http.post<any>(this.apiUrl, payload);
    }

    ignoreRecommendation(id: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/ignore`, {});
    }
}
