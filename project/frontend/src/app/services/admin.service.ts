import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/User';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private readonly apiUrl = 'https://localhost:7173/api/Admin';

    constructor(private http: HttpClient) { }

    getUsers(): Observable<User[]> {
        return this.http.get<User[]>(`${this.apiUrl}/users`);
    }

    registerUser(userDto: any): Observable<any> {
        return this.http.post(`${this.apiUrl}/register-user`, userDto);
    }
}
