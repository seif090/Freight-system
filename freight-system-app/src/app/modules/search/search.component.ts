import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SearchService } from './search.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html'
})
export class SearchComponent {
  query = '';
  shipments: any[] = [];
  customers: any[] = [];
  error = '';

  constructor(private searchService: SearchService) {}

  doSearch() {
    this.error = '';
    this.searchService.searchShipments(this.query).subscribe({
      next: (data) => (this.shipments = data),
      error: (err) => (this.error = err?.message || 'Search shipments failed.')
    });

    this.searchService.searchCustomers(this.query).subscribe({
      next: (data) => (this.customers = data),
      error: (err) => (this.error = err?.message || 'Search customers failed.')
    });
  }
}
