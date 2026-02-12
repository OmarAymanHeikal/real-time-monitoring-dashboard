import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { SignalRService } from './signalr.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let signalRServiceSpy: jasmine.SpyObj<SignalRService>;

  beforeEach(() => {
    const signalRSpy = jasmine.createSpyObj('SignalRService', ['startConnection', 'stopConnection']);
    signalRSpy.startConnection.and.returnValue(Promise.resolve());
    signalRSpy.stopConnection.and.returnValue(Promise.resolve());

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        { provide: SignalRService, useValue: signalRSpy }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    signalRServiceSpy = TestBed.inject(SignalRService) as jasmine.SpyObj<SignalRService>;
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login successfully and store token', (done) => {
    const mockResponse = {
      accessToken: 'test-token',
      refreshToken: 'refresh-token',
      role: 'Admin'
    };

    service.login('admin', 'password').subscribe({
      next: (response) => {
        expect(response).toEqual(mockResponse);
        expect(localStorage.getItem('token')).toBe('test-token');
        expect(localStorage.getItem('role')).toBe('Admin');
        expect(signalRServiceSpy.startConnection).toHaveBeenCalledWith('test-token');
        done();
      }
    });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ userName: 'admin', password: 'password' });
    req.flush(mockResponse);
  });

  it('should logout and clear stored data', (done) => {
    // Setup authenticated state
    localStorage.setItem('token', 'test-token');
    localStorage.setItem('refresh_token', 'refresh-token');
    localStorage.setItem('role', 'Admin');

    service.logout();

    setTimeout(() => {
      expect(localStorage.getItem('token')).toBeNull();
      expect(signalRServiceSpy.stopConnection).toHaveBeenCalled();
      done();
    }, 100);
  });

  it('should return false for isAuthenticated when no token', () => {
    localStorage.clear();
    const newService = TestBed.inject(AuthService);
    expect(newService.currentUser()).toBeNull();
  });

  it('should check if user is admin', (done) => {
    // Simulate login response that sets role
    const mockResponse = { accessToken: 'token123', refreshToken: 'refresh123', role: 'Admin' };
    
    service.login('admin', 'password').subscribe(() => {
      service.isAdmin$().subscribe(isAdmin => {
        expect(isAdmin).toBe(true);
        done();
      });
    });

    const req = httpMock.expectOne(request => request.url.includes('/api/v1/auth/login'));
    req.flush(mockResponse);
  });
});
