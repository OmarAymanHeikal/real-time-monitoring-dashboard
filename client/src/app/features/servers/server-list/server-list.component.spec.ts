import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { provideAnimations } from '@angular/platform-browser/animations';
import { ServerListComponent } from './server-list.component';

describe('ServerListComponent', () => {
  let component: ServerListComponent;
  let fixture: ComponentFixture<ServerListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ServerListComponent,
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        provideAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty data source', () => {
    expect(component.dataSource.data).toEqual([]);
  });

  it('should have correct table columns', () => {
    expect(component.cols).toEqual(['name', 'ipAddress', 'status', 'action']);
  });

  it('should have loadServers method', () => {
    expect(component.loadServers).toBeDefined();
  });

  it('should have getStatusColor method', () => {
    expect(component.getStatusColor('Up')).toBe('primary');
    expect(component.getStatusColor('Warning')).toBe('accent');
    expect(component.getStatusColor('Down')).toBe('warn');
  });
});
