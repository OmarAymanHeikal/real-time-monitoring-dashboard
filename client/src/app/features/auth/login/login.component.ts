import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  userName = '';
  password = '';
  error = '';

  constructor(public auth: AuthService, private router: Router) {}

  onSubmit(): void {
    this.error = '';
    this.auth.login(this.userName, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (e) => { this.error = (e.error && e.error.error) ? e.error.error : 'Login failed'; }
    });
  }
}
