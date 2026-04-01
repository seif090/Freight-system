import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Shipment {
  id: number;
  trackingNumber: string;
  type: 'Import' | 'Export' | 'Domestic';
  mode: 'Sea' | 'Air' | 'Land';
  status: 'Pending' | 'InTransit' | 'Delivered' | 'Cancelled';
  portOfLoading?: string;
  portOfDischarge?: string;
  etd?: string;
  eta?: string;
  containerType?: 'None' | 'TwentyFt' | 'FortyFt' | 'LCL' | 'FCL';
  vesselOrFlightNumber?: string;
  customerId?: number;
  supplierId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ShipmentService {
  private readonly apiUrl = 'https://localhost:5001/api/v1.0/shipments';

  constructor(private http: HttpClient) { }

  getShipments(): Observable<Shipment[]> {
    return this.http.get<Shipment[]>(this.apiUrl);
  }

  getShipment(id: number): Observable<Shipment> {
    return this.http.get<Shipment>(`${this.apiUrl}/${id}`);
  }

  createShipment(shipment: Partial<Shipment>): Observable<Shipment> {
    return this.http.post<Shipment>(this.apiUrl, shipment);
  }

  updateShipment(id: number, shipment: Partial<Shipment>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, shipment);
  }

  deleteShipment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
