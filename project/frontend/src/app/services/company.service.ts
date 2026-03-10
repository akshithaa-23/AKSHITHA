import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Company } from '../models/Company';

@Injectable({
    providedIn: 'root'
})
export class CompanyService {
    private readonly apiUrl = 'https://localhost:7173/api/Company';

    constructor(private http: HttpClient) { }

    getAllCompanies(): Observable<Company[]> {
        return this.http.get<Company[]>(`${this.apiUrl}/all`);
    }

    getCompaniesByAgent(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/by-agent`);
    }

    getMyCompany(): Observable<Company> {
        return this.http.get<Company>(`${this.apiUrl}/my`);
    }
}
