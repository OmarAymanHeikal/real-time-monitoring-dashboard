import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

interface Report {
  reportId: number;
  serverId: number;
  serverName?: string;
  startTime: Date;
  endTime: Date;
  status: string;
  filePath?: string;
  completedAt?: Date;
}

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatTableModule, MatSnackBarModule],
  templateUrl: './reports-page.component.html',
  styleUrls: ['./reports-page.component.scss']
})
export class ReportsPageComponent implements OnInit {
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);

  cols = ['reportId', 'serverName', 'dateRange', 'status', 'generatedAt', 'actions'];
  dataSource = new MatTableDataSource<Report>([]);
  servers: { serverId: number; name: string }[] = [];
  isGenerating = false;

  ngOnInit(): void {
    this.loadReports();
    this.loadServers();
  }

  loadReports(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: Report[] }>(`${apiUrl}/api/v1/reports`).subscribe({
      next: (res) => {
        this.dataSource.data = res.items ?? [];
      },
      error: (err) => console.error('Failed to load reports:', err)
    });
  }

  loadServers(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: any[] }>(`${apiUrl}/api/v1/servers`).subscribe({
      next: (res) => {
        this.servers = (res.items || []).map((s: any) => ({
          serverId: s.serverId,
          name: s.name
        }));
      },
      error: (err) => console.error('Failed to load servers:', err)
    });
  }

  generateReport(serverId?: number): void {
    if (this.isGenerating) return;

    this.isGenerating = true;
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    
    const now = new Date();
    const yesterday = new Date(now.getTime() - 24 * 60 * 60 * 1000);
    
    const payload = {
      serverId: serverId || this.servers[0]?.serverId || 1,
      startTime: yesterday.toISOString(),
      endTime: now.toISOString()
    };

    this.http.post(`${apiUrl}/api/v1/reports/request`, payload).subscribe({
      next: () => {
        this.snackBar.open('Report generation started!', 'Close', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top'
        });
        this.isGenerating = false;
        setTimeout(() => this.loadReports(), 2000); // Reload after delay
      },
      error: (err) => {
        console.error('Failed to generate report:', err);
        this.snackBar.open('Failed to generate report', 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
        this.isGenerating = false;
      }
    });
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'completed':
        return 'primary';
      case 'pending':
        return 'accent';
      case 'failed':
        return 'warn';
      default:
        return '';
    }
  }

  downloadReport(report: Report): void {
    if (!report.filePath) return;

    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    
    this.http.get(`${apiUrl}/api/v1/reports/${report.reportId}/download`, { 
      responseType: 'blob',
      observe: 'response'
    }).subscribe({
      next: (response) => {
        const blob = response.body;
        if (!blob) return;

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `report_${report.reportId}_${report.serverId}.html`;
        link.click();
        window.URL.revokeObjectURL(url);

        this.snackBar.open('Report downloaded successfully!', 'Close', { 
          duration: 3000,
          panelClass: ['success-snackbar']
        });
      },
      error: (err) => {
        console.error('Failed to download report:', err);
        this.snackBar.open('Failed to download report', 'Close', { 
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  deleteReport(report: Report): void {
    if (!confirm(`Are you sure you want to delete Report #${report.reportId}?`)) return;

    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    
    this.http.delete(`${apiUrl}/api/v1/reports/${report.reportId}`).subscribe({
      next: () => {
        this.snackBar.open('Report deleted successfully!', 'Close', { 
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.loadReports(); // Reload the list
      },
      error: (err) => {
        console.error('Failed to delete report:', err);
        this.snackBar.open('Failed to delete report', 'Close', { 
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }
}
