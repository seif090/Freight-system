import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private readonly apiUrl = 'https://localhost:5001/api/v1.0/search';

  constructor(private http: HttpClient) {}

  searchShipments(query: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/shipments?q=${encodeURIComponent(query)}`);
  }

  searchCustomers(query: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/customers?q=${encodeURIComponent(query)}`);
  }
}
