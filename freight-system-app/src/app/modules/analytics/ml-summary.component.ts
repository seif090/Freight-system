import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-ml-summary',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ml-summary.component.html'
})
export class MlSummaryComponent {
  shipmentId = 0;
  lastAnomaly: any = null;
  lastFill: any = null;
  lastStream: any = null;
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  async runAnomaly() {
    if (!this.shipmentId) { this.error = 'Enter shipment ID'; return; }
    this.analyticsService.delayAnomalyCheck(this.shipmentId).subscribe({
      next: r => { this.lastAnomaly = r; this.error = ''; },
      error: e => this.error = e?.message || 'Anomaly failed'
    });
  }

  async runStream() {
    if (!this.shipmentId) { this.error = 'Enter shipment ID'; return; }
    this.analyticsService.streamHistory(this.shipmentId).subscribe({
      next: r => { this.lastStream = r; this.error = ''; },
      error: e => this.error = e?.message || 'Stream failed'
    });
  }

  async captureWarehouse() {
    this.analyticsService.createWarehouseSnapshot().subscribe({
      next: r => { this.lastFill = r; this.error = ''; },
      error: e => this.error = e?.message || 'Snapshot failed'
    });
  }
}
