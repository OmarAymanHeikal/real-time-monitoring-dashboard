import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-server-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>Add New Server</h2>
    <mat-dialog-content>
      <form [formGroup]="serverForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Server Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g., Web Server 01">
          @if (serverForm.get('name')?.hasError('required')) {
            <mat-error>Server name is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>IP Address</mat-label>
          <input matInput formControlName="ipAddress" placeholder="e.g., 192.168.1.10">
          @if (serverForm.get('ipAddress')?.hasError('required')) {
            <mat-error>IP address is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3" placeholder="Optional description"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="!serverForm.valid || isSubmitting">
        {{ isSubmitting ? 'Creating...' : 'Create Server' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content {
      min-width: 400px;
      padding: 20px 24px;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
    }
  `]
})
export class ServerFormDialogComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private dialogRef = inject(MatDialogRef<ServerFormDialogComponent>);
  private snackBar = inject(MatSnackBar);

  isSubmitting = false;

  serverForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3)]],
    ipAddress: ['', [Validators.required]],
    description: ['']
  });

  onSubmit(): void {
    if (this.serverForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';

      this.http.post(`${apiUrl}/api/v1/servers`, this.serverForm.value).subscribe({
        next: () => {
          this.snackBar.open('Server created successfully!', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top'
          });
          this.dialogRef.close(true);
        },
        error: (err) => {
          console.error('Failed to create server:', err);
          this.snackBar.open('Failed to create server. Please try again.', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
          this.isSubmitting = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
