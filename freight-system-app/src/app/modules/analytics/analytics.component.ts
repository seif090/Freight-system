import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from './analytics.service';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html'
})
export class AnalyticsComponent implements OnInit {
  summary: any = null;
  error = '';

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getSummary().subscribe({
      next: data => this.summary = data,
      error: err => this.error = err?.message || 'Failed to fetch analytics'
    });
  }
}
