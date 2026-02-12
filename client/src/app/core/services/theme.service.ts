import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'theme-preference';
  isDarkMode = signal<boolean>(false);

  constructor() {
    this.loadThemePreference();
    this.applyTheme();
  }

  private loadThemePreference(): void {
    const savedTheme = localStorage.getItem(this.THEME_KEY);
    if (savedTheme) {
      this.isDarkMode.set(savedTheme === 'dark');
    } else {
      // Check system preference
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.isDarkMode.set(prefersDark);
    }
  }

  toggleTheme(): void {
    this.isDarkMode.update(value => !value);
    this.saveThemePreference();
    this.applyTheme();
  }

  private saveThemePreference(): void {
    const theme = this.isDarkMode() ? 'dark' : 'light';
    localStorage.setItem(this.THEME_KEY, theme);
  }

  private applyTheme(): void {
    const theme = this.isDarkMode() ? 'dark' : 'light';
    document.documentElement.classList.remove('light', 'dark');
    document.documentElement.classList.add(theme);
    
    // Update body class for Material theme
    document.body.classList.remove('light-theme', 'dark-theme');
    document.body.classList.add(`${theme}-theme`);
  }
}
