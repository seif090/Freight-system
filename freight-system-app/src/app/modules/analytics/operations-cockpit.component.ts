import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import Chart from 'chart.js/auto';
import { AdvancedAnalyticsService } from './advanced-analytics.service';
import { ShipmentService } from '../shipments/services/shipment.service';
import { SignalrService } from '../../core/signalr.service';

interface DispatchActionItem {
  shipmentId: number;
  instruction: string;
  routePreviewUrl: string;
  priority: 'Critical' | 'High' | 'Normal' | 'Low';
  dispatched: boolean;
  createdAt: string;
  dispatchedAt?: string;
}

@Component({
  selector: 'app-operations-cockpit',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
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

  manualShipmentId = 0;
  manualRouteJson = JSON.stringify([
    { segmentOrder: 1, startLatitude: 30.0, startLongitude: 31.0, endLatitude: 31.0, endLongitude: 32.0, distanceKm: 100 }
  ], null, 2);
  actionItems: DispatchActionItem[] = [];
  optimizing = false;
  toastMessage = '';
  toastType: 'success' | 'danger' = 'success';

  constructor(
    private analyticsService: AdvancedAnalyticsService,
    private shipmentService: ShipmentService,
    private signalr: SignalrService
  ) {}

  ngOnInit(): void {
    this.loadMetrics();
    this.loadActionItems();
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

  manualOptimizeRoute(): void {
    if (!this.manualShipmentId || !this.manualRouteJson) {
      this.toast('Specify shipment ID and route payload before optimization.', 'danger');
      return;
    }

    let segments;
    try {
      segments = JSON.parse(this.manualRouteJson);
    } catch (error) {
      this.toast('Route JSON is invalid.', 'danger');
      return;
    }

    this.optimizing = true;
    this.analyticsService.optimizeRoute(this.manualShipmentId, segments).subscribe({
      next: (result: any) => {
        this.optimizing = false;
        const now = new Date().toLocaleTimeString();
        const routePreviewUrl = `https://www.google.com/maps/dir/${segments.map((seg: any) => `${seg.startLatitude},${seg.startLongitude}`).join('/')}/${segments.slice(-1)[0].endLatitude},${segments.slice(-1)[0].endLongitude}`;
        const humanAction = `Reroute ${this.manualShipmentId}: apply optimized route (${result.optimizedTrajectory?.length || 0} segments).`;

        const actionItem: DispatchActionItem = {
          shipmentId: this.manualShipmentId,
          instruction: humanAction,
          routePreviewUrl,
          priority: 'High',
          dispatched: false,
          createdAt: new Date().toISOString()
        };

        this.actionItems.unshift(actionItem);
        this.saveActionItems();

        this.analyticsService.dispatchRoute(this.manualShipmentId, {
          instruction: humanAction,
          routePreviewUrl,
          priority: 'High',
          markDispatched: false
        }).subscribe({
          next: () => this.toast('Manual command created successfully.', 'success'),
          error: err => this.toast(`Dispatch endpoint error: ${err?.message || 'unknown'}`, 'danger')
        });
      },
      error: err => {
        this.optimizing = false;
        this.toast(`Optimization failed: ${err?.message || 'unknown error'}`, 'danger');
      }
    });
  }

  dispatchAsap(index: number): void {
    const item = this.actionItems[index];
    if (!item) return;

    item.dispatched = true;
    item.priority = 'Critical';
    item.dispatchedAt = new Date().toISOString();
    this.toast(`Dispatched ASAP: ${item.instruction}`, 'success');

    this.saveActionItems();

    this.analyticsService.dispatchRoute(item.shipmentId, {
      instruction: item.instruction,
      routePreviewUrl: item.routePreviewUrl || '',
      priority: item.priority,
      markDispatched: true
    }).subscribe({
      next: () => {},
      error: err => this.toast(`Dispatch patch error: ${err?.message || 'unknown'}`, 'danger')
    });

    this.sortActions();
  }

  private toast(message: string, type: 'success' | 'danger' = 'success'): void {
    this.toastMessage = message;
    this.toastType = type;
    setTimeout(() => this.toastMessage = '', 4000);
  }

  private loadActionItems(): void {
    const raw = localStorage.getItem('operationsCockpitActions');
    if (!raw) return;
    try {
      const parsed = JSON.parse(raw) as DispatchActionItem[];
      this.actionItems = parsed.map(x => ({
        ...x,
        priority: x.priority || 'Normal',
        dispatched: x.dispatched || false
      }));
    } catch {
      this.actionItems = [];
    }
    this.sortActions();
  }

  private saveActionItems(): void {
    localStorage.setItem('operationsCockpitActions', JSON.stringify(this.actionItems));
  }

  private sortActions(): void {
    const p: Record<DispatchActionItem['priority'], number> = { Critical: 0, High: 1, Normal: 2, Low: 3 };
    this.actionItems.sort((a, b) => {
      return (p[a.priority] || 99) - (p[b.priority] || 99);
    });
  }
}


