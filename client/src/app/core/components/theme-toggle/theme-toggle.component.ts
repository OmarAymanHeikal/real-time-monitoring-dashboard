import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  template: `
    <button 
      mat-icon-button 
      (click)="toggleTheme()"
      [matTooltip]="themeService.isDarkMode() ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
      aria-label="Toggle theme">
      <mat-icon>{{ themeService.isDarkMode() ? 'light_mode' : 'dark_mode' }}</mat-icon>
    </button>
  `,
  styles: [`
    button {
      margin: 0 8px;
    }
  `]
})
export class ThemeToggleComponent {
  themeService = inject(ThemeService);

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }
}
