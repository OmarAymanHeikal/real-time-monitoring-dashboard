import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-jobs-page',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatListModule],
  templateUrl: './jobs-page.component.html',
  styleUrls: ['./jobs-page.component.scss']
})
export class JobsPageComponent {
  get hangfireUrl(): string {
    const apiUrl = (window as any).env?.apiUrl ?? 'http://localhost:5000';
    return `${apiUrl}/hangfire`;
  }

  jobs = [
    {
      name: 'Metrics Collection Job',
      type: 'Recurring',
      schedule: 'Every 2 minutes',
      description: 'Collects CPU, Memory, Disk metrics from all registered servers',
      icon: 'schedule',
      color: 'primary'
    },
    {
      name: 'Alert Threshold Job',
      type: 'Recurring',
      schedule: 'Every 1 minute',
      description: 'Checks metrics against thresholds and triggers alerts',
      icon: 'notifications_active',
      color: 'warn'
    },
    {
      name: 'Report Generation Job',
      type: 'Fire-and-Forget',
      schedule: 'On-demand',
      description: 'Generates PDF/Excel reports for specified time periods',
      icon: 'description',
      color: 'accent'
    },
    {
      name: 'Report Cleanup Job',
      type: 'Continuation',
      schedule: 'After report generation',
      description: 'Archives old reports and cleans temporary files',
      icon: 'cleaning_services',
      color: 'primary'
    },
    {
      name: 'Maintenance Notification Job',
      type: 'Delayed',
      schedule: 'Scheduled delays',
      description: 'Sends maintenance notifications at scheduled times',
      icon: 'build',
      color: 'accent'
    }
  ];

  openHangfire(): void {
    const token = localStorage.getItem('token');
    const apiUrl = (window as any).env?.apiUrl ?? 'http://localhost:5000';
    const url = token 
      ? `${apiUrl}/hangfire?access_token=${encodeURIComponent(token)}`
      : `${apiUrl}/hangfire`;
    window.open(url, '_blank');
  }
}
