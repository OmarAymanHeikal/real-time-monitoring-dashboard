import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AsyncPipe } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { SignalRService } from './core/services/signalr.service';
import { ThemeToggleComponent } from './core/components/theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatBadgeModule, MatIconModule, MatTooltipModule, MatSnackBarModule, AsyncPipe, ThemeToggleComponent],
  template: `
    <mat-toolbar color="primary">
      <a mat-button routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
      <a mat-button routerLink="/servers" routerLinkActive="active">Servers</a>
      <a mat-button routerLink="/alerts" routerLinkActive="active">Alerts</a>
      <a mat-button routerLink="/reports" routerLinkActive="active">Reports</a>
      @if (auth.isAdmin$() | async) {
        <a mat-button routerLink="/jobs" routerLinkActive="active">Jobs</a>
        <a mat-button routerLink="/admin/users" routerLinkActive="active">Users</a>
      }
      <span style="flex: 1"></span>
      
      <!-- User Presence Indicator -->
      <button mat-icon-button 
              [matBadge]="signalR.onlineUsersCount()" 
              matBadgeColor="accent" 
              matBadgeSize="small"
              [matTooltip]="signalR.onlineUsersCount() + ' user(s) online - Click to view'"
              matTooltipPosition="below"
              (click)="showOnlineUsers()">
        <mat-icon>people</mat-icon>
      </button>
      
      <!-- Connection Status -->
      @if (signalR.connectionStatus() === 'connected') {
        <button mat-icon-button 
                style="color: #4caf50;" 
                matTooltip="Real-time connected - Click to view details" 
                matTooltipPosition="below"
                (click)="showConnectionInfo()">
          <mat-icon>wifi</mat-icon>
        </button>
      } @else if (signalR.connectionStatus() === 'connecting') {
        <button mat-icon-button 
                style="color: #ff9800;" 
                matTooltip="Connecting to server... - Click to retry" 
                matTooltipPosition="below"
                (click)="reconnectSignalR()">
          <mat-icon>wifi_tethering</mat-icon>
        </button>
      } @else {
        <button mat-icon-button 
                style="color: #f44336;" 
                matTooltip="Disconnected - Click to reconnect" 
                matTooltipPosition="below"
                (click)="reconnectSignalR()">
          <mat-icon>wifi_off</mat-icon>
        </button>
      }
      
      <!-- Theme Toggle -->
      <app-theme-toggle></app-theme-toggle>
      
      <span>{{ auth.currentUser()?.userName }}</span>
      <button mat-button (click)="auth.logout()">Logout</button>
    </mat-toolbar>
    <main class="container">
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
    }
    :host ::ng-deep .mat-toolbar .active {
      background-color: rgba(255, 255, 255, 0.1);
    }
  `]
})
export class AppComponent {
  auth = inject(AuthService);
  signalR = inject(SignalRService);
  private snackBar = inject(MatSnackBar);

  showOnlineUsers(): void {
    const count = this.signalR.onlineUsersCount();
    const currentUser = this.auth.currentUser()?.userName || 'You';
    const message = count === 1 
      ? currentUser + ' (only you are online)'
      : count + ' users are currently online, including ' + currentUser;
    
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }

  showConnectionInfo(): void {
    const latestMetric = this.signalR.latestMetric();
    const latestAlert = this.signalR.latestAlert();
    
    let message = '✓ Real-time connection active';
    if (latestMetric) {
      const timestamp = new Date(latestMetric.timestamp).toLocaleTimeString();
      message += ' | Last metric: ' + timestamp;
    }
    if (latestAlert) {
      message += ' | Last alert: ' + latestAlert.serverName + ' - ' + latestAlert.metricType;
    }
    
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
  }

  reconnectSignalR(): void {
    const token = this.auth.getToken();
    
    if (token) {
      this.snackBar.open('Reconnecting to real-time server...', 'Close', {
        duration: 3000,
        horizontalPosition: 'center',
        verticalPosition: 'top'
      });
      
      // Stop existing connection and restart
      this.signalR.stopConnection();
      setTimeout(() => {
        this.signalR.startConnection(token);
      }, 1000);
    } else {
      // No token found - user needs to re-authenticate
      this.snackBar.open('Session expired. Please logout and login again.', 'Close', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'top',
        panelClass: ['warn-snackbar']
      });
    }
  }
}
