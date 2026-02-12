import { TestBed } from '@angular/core/testing';
import { SignalRService, MetricUpdate, AlertUpdate } from './signalr.service';
import * as signalR from '@microsoft/signalr';

describe('SignalRService', () => {
  let service: SignalRService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [SignalRService]
    });
    service = TestBed.inject(SignalRService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with disconnected status', () => {
    expect(service.connectionStatus()).toBe('disconnected');
  });

  it('should initialize online users count to 0', () => {
    expect(service.onlineUsersCount()).toBe(0);
  });

  it('should initialize latest metric to null', () => {
    expect(service.latestMetric()).toBeNull();
  });

  it('should initialize latest alert to null', () => {
    expect(service.latestAlert()).toBeNull();
  });

  it('should register metric callback', () => {
    const callback = jasmine.createSpy('metricCallback');
    const unsubscribe = service.onMetricUpdate(callback);
    
    expect(unsubscribe).toEqual(jasmine.any(Function));
  });

  it('should register alert callback', () => {
    const callback = jasmine.createSpy('alertCallback');
    const unsubscribe = service.onAlertUpdate(callback);
    
    expect(unsubscribe).toEqual(jasmine.any(Function));
  });

  it('should set status to connecting when starting connection', () => {
    service.startConnection('test-token');
    expect(service.connectionStatus()).toBe('connecting');
  });

  it('should handle metric updates', () => {
    const mockMetric: MetricUpdate = {
      serverId: 1,
      serverName: 'Test Server',
      cpuUsage: 75,
      memoryUsage: 60,
      diskUsage: 50,
      responseTime: 100,
      status: 'Up',
      timestamp: new Date()
    };

    const callback = jasmine.createSpy('metricCallback');
    service.onMetricUpdate(callback);

    // Simulate metric update
    service['latestMetric'].set(mockMetric);
    
    expect(service.latestMetric()).toEqual(mockMetric);
  });
});
