import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SignalrService } from './core/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'freight-system-app';

  constructor(private signalr: SignalrService) {}

  ngOnInit(): void {
    this.signalr.startConnection();
    this.signalr.shipmentUpdated$.subscribe(update => {
      console.log('Live tracking event:', update);
    });
  }
}
