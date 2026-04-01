import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-route-deviation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './route-deviation.component.html'
})
export class RouteDeviationComponent implements OnInit {
  shipmentId = 1;
  currentLat = 30.1;
  currentLon = 31.1;
  plannedSegments = '[{"segmentOrder":1,"startLatitude":30,"startLongitude":31,"endLatitude":30.5,"endLongitude":31.5,"distanceKm":70}]';
  deviationResult: any = null;
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
  }

  checkDeviation(): void {
    try {
      const segments = JSON.parse(this.plannedSegments);
      this.analyticsService.checkDeviation({
        shipmentId: this.shipmentId,
        currentLatitude: this.currentLat,
        currentLongitude: this.currentLon,
        plannedSegments: segments
      }).subscribe({
        next: res => { this.deviationResult = res; this.error = ''; },
        error: err => { this.error = err?.message || 'Deviation check failed'; }
      });
    } catch (ex) {
      this.error = 'Invalid plannedSegments JSON';
    }
  }
}
