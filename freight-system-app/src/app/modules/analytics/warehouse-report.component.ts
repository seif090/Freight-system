import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedAnalyticsService, WarehouseFact } from './advanced-analytics.service';

@Component({
  selector: 'app-warehouse-report',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './warehouse-report.component.html'
})
export class WarehouseReportComponent implements OnInit {
  facts: WarehouseFact[] = [];
  message = '';
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
    this.loadFacts();
  }

  loadFacts(): void {
    this.analyticsService.getWarehouseFacts().subscribe({
      next: data => this.facts = data,
      error: err => this.error = err?.message || 'Failed to load warehouse facts'
    });
  }

  createSnapshot(): void {
    this.analyticsService.createWarehouseSnapshot().subscribe({
      next: (result: any) => {
        this.message = `Snapshot loaded with ${result.inserted} records`;
        this.error = '';
        this.loadFacts();
      },
      error: err => this.error = err?.message || 'Unable to create snapshot'
    });
  }
}
