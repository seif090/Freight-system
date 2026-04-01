import { Component, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { MapService } from './map.service';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements AfterViewInit {
  private map?: L.Map;

  constructor(private mapService: MapService) {}

  ngAfterViewInit(): void {
    this.map = L.map('map').setView([30.0444, 31.2357], 5);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    this.loadShipments();

    setInterval(() => this.loadShipments(), 15000);
  }

  private loadShipments(): void {
    if (!this.map) return;

    this.mapService.getLiveLocations().subscribe((shipments: any[]) => {
      shipments.forEach(sh => {
        if (sh.currentLatitude && sh.currentLongitude) {
          L.circleMarker([sh.currentLatitude, sh.currentLongitude], {
            radius: 6,
            color: sh.priority === 'Critical' ? 'red' : sh.priority === 'High' ? 'orange' : 'green'
          }).bindPopup(`<b>${sh.trackingNumber}</b><br>Status: ${sh.status}<br>Priority: ${sh.priority}`).addTo(this.map!);
        }
      });
    });
  }
}
