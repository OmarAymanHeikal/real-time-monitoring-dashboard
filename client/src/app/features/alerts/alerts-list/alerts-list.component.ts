import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { SignalRService, AlertUpdate } from '../../../core/services/signalr.service';

interface Alert {
  alertId: number;
  serverId: number;
  serverName?: string;
  metricType: string;
  threshold: number;
  currentValue: number;
  status: string;
  message: string;
  triggeredAt: Date;
}

@Component({
  selector: 'app-alerts-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatCardModule, MatChipsModule, MatIconModule, MatButtonModule],
  templateUrl: './alerts-list.component.html',
  styleUrls: ['./alerts-list.component.scss']
})
export class AlertsListComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private signalR = inject(SignalRService);
  private unsubscribe?: () => void;

  cols = ['severity', 'serverName', 'metricType', 'value', 'triggeredAt', 'status'];
  dataSource = new MatTableDataSource<Alert>([]);

  get activeAlertsCount(): number {
    return this.dataSource.data.filter(a => a.status === 'Active').length;
  }

  ngOnInit(): void {
    this.loadAlerts();

    // Subscribe to real-time alert updates
    this.unsubscribe = this.signalR.onAlertUpdate((alert) => {
      this.handleAlertUpdate(alert);
    });
  }

  ngOnDestroy(): void {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
  }

  loadAlerts(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: Alert[] }>(`${apiUrl}/api/v1/alerts`).subscribe({
      next: (res) => {
        this.dataSource.data = res.items ?? [];
      },
      error: (err) => console.error('Failed to load alerts:', err)
    });
  }

  private handleAlertUpdate(alert: AlertUpdate): void {
    const newAlert: Alert = {
      alertId: alert.alertId,
      serverId: alert.serverId,
      serverName: alert.serverName,
      metricType: alert.metricType,
      threshold: alert.threshold,
      currentValue: alert.currentValue,
      status: alert.status,
      message: `${alert.metricType} exceeded threshold`,
      triggeredAt: alert.triggeredAt
    };

    // Add to top of list
    this.dataSource.data = [newAlert, ...this.dataSource.data];
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'warn';
      case 'acknowledged':
        return 'accent';
      case 'resolved':
        return 'primary';
      default:
        return '';
    }
  }

  getSeverityIcon(metricType: string, currentValue: number, threshold: number): string {
    const ratio = currentValue / threshold;
    if (ratio >= 1.2) return 'error';
    if (ratio >= 1.0) return 'warning';
    return 'info';
  }

  getSeverityColor(metricType: string, currentValue: number, threshold: number): string {
    const ratio = currentValue / threshold;
    if (ratio >= 1.2) return '#f44336';
    if (ratio >= 1.0) return '#ff9800';
    return '#2196f3';
  }
}
