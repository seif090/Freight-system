import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdvancedAnalyticsService } from './advanced-analytics.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-time-to-failure',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './time-to-failure.component.html'
})
export class TimeToFailureComponent implements OnInit {
  riskData: any[] = [];
  error = '';
  @ViewChild('riskChart') riskChart?: ElementRef<HTMLCanvasElement>;
  chartRef: any;

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getMaintenanceRisk().subscribe({
      next: data => {
        this.riskData = data;
        setTimeout(() => this.createChart(), 0);
      },
      error: err => (this.error = err?.message || 'Unable to load risk data')
    });
  }

  createChart(): void {
    if (!this.riskData || this.riskData.length === 0) return;

    if (this.chartRef) this.chartRef.destroy();

    const labels = this.riskData.map(v => v.registrationNumber || `Vehicle ${v.id}`);
    const dueDays = this.riskData.map(v => {
      const due = new Date(v.nextInspectionDue).getTime();
      const now = new Date().getTime();
      return Math.max(0, Math.round((due - now) / (1000 * 60 * 60 * 24)));
    });

    if (this.riskChart?.nativeElement) {
      this.chartRef = new Chart(this.riskChart.nativeElement.getContext('2d')!, {
        type: 'bar',
        data: {
          labels,
          datasets: [{
            label: 'Days until next inspection',
            data: dueDays,
            backgroundColor: 'rgba(255,99,132,0.5)'
          }]
        },
        options: {
          scales: { y: { beginAtZero: true } },
          plugins: { title: { display: true, text: 'Time-to-Failure Risk Chart' } }
        }
      });
    }
  }
}
