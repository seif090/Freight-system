import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection?: signalR.HubConnection;
  public shipmentUpdated$ = new Subject<any>();
  public missedEtaDelayHistoryPopulated$ = new Subject<any>();
  public overdueAlert$ = new Subject<any>();

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5001/hubs/tracking')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().then(() => {
      this.hubConnection?.invoke('SubscribeToDispatchers').catch(err => console.error('SignalR subscribe dispatcher failed:', err));
    }).catch(err => console.error('SignalR Connection Error:', err));

    this.hubConnection.on('ShipmentUpdated', data => {
      this.shipmentUpdated$.next(data);
      this.notifyUser(data, 'Shipment update');
    });

    this.hubConnection.on('ShipmentCreated', data => {
      this.shipmentUpdated$.next(data);
      this.notifyUser(data, 'New shipment');
    });

    this.hubConnection.on('MissedEtaDelayHistoryPopulated', data => {
      this.missedEtaDelayHistoryPopulated$.next(data);
      this.notifyUser(data, 'Missed ETA Delay history update');
    });

    this.hubConnection.on('OverdueAlert', data => {
      this.overdueAlert$.next(data);
      this.notifyUser(data, 'Overdue shipments alert');
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

