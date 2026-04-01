import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdvancedAnalyticsService } from './advanced-analytics.service';

@Component({
  selector: 'app-maintenance-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './maintenance-notifications.component.html'
})
export class MaintenanceNotificationsComponent {
  tenantId = 'default';
  notifications: string[] = [];
  error = '';

  constructor(private analyticsService: AdvancedAnalyticsService){ }

  generate(): void {
    this.analyticsService.getMaintenanceRisk(this.tenantId).subscribe({
      next: vehicles => {
        this.notifications = (vehicles as any[]).map(v => {
          const d = new Date(v.nextInspectionDue);
          return `Vehicle ${v.registrationNumber} due in ${Math.max(0, Math.round((d.getTime() - Date.now())/(1000*60*60*24)))} days`; 
        });
        if(!this.notifications.length){
          this.notifications.push('No immediate maintenance alerts.');
        }
        this.error = '';
      },
      error: err => this.error = err?.message || 'Cannot fetch alerts'
    });
  }

  simulateReroute(): void {
    this.notifications.unshift('Reroute recommendation: avoid congestion on highway M3, use local alternative via Route 12');
  }
}
