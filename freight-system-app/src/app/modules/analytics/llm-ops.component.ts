import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsService } from './analytics.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-llm-ops',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './llm-ops.component.html'
})
export class LlmOpsComponent implements OnInit {
  @ViewChild('tokenCostChart') tokenCostChart?: ElementRef<HTMLCanvasElement>;
  @ViewChild('riskTrendChart') riskTrendChart?: ElementRef<HTMLCanvasElement>;
  @ViewChild('regressionChart') regressionChart?: ElementRef<HTMLCanvasElement>;

  llmSpendData: any;
  delayRisk: any;
  delayRegression: any;

  tokenCostChartInstance: any;
  riskTrendChartInstance: any;
  regressionChartInstance: any;

  error = '';

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.analyticsService.getLlmSpendTrend().subscribe({
      next: data => {
        this.llmSpendData = data;
        setTimeout(() => this.drawTokenCostChart(), 0);
      },
      error: err => this.error = err?.message || 'Failed to load LLM spend'
    });

    this.analyticsService.getDelayRiskForecast().subscribe({
      next: data => {
        this.delayRisk = data;
        setTimeout(() => this.drawRiskTrendChart(), 0);
      },
      error: err => this.error = err?.message || 'Failed to load delay risk'
    });

    this.analyticsService.getDelayRegression().subscribe({
      next: data => {
        this.delayRegression = data;
        setTimeout(() => this.drawRegressionChart(), 0);
      },
      error: err => this.error = err?.message || 'Failed to load regression'
    });
  }

  private drawTokenCostChart(): void {
    if (!this.llmSpendData || !this.tokenCostChart?.nativeElement) return;

    const labels = (this.llmSpendData.groupByDay || []).map((item: any) => new Date(item.date).toLocaleDateString());
    const tokenData = (this.llmSpendData.groupByDay || []).map((item: any) => item.tokenUsage);
    const costData = (this.llmSpendData.groupByDay || []).map((item: any) => item.costUsd);

    if (this.tokenCostChartInstance) this.tokenCostChartInstance.destroy();

    this.tokenCostChartInstance = new Chart(this.tokenCostChart.nativeElement.getContext('2d')!, {
      type: 'line',
      data: {
        labels,
        datasets: [
          { label: 'Tokens', data: tokenData, borderColor: '#2a9d8f', yAxisID: 'y1', fill: false },
          { label: 'Cost (USD)', data: costData, borderColor: '#e76f51', yAxisID: 'y2', fill: false }
        ]
      },
      options: {
        responsive: true,
        scales: {
          y1: { type: 'linear', position: 'left', title: { display: true, text: 'Tokens' } },
          y2: { type: 'linear', position: 'right', grid: { drawOnChartArea: false }, title: { display: true, text: 'Cost (USD)' } }
        },
        plugins: { title: { display: true, text: 'LLM Token/Cost Evolution' }}
      }
    });
  }

  private drawRiskTrendChart(): void {
    if (!this.delayRisk || !this.riskTrendChart?.nativeElement) return;

    if (this.riskTrendChartInstance) this.riskTrendChartInstance.destroy();

    const days = this.delayRisk.highRisk?.map((x: any) => x.date) ?? [];

    const highData = this.delayRisk.highRisk?.map((x: any) => x.riskScore) ?? [];
    const medData = this.delayRisk.mediumRisk?.map((x: any) => x.riskScore) ?? [];
    const lowData = this.delayRisk.lowRisk?.map((x: any) => x.riskScore) ?? [];

    this.riskTrendChartInstance = new Chart(this.riskTrendChart.nativeElement.getContext('2d')!, {
      type: 'line',
      data: {
        labels: days,
        datasets: [
          { label: 'High Risk', data: highData, borderColor: '#e76f51', fill: false },
          { label: 'Medium Risk', data: medData, borderColor: '#f4a261', fill: false },
          { label: 'Low Risk', data: lowData, borderColor: '#2a9d8f', fill: false }
        ]
      },
      options: {
        responsive: true,
        plugins: { title: { display: true, text: 'Risk Level Trend' } }
      }
    });
  }

  private drawRegressionChart(): void {
    if (!this.delayRegression || !this.regressionChart?.nativeElement) return;

    if (this.regressionChartInstance) this.regressionChartInstance.destroy();

    const points = (this.delayRegression.samples || []).map((sample: any) => ({ x: sample.hours, y: sample.delay }));
    const lineX = points.map((p: any) => p.x);
    const minX = Math.min(...lineX); const maxX = Math.max(...lineX);
    const slope = this.delayRegression.slope;
    const intercept = this.delayRegression.intercept;
    const line = [
      { x: minX, y: slope * minX + intercept },
      { x: maxX, y: slope * maxX + intercept }
    ];

    this.regressionChartInstance = new Chart(this.regressionChart.nativeElement.getContext('2d')!, {
      type: 'scatter',
      data: {
        datasets: [
          { label: 'Observed delay', data: points, backgroundColor: '#264653' },
          { label: 'Regression line', data: line, type: 'line', borderColor: '#e9c46a', fill: false, pointRadius: 0 }
        ]
      },
      options: {
        responsive: true,
        scales: {
          x: { title: { display: true, text: 'Duration Hours' } },
          y: { title: { display: true, text: 'Delay Minutes' } }
        },
        plugins: { title: { display: true, text: 'Delay vs Duration Regression' } }
      }
    });
  }
}
