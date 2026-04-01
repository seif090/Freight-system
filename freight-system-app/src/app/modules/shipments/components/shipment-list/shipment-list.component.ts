import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Shipment, ShipmentService } from '../../services/shipment.service';

@Component({
  selector: 'app-shipment-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './shipment-list.component.html',
  styleUrls: ['./shipment-list.component.scss']
})
export class ShipmentListComponent implements OnInit {
  shipments: Shipment[] = [];
  loading = false;
  error = '';

  constructor(private shipmentService: ShipmentService) { }

  ngOnInit(): void {
    this.fetchShipments();
  }

  fetchShipments() {
    this.loading = true;
    this.shipmentService.getShipments().subscribe({
      next: (data) => {
        this.shipments = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'فشل تحميل الشحنات';
        console.error(err);
        this.loading = false;
      }
    });
  }
}
