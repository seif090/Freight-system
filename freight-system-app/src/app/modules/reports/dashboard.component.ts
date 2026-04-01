import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReportsService } from './reports.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class DashboardComponent implements OnInit {
  @ViewChild('statusCanvas') statusCanvas?: ElementRef<HTMLCanvasElement>;
  @ViewChild('modeCanvas') modeCanvas?: ElementRef<HTMLCanvasElement>;

  dashboardData: any = null;
  error = '';
  statusChart: any;
  modeChart: any;

  constructor(private reportsService: ReportsService) {}

  ngOnInit(): void {
    this.reportsService.getDashboardData().subscribe({
      next: data => {
        this.dashboardData = data;
        setTimeout(() => this.createCharts(), 0);
      },
      error: err => (this.error = err?.message || 'فشل تحميل البيانات')
    });
  }

  exportShipments(format: string): void {
    const url = `/api/v1.0/reports/export/shipments?format=${format}`;
    window.open(url, '_blank');
  }

  getStatusKeys() {
    return this.dashboardData?.shipmentsPerStatus ? Object.keys(this.dashboardData.shipmentsPerStatus) : [];
  }

  getModeKeys() {
    return this.dashboardData?.shipmentsPerMode ? Object.keys(this.dashboardData.shipmentsPerMode) : [];
  }

  private createCharts() {
    if (!this.dashboardData) return;

    if (this.statusChart) {
      this.statusChart.destroy();
    }
    if (this.modeChart) {
      this.modeChart.destroy();
    }

    const statusLabels = this.getStatusKeys();
    const statusData = statusLabels.map((k: string) => this.dashboardData.shipmentsPerStatus[k]);

    const modeLabels = this.getModeKeys();
    const modeData = modeLabels.map((k: string) => this.dashboardData.shipmentsPerMode[k]);

    if (this.statusCanvas) {
      this.statusChart = new Chart(this.statusCanvas.nativeElement.getContext('2d')!, {
        type: 'doughnut',
        data: {
          labels: statusLabels,
          datasets: [{
            data: statusData,
            backgroundColor: ['#2a9d8f', '#e9c46a', '#f4a261', '#e76f51']
          }]
        },
        options: {
          responsive: true,
          plugins: {
            title: { display: true, text: 'Shipments by Status' }
          }
        }
      });
    }

    if (this.modeCanvas) {
      this.modeChart = new Chart(this.modeCanvas.nativeElement.getContext('2d')!, {
        type: 'bar',
        data: {
          labels: modeLabels,
          datasets: [{
            label: 'Count',
            data: modeData,
            backgroundColor: '#264653'
          }]
        },
        options: {
          responsive: true,
          plugins: {
            title: { display: true, text: 'Shipments by Mode' }
          },
          onClick: (e: any, elements: any[]) => {
            if (elements.length) {
              const index = elements[0].index;
              const name = modeLabels[index];
              alert(`Filter by mode: ${name}`);
            }
          }
        }
      });
    }
  }
}
