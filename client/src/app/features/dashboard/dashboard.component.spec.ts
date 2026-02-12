import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { provideAnimations } from '@angular/platform-browser/animations';
import { DashboardComponent } from './dashboard.component';
import { SignalRService } from '../../core/services/signalr.service';
import { of } from 'rxjs';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let signalRServiceSpy: jasmine.SpyObj<SignalRService>;

  beforeEach(async () =>{
    const signalRSpy = jasmine.createSpyObj('SignalRService', ['onMetricUpdate', 'onAlertUpdate', 'onUserPresence']);
    signalRSpy.onMetricUpdate.and.returnValue(() => {});
    signalRSpy.onAlertUpdate.and.returnValue(() => {});
    signalRSpy.onUserPresence.and.returnValue(() => {});

    await TestBed.configureTestingModule({
      imports: [
        DashboardComponent,
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        provideAnimations(),
        { provide: SignalRService, useValue: signalRSpy }
      ]
    }).compileComponents();

    signalRServiceSpy = TestBed.inject(SignalRService) as jasmine.SpyObj<SignalRService>;
    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize empty servers array', () => {
    expect(component.servers).toEqual([]);
  });

  it('should initialize empty recent metrics array', () => {
    expect(component.recentMetrics).toEqual([]);
  });

  it('should subscribe to SignalR metric updates on init', () => {
    fixture.detectChanges();
    expect(signalRServiceSpy.onMetricUpdate).toHaveBeenCalled();
  });

  it('should have initial stats', () => {
    expect(component.stats).toBeDefined();
    expect(component.stats.totalServers).toBe(0);
    expect(component.stats.activeServers).toBe(0);
    expect(component.stats.activeAlerts).toBe(0);
  });
});
