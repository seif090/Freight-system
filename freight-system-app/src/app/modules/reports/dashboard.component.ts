import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportsService } from './reports.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  standalone: true,
  imports: [CommonModule]
})
export class DashboardComponent implements OnInit {
  dashboardData: any = null;
  error = '';

  constructor(private reportsService: ReportsService) {}

  ngOnInit(): void {
    this.reportsService.getDashboardData().subscribe({
      next: data => (this.dashboardData = data),
      error: err => (this.error = err?.message || 'فشل تحميل البيانات')
    });
  }

  getStatusKeys() {
    return this.dashboardData?.shipmentsPerStatus ? Object.keys(this.dashboardData.shipmentsPerStatus) : [];
  }

  getModeKeys() {
    return this.dashboardData?.shipmentsPerMode ? Object.keys(this.dashboardData.shipmentsPerMode) : [];
  }
}
