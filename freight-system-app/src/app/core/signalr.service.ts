import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection?: signalR.HubConnection;
  public shipmentUpdated$ = new Subject<any>();

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5001/hubs/tracking')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.error('SignalR Connection Error:', err));

    this.hubConnection.on('ShipmentUpdated', data => {
      this.shipmentUpdated$.next(data);
      this.notifyUser(data, 'Shipment update');
    });

    this.hubConnection.on('ShipmentCreated', data => {
      this.shipmentUpdated$.next(data);
      this.notifyUser(data, 'New shipment');
    });
  }

  private async notifyUser(data: any, title: string): Promise<void> {
    if (!('Notification' in window)) return;
    if (Notification.permission !== 'granted') return;

    const registration = await navigator.serviceWorker.ready;
    registration.showNotification(`${title}: ${data.trackingNumber || data.shipmentId || 'Unknown'}`, {
      body: `Status: ${data.status || 'N/A'}`,
      icon: '/assets/icon-72x72.png',
      data,
      tag: 'freight-alert'
    });
  }
}

