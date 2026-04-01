import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService, RouteSegment } from './advanced-analytics.service';

@Component({
  selector: 'app-eta-risk-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './eta-risk-dashboard.component.html'
})
export class EtaRiskDashboardComponent {
  shipmentId = 0;
  segment: RouteSegment = { shipmentId: 0, segmentOrder: 1, startLatitude: 0, startLongitude: 0, endLatitude: 0, endLongitude: 0, distanceKm: 1, durationMinutes: 10 };
  anomalyResult: any = null;
  streamResult: any = null;
  addedSegment: any = null;
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  createSegment() {
    if (this.shipmentId <= 0) {
      this.error = 'Enter shipment ID';
      return;
    }
    this.segment.shipmentId = this.shipmentId;
    this.analyticsService.addRouteSegment(this.shipmentId, this.segment).subscribe({
      next: value => {
        this.addedSegment = value;
        this.error = '';
      },
      error: err => this.error = err?.message || 'Failed to add segment'
    });
  }

  checkAnomaly() {
    if (this.shipmentId <= 0) { this.error = 'Enter shipment ID'; return; }
    this.analyticsService.delayAnomalyCheck(this.shipmentId).subscribe({
      next: val => {
        this.anomalyResult = val;
        this.error = '';
      },
      error: err => this.error = err?.message || 'Anomaly check failed'
    });
  }

  streamHistory() {
    if (this.shipmentId <= 0) { this.error = 'Enter shipment ID'; return; }
    this.analyticsService.streamHistory(this.shipmentId).subscribe({
      next: val => {
        this.streamResult = val;
        this.error = '';
      },
      error: err => this.error = err?.message || 'Stream failure'
    });
  }
}
