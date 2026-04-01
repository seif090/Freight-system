import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from './analytics.service';

interface DelayRegressionSample {
  hours: number;
  delay: number;
}

interface DelayRegressionResult {
  slope: number;
  intercept: number;
  rSquared: number;
  forecastDelayMinutes: number;
  sampleSize: number;
  samples: DelayRegressionSample[];
}

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html'
})
export class AnalyticsComponent implements OnInit {
  summary: any = null;
  llmSpendTrend: any = null;
  delayRisk: any = null;
  delayRegression: DelayRegressionResult | null = null;
  error = '';

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getSummary().subscribe({
      next: data => this.summary = data,
      error: err => this.error = err?.message || 'Failed to fetch analytics'
    });

    this.analyticsService.getLlmSpendTrend().subscribe({
      next: data => this.llmSpendTrend = data,
      error: err => this.error = err?.message || 'Failed to fetch LLM spend trend'
    });

    this.analyticsService.getDelayRiskForecast().subscribe({
      next: data => this.delayRisk = data,
      error: err => this.error = err?.message || 'Failed to fetch delay risk forecast'
    });

    this.analyticsService.getDelayRegression().subscribe({
      next: data => this.delayRegression = data,
      error: err => this.error = err?.message || 'Failed to fetch delay regression'
    });
  }
}
