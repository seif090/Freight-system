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
    });

    this.hubConnection.on('ShipmentCreated', data => {
      this.shipmentUpdated$.next(data);
    });
  }
}
