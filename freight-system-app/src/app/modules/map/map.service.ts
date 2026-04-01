import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class MapService {
  private readonly apiUrl = 'https://localhost:5001/api/v1.0/shipments';

  constructor(private http: HttpClient) {}

  getLiveLocations(): Observable<any> {
    return this.http.get(`${this.apiUrl}`);
  }
}
