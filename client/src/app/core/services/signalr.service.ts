import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';

export interface MetricUpdate {
  serverId: number;
  serverName: string;
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
  responseTime: number;
  status: string;
  timestamp: Date;
}

export interface AlertUpdate {
  alertId: number;
  serverId: number;
  serverName: string;
  metricType: string;
  threshold: number;
  currentValue: number;
  status: string;
  triggeredAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  
  // Signals for reactive updates
  onlineUsersCount = signal<number>(0);
  latestMetric = signal<MetricUpdate | null>(null);
  latestAlert = signal<AlertUpdate | null>(null);
  connectionStatus = signal<'connected' | 'disconnected' | 'connecting'>('disconnected');

  // Callbacks for components that need custom handling
  private metricCallbacks: ((metric: MetricUpdate) => void)[] = [];
  private alertCallbacks: ((alert: AlertUpdate) => void)[] = [];

  constructor() {}

  startConnection(accessToken: string): Promise<void> {
    if (this.hubConnection) {
      return Promise.resolve();
    }

    this.connectionStatus.set('connecting');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/monitoring`, {
        accessTokenFactory: () => accessToken,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupEventHandlers();

    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR Connected');
        this.connectionStatus.set('connected');
      })
      .catch(err => {
        console.error('SignalR Connection Error:', err);
        this.connectionStatus.set('disconnected');
        throw err;
      });
  }

  stopConnection(): Promise<void> {
    if (!this.hubConnection) {
      return Promise.resolve();
    }

    return this.hubConnection.stop()
      .then(() => {
        console.log('SignalR Disconnected');
        this.hubConnection = null;
        this.connectionStatus.set('disconnected');
      });
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle metric updates
    this.hubConnection.on('ReceiveMetric', (metric: MetricUpdate) => {
      console.log('Received Metric Update:', metric);
      this.latestMetric.set(metric);
      this.metricCallbacks.forEach(callback => callback(metric));
    });

    // Handle alert notifications
    this.hubConnection.on('ReceiveAlert', (alert: AlertUpdate) => {
      console.log('Received Alert:', alert);
      this.latestAlert.set(alert);
      this.alertCallbacks.forEach(callback => callback(alert));
    });

    // Handle user presence updates
    this.hubConnection.on('UserPresence', (data: { userId: string; isOnline: boolean; totalOnline: number }) => {
      console.log('Online Users:', data.totalOnline);
      this.onlineUsersCount.set(data.totalOnline);
    });

    // Handle reconnection
    this.hubConnection.onreconnecting(() => {
      this.connectionStatus.set('connecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStatus.set('connected');
    });

    this.hubConnection.onclose(() => {
      this.connectionStatus.set('disconnected');
    });
  }

  // Subscribe to metric updates
  onMetricUpdate(callback: (metric: MetricUpdate) => void): () => void {
    this.metricCallbacks.push(callback);
    return () => {
      const index = this.metricCallbacks.indexOf(callback);
      if (index > -1) {
        this.metricCallbacks.splice(index, 1);
      }
    };
  }

  // Subscribe to alert updates
  onAlertUpdate(callback: (alert: AlertUpdate) => void): () => void {
    this.alertCallbacks.push(callback);
    return () => {
      const index = this.alertCallbacks.indexOf(callback);
      if (index > -1) {
        this.alertCallbacks.splice(index, 1);
      }
    };
  }

  // Send heartbeat (if needed for keepalive)
  sendHeartbeat(): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      return Promise.reject('Not connected');
    }
    return this.hubConnection.invoke('Heartbeat');
  }
}
