import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { of, throwError } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login']);
    const router = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        FormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule
      ],
      providers: [
        provideAnimations(),
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: router }
      ]
    }).compileComponents();

    authServiceSpy = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty username and password', () => {
    expect(component.userName).toBe('');
    expect(component.password).toBe('');
  });

  it('should call auth service on login', () => {
    const mockResponse = { accessToken: 'token', refreshToken: 'refresh', role: 'Admin' };
    authServiceSpy.login.and.returnValue(of(mockResponse));

    component.userName = 'admin';
    component.password = 'password123';

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith('admin', 'password123');
  });

  it('should navigate to dashboard on successful login', (done) => {
    const mockResponse = { accessToken: 'token', refreshToken: 'refresh', role: 'Admin' };
    authServiceSpy.login.and.returnValue(of(mockResponse));

    component.userName = 'admin';
    component.password = 'password123';

    component.onSubmit();

    setTimeout(() => {
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/dashboard']);
      done();
    }, 100);
  });

  it('should display error message on login failure', (done) => {
    authServiceSpy.login.and.returnValue(
      throwError(() => ({ error: { error: 'Invalid credentials' } }))
    );

    component.userName = 'admin';
    component.password = 'wrongpassword';

    component.onSubmit();

    setTimeout(() => {
      expect(component.error).toBe('Invalid credentials');
      done();
    }, 100);
  });
});
