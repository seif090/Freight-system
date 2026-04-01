import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

interface TelemetryPoint {
  id: number;
  shipmentId: number;
  timestamp: string;
  latitude: number;
  longitude: number;
  speedKmh: number;
  headingDegrees: number;
  fuelLevel: number;
}

@Component({
  selector: 'app-digital-twin',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './digital-twin.component.html'
})
export class DigitalTwinComponent implements OnInit {
  livePoints: TelemetryPoint[] = [];
  shipmentId = 1;
  sse?: EventSource;
  routeSuggestion: any = null;
  voiceText = '';
  voiceStatus = 'idle';

  constructor(private analyticsService: AdvancedAnalyticsService) {}

  ngOnInit(): void {
    this.connectSse();
  }

  connectSse(): void {
    this.disconnectSse();
    const url = `https://localhost:5001/api/v1.0/telemetry/live-stream?shipmentId=${this.shipmentId}`;
    this.sse = new EventSource(url);

    this.sse.onmessage = (ev) => {
      const data = JSON.parse(ev.data) as TelemetryPoint;
      this.livePoints.unshift(data);
      if (this.livePoints.length > 50) this.livePoints.pop();
    };

    this.sse.onerror = () => {
      this.sse?.close();
      setTimeout(() => this.connectSse(), 3000);
    };
  }

  disconnectSse(): void {
    if (this.sse) {
      this.sse.close();
      this.sse = undefined;
    }
  }

  requestRouteOptimization() {
    const neutralSegments = [
      { segmentOrder: 1, startLatitude: 30.0, startLongitude: 31.0, endLatitude: 31.2, endLongitude: 32.1, distanceKm: 120 },
      { segmentOrder: 2, startLatitude: 31.2, startLongitude: 32.1, endLatitude: 32.0, endLongitude: 33.0, distanceKm: 90 }
    ];

    this.analyticsService.optimizeRoute(this.shipmentId, neutralSegments).subscribe(result => {
      this.routeSuggestion = result;
    });
  }

  startVoiceAssistant(): void {
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
    if (!SpeechRecognition) {
      this.voiceStatus = 'unsupported';
      return;
    }

    const recognition = new SpeechRecognition();
    recognition.lang = 'en-US';
    recognition.interimResults = false;
    recognition.maxAlternatives = 1;

    recognition.onstart = () => this.voiceStatus = 'listening';
    recognition.onresult = (event: any) => {
      this.voiceText = event.results[0][0].transcript;
      this.voiceStatus = 'captured';
      // interpret command
      if (this.voiceText.toLowerCase().includes('schedule maintenance')) {
        this.scheduleMaintenanceViaVoice();
      }
    };

    recognition.onerror = () => this.voiceStatus = 'error';
    recognition.onend = () => { if (this.voiceStatus === 'listening') this.voiceStatus = 'idle'; };

    recognition.start();
  }

  scheduleMaintenanceViaVoice(): void {
    this.analyticsService.scheduleMaintenance({
      shipmentId: this.shipmentId,
      vehicleId: 1,
      maintenanceDate: new Date(new Date().getTime() + 24*60*60*1000).toISOString(),
      description: 'Voice assistant scheduled maintenance'
    }).subscribe();
  }
}
