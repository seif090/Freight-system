import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-vehicle-fleet-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vehicle-fleet-dashboard.component.html'
})
export class VehicleFleetDashboardComponent implements OnInit {
  vehicles: any[] = [];
  tenantId = 'default';
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles(): void {
    this.analyticsService.getVehicles(this.tenantId).subscribe({
      next: data => { this.vehicles = data; this.error = ''; },
      error: err => { this.error = err?.message || 'Failed to load vehicles'; }
    });
  }

  refresh(): void {
    this.loadVehicles();
  }
}
