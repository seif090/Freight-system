import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Geofence {
  id?: number;
  name: string;
  centerLatitude: number;
  centerLongitude: number;
  radiusMeters: number;
  isActive: boolean;
  tenantId?: string;
}

export interface RouteSegment {
  id?: number;
  shipmentId: number;
  segmentOrder: number;
  startLatitude: number;
  startLongitude: number;
  endLatitude: number;
  endLongitude: number;
  distanceKm: number;
  durationMinutes: number;
  res?: any;
}

export interface WarehouseFact {
  id: number;
  shipmentId: number;
  trackingNumber: string;
  routeKey: string;
  status: string;
  origin: string;
  destination: string;
  etd: string;
  eta: string;
  factDate: string;
  isDelayAnomaly: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdvancedAnalyticsService {
  private readonly baseUrl = 'https://localhost:5001/api/v1.0/advancedanalytics';

  constructor(private http: HttpClient) {}

  getGeofences(): Observable<Geofence[]> {
    return this.http.get<Geofence[]>(`${this.baseUrl}/geofences`);
  }

  addGeofence(geofence: Geofence): Observable<Geofence> {
    return this.http.post<Geofence>(`${this.baseUrl}/geofences`, geofence);
  }

  geofenceCheck(shipmentId: number) {
    return this.http.get(`${this.baseUrl}/shipments/${shipmentId}/geofence-check`);
  }

  addRouteSegment(shipmentId: number, segment: RouteSegment) {
    return this.http.post(`${this.baseUrl}/shipments/${shipmentId}/segments`, segment);
  }

  delayAnomalyCheck(shipmentId: number) {
    return this.http.post(`${this.baseUrl}/shipments/${shipmentId}/delay-anomaly-check`, {});
  }

  streamHistory(shipmentId: number) {
    return this.http.post(`${this.baseUrl}/shipments/${shipmentId}/stream-history`, {});
  }

  createWarehouseSnapshot() {
    return this.http.post(`${this.baseUrl}/warehouse/snapshot`, {});
  }

  getVehicles(tenantId: string = 'default') {
    return this.http.get<any[]>(`https://localhost:5001/api/v1.0/advancedoperations/vehicles?tenantId=${tenantId}`);
  }

  getMaintenanceRisk(tenantId: string = 'default') {
    return this.http.get<any[]>(`https://localhost:5001/api/v1.0/advancedoperations/vehicles/maintenance-risk?tenantId=${tenantId}`);
  }

  optimizeRoute(shipmentId: number, segments: any[]) {
    return this.http.post(`https://localhost:5001/api/v1.0/advancedoperations/shipments/${shipmentId}/optimize-route`, segments);
  }

  dispatchRoute(shipmentId: number, payload: { instruction: string; routePreviewUrl: string; routeGeoJson?: string; priority: string; markDispatched: boolean }) {
    return this.http.patch(`https://localhost:5001/api/v1.0/advancedoperations/shipments/${shipmentId}/dispatch`, payload);
  }

  getDispatchActions(page: number = 1, pageSize: number = 20) {
    return this.http.get<any>(`https://localhost:5001/api/v1.0/advancedoperations/dispatch-actions?page=${page}&pageSize=${pageSize}`);
  }

  undoDispatchAction(actionId: number) {
    return this.http.patch(`https://localhost:5001/api/v1.0/advancedoperations/dispatch-actions/${actionId}/undo`, {});
  }

  scheduleMaintenance(request: { shipmentId: number; vehicleId: number; maintenanceDate: string; description: string }) {
    return this.http.post(`https://localhost:5001/api/v1.0/advancedoperations/schedule-maintenance`, request);
  }

  getWarehouseFacts(limit: number = 200) {
    return this.http.get<WarehouseFact[]>(`${this.baseUrl}/warehouse/facts?limit=${limit}`);
  }
}
