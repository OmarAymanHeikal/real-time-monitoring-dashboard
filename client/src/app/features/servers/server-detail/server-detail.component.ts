import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { SignalRService, MetricUpdate } from '../../../core/services/signalr.service';

interface Server {
  serverId: number;
  name: string;
  ipAddress: string;
  status: string;
  description?: string;
}

@Component({
  selector: 'app-server-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatChipsModule, MatIconModule, NgChartsModule],
  templateUrl: './server-detail.component.html',
  styleUrls: ['./server-detail.component.scss']
})
export class ServerDetailComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private signalR = inject(SignalRService);
  private unsubscribe?: () => void;

  server: Server | null = null;
  metrics: MetricUpdate[] = [];
  latestMetric: MetricUpdate | null = null;

  // Chart data
  metricsChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'CPU Usage (%)',
        data: [],
        borderColor: '#2196F3',
        backgroundColor: 'rgba(33, 150, 243, 0.1)',
        tension: 0.4,
        yAxisID: 'y'
      },
      {
        label: 'Memory Usage (%)',
        data: [],
        borderColor: '#4CAF50',
        backgroundColor: 'rgba(76, 175, 80, 0.1)',
        tension: 0.4,
        yAxisID: 'y'
      },
      {
        label: 'Disk Usage (%)',
        data: [],
        borderColor: '#FF9800',
        backgroundColor: 'rgba(255, 152, 0, 0.1)',
        tension: 0.4,
        yAxisID: 'y'
      }
    ]
  };

  responseTimeChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{
      label: 'Response Time (ms)',
      data: [],
      backgroundColor: '#9C27B0'
    }]
  };

  chartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top'
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        max: 100,
        position: 'left'
      }
    }
  };

  responseTimeChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.loadServerDetails(+id);
    this.loadMetrics(+id);

    // Subscribe to real-time updates
    this.unsubscribe = this.signalR.onMetricUpdate((metric) => {
      if (metric.serverId === +id!) {
        this.handleMetricUpdate(metric);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
  }

  private loadServerDetails(id: number): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<Server>(`${apiUrl}/api/v1/servers/${id}`).subscribe({
      next: (server) => {
        this.server = server;
      },
      error: (err) => console.error('Failed to load server:', err)
    });
  }

  private loadMetrics(serverId: number): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: any[] }>(`${apiUrl}/api/v1/metrics/server/${serverId}?pageSize=20`).subscribe({
      next: (res) => {
        this.metrics = (res.items || []).map((m: any) => ({
          serverId: m.serverId,
          serverName: this.server?.name || '',
          cpuUsage: m.cpuUsage,
          memoryUsage: m.memoryUsage,
          diskUsage: m.diskUsage,
          responseTime: m.responseTime,
          status: m.status,
          timestamp: new Date(m.timestamp)
        })).reverse();

        if (this.metrics.length > 0) {
          this.latestMetric = this.metrics[this.metrics.length - 1];
        }

        this.updateCharts();
      },
      error: (err) => console.error('Failed to load metrics:', err)
    });
  }

  private handleMetricUpdate(metric: MetricUpdate): void {
    this.metrics.push(metric);
    this.latestMetric = metric;

    // Keep only last 20 metrics
    if (this.metrics.length > 20) {
      this.metrics.shift();
    }

    this.updateCharts();
  }

  private updateCharts(): void {
    const recent = this.metrics.slice(-15);

    this.metricsChartData.labels = recent.map(m =>
      new Date(m.timestamp).toLocaleTimeString()
    );
    this.metricsChartData.datasets[0].data = recent.map(m => m.cpuUsage);
    this.metricsChartData.datasets[1].data = recent.map(m => m.memoryUsage);
    this.metricsChartData.datasets[2].data = recent.map(m => m.diskUsage);

    this.responseTimeChartData.labels = recent.map(m =>
      new Date(m.timestamp).toLocaleTimeString()
    );
    this.responseTimeChartData.datasets[0].data = recent.map(m => m.responseTime);
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'up':
        return 'primary';
      case 'warning':
        return 'accent';
      case 'down':
        return 'warn';
      default:
        return '';
    }
  }
}
