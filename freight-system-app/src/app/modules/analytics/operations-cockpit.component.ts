import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import Chart from 'chart.js/auto';
import { AdvancedAnalyticsService } from './advanced-analytics.service';
import { ShipmentService } from '../shipments/services/shipment.service';
import { SignalrService } from '../../core/signalr.service';

@Component({
  selector: 'app-operations-cockpit',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './operations-cockpit.component.html'
})
export class OperationsCockpitComponent implements OnInit {
  vehicleCount = 0;
  maintenanceRiskCount = 0;
  shipmentCount = 0;
  etaChartData: any = null;
  alerts: string[] = [];
  @ViewChild('etaCanvas') etaCanvas?: ElementRef<HTMLCanvasElement>;
  etaChart: any;

  constructor(
    private analyticsService: AdvancedAnalyticsService,
    private shipmentService: ShipmentService,
    private signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.loadMetrics();
    this.signalr.shipmentUpdated$.subscribe(update => {
      this.alerts.unshift(`Realtime event: ${update.trackingNumber || update.shipmentId || 'Shipment'} updated.`);
      if (this.alerts.length > 10) this.alerts.pop();
    });
  }

  loadMetrics(): void {
    this.analyticsService.getVehicles().subscribe({
      next: vehicles => {
        this.vehicleCount = (vehicles as any[]).length;
      }
    });

    this.analyticsService.getMaintenanceRisk().subscribe({
      next: risky => {
        this.maintenanceRiskCount = (risky as any[]).length;
        this.setupEtaChart(risky as any[]);
      }
    });

    this.shipmentService.getShipments().subscribe({
      next: shipments => {
        this.shipmentCount = shipments.length;
      }
    });
  }

  setupEtaChart(riskyVehicles: any[]): void {
    const labels = riskyVehicles.map(v => v.registrationNumber || `V${v.id}`);
    const values = riskyVehicles.map(v => {
      const due = new Date(v.nextInspectionDue).getTime();
      const now = new Date().getTime();
      return Math.max(0, Math.round((due - now) / (1000 * 60 * 60 * 24)));
    });

    if (this.etaChart) this.etaChart.destroy();
    if (!this.etaCanvas) return;

    this.etaChart = new Chart(this.etaCanvas.nativeElement.getContext('2d')!, {
      type: 'line',
      data: {
        labels,
        datasets: [{
            label: 'Days until inspection',
            data: values,
            borderColor: '#e76f51',
            backgroundColor: 'rgba(231,111,81,0.3)',
            fill: true
        }]
      },
      options: {
        responsive: true,
        plugins: { title: { display: true, text: 'ETA Risk: Maintenance window (days)' }},
        scales: { y: { beginAtZero: true }}
      }
    });
  }
}
