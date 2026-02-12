import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

interface User {
  id: number;
  userName: string;
  email: string;
  roleName: string;
}

@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule
  ],
  templateUrl: './users-page.component.html',
  styleUrls: ['./users-page.component.scss']
})
export class UsersPageComponent implements OnInit {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);
  
  dataSource = new MatTableDataSource<User>([]);
  displayedColumns = ['userName', 'email', 'roleName', 'actions'];

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
    this.http.get<{ items: User[] }>(`${apiUrl}/api/v1/users`).subscribe({
      next: (res) => {
        this.dataSource.data = (res.items ?? res) as User[];
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(UserFormDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadUsers();
      }
    });
  }

  getRoleColor(role: string): string {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'warn';
      case 'user':
        return 'primary';
      default:
        return 'accent';
    }
  }

  deleteUser(user: User): void {
    if (confirm(`Are you sure you want to delete user "${user.userName}"?`)) {
      const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
      this.http.delete(`${apiUrl}/api/v1/users/${user.id}`).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
        }
      });
    }
  }
}

// User Form Dialog Component
@Component({
  selector: 'app-user-form-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Create New User</h2>
    <mat-dialog-content>
      <form [formGroup]="userForm">
        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Username</mat-label>
          <input matInput formControlName="userName" placeholder="e.g., johndoe">
          @if (userForm.get('userName')?.invalid && userForm.get('userName')?.touched) {
            <mat-error>Username is required (min 3 characters)</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" placeholder="e.g., johndoe@example.com">
          @if (userForm.get('email')?.invalid && userForm.get('email')?.touched) {
            <mat-error>Valid email is required</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Password</mat-label>
          <input matInput type="password" formControlName="password" placeholder="Min 6 characters">
          @if (userForm.get('password')?.invalid && userForm.get('password')?.touched) {
            <mat-error>Password is required (min 6 characters)</mat-error>
          }
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Role</mat-label>
          <mat-select formControlName="roleId">
            <mat-option [value]="1">Admin</mat-option>
            <mat-option [value]="2">User</mat-option>
          </mat-select>
          @if (userForm.get('roleId')?.invalid && userForm.get('roleId')?.touched) {
            <mat-error>Role is required</mat-error>
          }
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" [disabled]="userForm.invalid">
        Create User
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-form-field {
      margin-bottom: 16px;
    }
  `]
})
export class UserFormDialogComponent {
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);
  private dialogRef = inject(MatDialog);
  private fb = inject(FormBuilder);

  userForm: FormGroup = this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    roleId: [2, Validators.required]
  });

  onSubmit(): void {
    if (this.userForm.valid) {
      const apiUrl = (window as unknown as { env?: { apiUrl?: string } }).env?.apiUrl ?? 'http://localhost:5000';
      this.http.post(`${apiUrl}/api/v1/users`, this.userForm.value).subscribe({
        next: () => {
          this.snackBar.open('User created successfully!', 'Close', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top'
          });
          (this.dialogRef as any).closeAll();
        },
        error: (error) => {
          this.snackBar.open('Error creating user: ' + (error.error?.message || 'Unknown error'), 'Close', {
            duration: 5000,
            horizontalPosition: 'end',
            verticalPosition: 'top'
          });
        }
      });
    }
  }
}
