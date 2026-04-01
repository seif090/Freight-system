import { Component, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-dispatch-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dispatch-history.component.html'
})
export class DispatchHistoryComponent implements AfterViewInit {
  actions: any[] = [];
  selectedAction: any = null;
  page = 1;
  pageSize = 20;
  total = 0;

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }
  @ViewChild('map') mapContainer?: ElementRef;
  private map?: L.Map;
  private routeLayer?: L.LayerGroup;

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngAfterViewInit(): void {
    this.map = L.map('dispatchMap').setView([30.0, 31.0], 5);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '© OpenStreetMap contributors' }).addTo(this.map);
    this.routeLayer = L.layerGroup().addTo(this.map);
    this.loadActions();
  }

  loadActions(): void {
    this.analyticsService.getDispatchActions(this.page, this.pageSize).subscribe((data: any) => {
      this.actions = data.actions;
      this.total = data.total;
      if (this.actions.length && !this.selectedAction) {
        this.selectAction(this.actions[0]);
      }
    });
  }

  selectAction(action: any): void {
    this.selectedAction = action;
    this.routeLayer?.clearLayers();

    if (action.routeGeoJson) {
      try {
        const geo = JSON.parse(action.routeGeoJson);
        const coords = (geo.coordinates || []).map((c: any) => [c[1], c[0]]);
        const poly = L.polyline(coords as L.LatLngExpression[], { color: 'blue' });
        poly.addTo(this.routeLayer!);
        this.map?.fitBounds(poly.getBounds(), { padding: [20, 20] });
      } catch (error) {
        console.warn('Invalid route GeoJSON', error);
      }
    }
  }

  undo(action: any): void {
    this.analyticsService.undoDispatchAction(action.id).subscribe(() => this.loadActions());
  }

  prevPage(): void {
    if (this.page <= 1) return;
    this.page -= 1;
    this.loadActions();
  }

  nextPage(): void {
    if (this.page * this.pageSize >= this.total) return;
    this.page += 1;
    this.loadActions();
  }
}
