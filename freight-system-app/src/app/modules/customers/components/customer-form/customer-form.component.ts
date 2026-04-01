import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Customer, CustomerService } from '../../services/customer.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-form.component.html',
  styleUrls: ['./customer-form.component.scss']
})
export class CustomerFormComponent implements OnInit {
  customer: Partial<Customer> = {
    name: '',
    email: '',
    phone: '',
    address: '',
    creditLimit: 0,
    balance: 0
  };
  isEdit = false;
  error = '';

  constructor(
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.isEdit = true;
      this.customerService.getCustomer(id).subscribe({
        next: (data) => this.customer = data,
        error: (err) => this.error = 'لم يتم تحميل بيانات العميل.'
      });
    }
  }

  save(): void {
    if (this.isEdit && this.customer.id) {
      this.customerService.updateCustomer(this.customer.id, this.customer).subscribe({
        next: () => this.router.navigate(['/customers']),
        error: () => this.error = 'فشل تحديث العميل.'
      });
    } else {
      this.customerService.createCustomer(this.customer).subscribe({
        next: () => this.router.navigate(['/customers']),
        error: () => this.error = 'فشل إنشاء العميل.'
      });
    }
  }
}
