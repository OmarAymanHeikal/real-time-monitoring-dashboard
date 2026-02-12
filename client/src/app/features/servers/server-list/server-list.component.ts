import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ServerFormDialogComponent } from './server-form-dialog.component';

interface Server {
  serverId: number;
  name: string;
  ipAddress: string;
  status: string;
  description?: string;
}

@Component({
  selector: 'app-server-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatChipsModule, MatCardModule, RouterLink, MatDialogModule],
  templateUrl: './server-list.component.html',
  styleUrls: ['./server-list.component.scss']
})
export class ServerListComponent implements OnInit {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);

  cols = ['name', 'ipAddress', 'status', 'action'];
  dataSource = new MatTableDataSource<Server>([]);

  ngOnInit(): void {
    this.loadServers();
  }

  loadServers(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: Server[] }>(`${apiUrl}/api/v1/servers`).subscribe({
      next: (res) => {
        this.dataSource.data = res.items ?? res as any;
      },
      error: (err) => console.error('Failed to load servers:', err)
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(ServerFormDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadServers(); // Reload after creation
      }
    });
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
