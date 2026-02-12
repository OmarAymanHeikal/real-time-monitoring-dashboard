import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { NgChartsModule, BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartData, registerables } from 'chart.js';
import { SignalRService, MetricUpdate } from '../../core/services/signalr.service';

Chart.register(...registerables);

interface DashboardStats {
  totalServers: number;
  activeServers: number;
  activeAlerts: number;
  avgCpuUsage: number;
  avgMemoryUsage: number;
}

interface Server {
  serverId: number;
  name: string;
  status: string;
  ipAddress: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatGridListModule, RouterLink, NgChartsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private signalR = inject(SignalRService);
  private cdr = inject(ChangeDetectorRef);
  private unsubscribe?: () => void;

  stats: DashboardStats = {
    totalServers: 0,
    activeServers: 0,
    activeAlerts: 0,
    avgCpuUsage: 0,
    avgMemoryUsage: 0
  };

  servers: Server[] = [];
  recentMetrics: MetricUpdate[] = [];

  // Chart configurations
  cpuChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [{
      label: 'CPU Usage (%)',
      data: [],
      borderColor: '#2196F3',
      backgroundColor: 'rgba(33, 150, 243, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  memoryChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [{
      label: 'Memory Usage (%)',
      data: [],
      borderColor: '#4CAF50',
      backgroundColor: 'rgba(76, 175, 80, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  serverStatusChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: ['Up', 'Warning', 'Down'],
    datasets: [{
      data: [0, 0, 0],
      backgroundColor: ['#4CAF50', '#FF9800', '#F44336']
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
        max: 100
      }
    }
  };

  doughnutChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'right'
      }
    }
  };

  ngOnInit(): void {
    this.loadDashboardData();
    
    // Subscribe to real-time metric updates
    this.unsubscribe = this.signalR.onMetricUpdate((metric) => {
      this.handleMetricUpdate(metric);
    });
  }

  ngOnDestroy(): void {
    if (this.unsubscribe) {
      this.unsubscribe();
    }
  }

  private loadDashboardData(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    
    console.log('🔄 Loading dashboard data from:', apiUrl);
    
    // Load servers
    this.http.get<{ items: Server[] }>(`${apiUrl}/api/v1/servers`).subscribe({
      next: (res) => {
        console.log('✅ Servers loaded:', res);
        this.servers = res.items || [];
        console.log('📊 Servers array:', this.servers);
        this.updateStats();
        this.updateServerStatusChart();
        
        // Load recent metrics for each server
        this.servers.slice(0, 3).forEach(server => {
          this.loadServerMetrics(server.serverId);
        });
      },
      error: (err) => {
        console.error('❌ Failed to load servers:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
      }
    });

    // Load alerts count
    this.http.get<{ items: unknown[] }>(`${apiUrl}/api/v1/alerts`).subscribe({
      next: (res) => {
        console.log('✅ Alerts loaded:', res);
        this.stats.activeAlerts = (res.items || []).length;
      },
      error: (err) => {
        console.error('❌ Failed to load alerts:', err);
      }
    });
  }

  private loadServerMetrics(serverId: number): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    
    console.log(`🔄 Loading metrics for server ${serverId}`);
    this.http.get<{ items: any[] }>(`${apiUrl}/api/v1/metrics/server/${serverId}?pageSize=10`).subscribe({
      next: (res) => {
        console.log(`✅ Metrics loaded for server ${serverId}:`, res);
        const metrics = res.items || [];
        metrics.forEach((m: any) => {
          this.recentMetrics.push({
            serverId: m.serverId,
            serverName: this.servers.find(s => s.serverId === m.serverId)?.name || '',
            cpuUsage: m.cpuUsage,
            memoryUsage: m.memoryUsage,
            diskUsage: m.diskUsage,
            responseTime: m.responseTime,
            status: m.status,
            timestamp: new Date(m.timestamp)
          });
        });
        
        console.log('📈 Recent metrics array:', this.recentMetrics);
        
        // Update charts with latest data
        this.updateCharts();
      },
      error: (err) => {
        console.error(`❌ Failed to load metrics for server ${serverId}:`, err);
      }
    });
  }

  private handleMetricUpdate(metric: MetricUpdate): void {
    // Add to recent metrics
    this.recentMetrics.unshift(metric);
    
    // Keep only last 20 metrics
    if (this.recentMetrics.length > 20) {
      this.recentMetrics.pop();
    }
    
    // Update charts
    this.updateCharts();
    
    // Update stats
    this.calculateAverages();
  }

  private updateStats(): void {
    this.stats.totalServers = this.servers.length;
    this.stats.activeServers = this.servers.filter(s => s.status === 'Up').length;
    this.calculateAverages();
  }

  private calculateAverages(): void {
    if (this.recentMetrics.length > 0) {
      const recent = this.recentMetrics.slice(0, 10);
      this.stats.avgCpuUsage = recent.reduce((sum, m) => sum + m.cpuUsage, 0) / recent.length;
      this.stats.avgMemoryUsage = recent.reduce((sum, m) => sum + m.memoryUsage, 0) / recent.length;
    }
  }

  private updateCharts(): void {
    const recent = this.recentMetrics.slice(0, 15).reverse();
    
    console.log('📊 Updating charts with', recent.length, 'data points');
    
    // Create new data objects to trigger change detection
    this.cpuChartData = {
      labels: recent.map(m => new Date(m.timestamp).toLocaleTimeString()),
      datasets: [{
        label: 'CPU Usage (%)',
        data: recent.map(m => m.cpuUsage),
        borderColor: '#2196F3',
        backgroundColor: 'rgba(33, 150, 243, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
    
    this.memoryChartData = {
      labels: recent.map(m => new Date(m.timestamp).toLocaleTimeString()),
      datasets: [{
        label: 'Memory Usage (%)',
        data: recent.map(m => m.memoryUsage),
        borderColor: '#4CAF50',
        backgroundColor: 'rgba(76, 175, 80, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
    
    // Trigger change detection
    this.cdr.detectChanges();
  }

  private updateServerStatusChart(): void {
    const up = this.servers.filter(s => s.status === 'Up').length;
    const warning = this.servers.filter(s => s.status === 'Warning').length;
    const down = this.servers.filter(s => s.status === 'Down').length;
    
    console.log('📊 Server status:', { up, warning, down });
    
    // Create new data object to trigger change detection
    this.serverStatusChartData = {
      labels: ['Up', 'Warning', 'Down'],
      datasets: [{
        data: [up, warning, down],
        backgroundColor: ['#4CAF50', '#FF9800', '#F44336']
      }]
    };
    
    this.cdr.detectChanges();
  }
}
