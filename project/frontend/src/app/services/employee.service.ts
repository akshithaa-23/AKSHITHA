import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee } from '../models/Employee';

@Injectable({
    providedIn: 'root'
})
export class EmployeeService {
    private readonly apiUrl = 'https://localhost:7173/api/employee';

    constructor(private http: HttpClient) { }

    getMyCompanyEmployees(): Observable<Employee[]> {
        return this.http.get<Employee[]>(`${this.apiUrl}/my-company`);
    }

    addEmployee(payload: any): Observable<any> {
        return this.http.post<any>(this.apiUrl, payload);
    }

    updateEmployee(id: any, payload: any): Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/${id}`, payload);
    }

    deactivateEmployee(id: any): Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/${id}/deactivate`, {});
    }
}
