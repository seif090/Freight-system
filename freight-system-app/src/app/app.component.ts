import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SignalrService } from './core/signalr.service';
import { AuthService } from './modules/auth/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'freight-system-app';
  userRoles: string[] = [];

  constructor(private signalr: SignalrService, private authService: AuthService) {}

  ngOnInit(): void {
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker.register('/sw.js').catch(err => console.warn('SW register failed', err));
    }

    if ('Notification' in window && Notification.permission !== 'granted') {
      Notification.requestPermission();
    }

    this.signalr.startConnection();
    this.signalr.shipmentUpdated$.subscribe(update => {
      console.log('Live tracking event:', update);
    });

    this.userRoles = this.authService.getUserRoles();
  }

  isAdmin() {
    return this.userRoles.includes('Admin');
  }

  isSales() {
    return this.userRoles.includes('Sales');
  }

  isOperation() {
    return this.userRoles.includes('Operation');
  }

  logout() {
    this.authService.logout();
    window.location.href = '/login';
  }
}
