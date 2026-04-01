import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-route-optimizer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './route-optimizer.component.html'
})
export class RouteOptimizerComponent {
  shipmentId = 0;
  routeJson = '[{"segmentOrder":1,"startLatitude":30.0,"startLongitude":31.0,"endLatitude":31.0,"endLongitude":32.0,"distanceKm":100}]';
  result: any = null;
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  optimize(): void {
    try {
      const segments = JSON.parse(this.routeJson);
      this.analyticsService.optimizeRoute(this.shipmentId, segments).subscribe({
        next: data => { this.result = data; this.error = ''; },
        error: err => { this.error = err?.message || 'Optimization failed'; }
      });
    } catch (ex) {
      this.error = 'Invalid JSON for route segments';
    }
  }
}
