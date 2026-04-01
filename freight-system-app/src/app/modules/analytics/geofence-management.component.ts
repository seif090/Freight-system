import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService, Geofence } from './advanced-analytics.service';

@Component({
  selector: 'app-geofence-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './geofence-management.component.html'
})
export class GeofenceManagementComponent implements OnInit {
  geofences: Geofence[] = [];
  newGeofence: Geofence = { name: '', centerLatitude: 0, centerLongitude: 0, radiusMeters: 500, isActive: true };
  error = '';
  message = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
    this.loadGeofences();
  }

  loadGeofences(): void {
    this.analyticsService.getGeofences().subscribe({
      next: data => this.geofences = data,
      error: err => this.error = err?.message || 'Failed to load geofences'
    });
  }

  addGeofence(): void {
    this.analyticsService.addGeofence(this.newGeofence).subscribe({
      next: result => {
        this.message = `Added geofence ${result.name}`;
        this.error = '';
        this.newGeofence = { name: '', centerLatitude: 0, centerLongitude: 0, radiusMeters: 500, isActive: true };
        this.loadGeofences();
      },
      error: err => this.error = err?.message || 'Unable to add geofence'
    });
  }
}
