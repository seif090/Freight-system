import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Customer, CustomerService } from '../../services/customer.service';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.scss']
})
export class CustomerListComponent implements OnInit {
  customers: Customer[] = [];
  loading = false;
  error = '';

  constructor(private customerService: CustomerService) { }

  ngOnInit(): void {
    this.fetchCustomers();
  }

  fetchCustomers(): void {
    this.loading = true;
    this.customerService.getCustomers().subscribe({
      next: data => {
        this.customers = data;
        this.loading = false;
      },
      error: err => {
        this.error = 'فشل جلب العملاء';
        console.error(err);
        this.loading = false;
      }
    });
  }
}
