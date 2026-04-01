import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl = 'https://localhost:5001/api/v1.0/analytics';

  constructor(private http: HttpClient) {}

  getSummary(): Observable<any> {
    return this.http.get(`${this.apiUrl}/summary`);
  }

  getLlmSpendTrend(from?: string, to?: string): Observable<any> {
    let params = '';
    if (from) params += `from=${encodeURIComponent(from)}&`;
    if (to) params += `to=${encodeURIComponent(to)}&`;
    return this.http.get(`${this.apiUrl}/llm-spend-trend?${params}`);
  }

  getDelayRiskForecast(): Observable<any> {
    return this.http.get(`${this.apiUrl}/delay-risk-forecast`);
  }

  getDelayRegression(sampleSize = 100): Observable<any> {
    return this.http.get(`${this.apiUrl}/delay-regression?sampleSize=${sampleSize}`);
  }

  getDelayAnomalyClusters(thresholdMinutes = 30): Observable<any> {
    return this.http.get(`${this.apiUrl}/delay-history/anomalies?thresholdMinutes=${thresholdMinutes}`);
  }

  getDelayAnomalyClusterHistory(from?: string, to?: string): Observable<any> {
    let params = '';
    if (from) params += `from=${encodeURIComponent(from)}&`;
    if (to) params += `to=${encodeURIComponent(to)}&`;
    return this.http.get(`${this.apiUrl}/delay-history/cluster-history?${params}`);
  }

  getFinancialSummary(): Observable<any> {
    return this.http.get('https://localhost:5001/api/v1.0/reports/financial/summary');
  }

  getInvoiceAging(): Observable<any> {
    return this.http.get('https://localhost:5001/api/v1.0/reports/financial/aging');
  }

  markInvoicePaid(invoiceId: number): Observable<any> {
    return this.http.post(`https://localhost:5001/api/v1.0/reports/invoices/${invoiceId}/mark-paid`, {});
  }

  manualPopulateDelayHistory(): Observable<any> {
    return this.http.post(`${this.apiUrl}/shipments/missed-eta-populate`, {});
  }
}
