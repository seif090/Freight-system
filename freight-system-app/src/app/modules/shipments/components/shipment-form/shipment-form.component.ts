import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ShipmentService, Shipment } from '../../services/shipment.service';
import { Customer, CustomerService } from '../../../customers/services/customer.service';

@Component({
  selector: 'app-shipment-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './shipment-form.component.html',
  styleUrls: ['./shipment-form.component.scss']
})
export class ShipmentFormComponent implements OnInit {
  shipment: Partial<Shipment> = {
    trackingNumber: '',
    type: 'Import',
    mode: 'Sea',
    status: 'Pending',
    portOfLoading: '',
    portOfDischarge: '',
    containerType: 'None',
    vesselOrFlightNumber: ''
  };
  customers: Customer[] = [];
  isEdit = false;
  error = '';

  constructor(
    private shipmentService: ShipmentService,
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadCustomers();

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.shipmentService.getShipment(id).subscribe({
        next: data => this.shipment = data,
        error: () => this.error = 'فشل تحميل الشحنة.'
      });
    }
  }

  loadCustomers(): void {
    this.customerService.getCustomers().subscribe({
      next: data => this.customers = data,
      error: () => this.error = 'فشل تحميل قائمة العملاء.'
    });
  }

  save(): void {
    if (this.isEdit && this.shipment.id) {
      this.shipmentService.updateShipment(this.shipment.id, this.shipment).subscribe({
        next: () => this.router.navigate(['/shipments']),
        error: () => this.error = 'فشل تحديث الشحنة.'
      });
    } else {
      this.shipmentService.createShipment(this.shipment).subscribe({
        next: () => this.router.navigate(['/shipments']),
        error: () => this.error = 'فشل إنشاء الشحنة.'
      });
    }
  }
}
