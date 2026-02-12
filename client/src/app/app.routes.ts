import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authGuard] },
  { path: 'servers', loadComponent: () => import('./features/servers/server-list/server-list.component').then(m => m.ServerListComponent), canActivate: [authGuard] },
  { path: 'servers/:id', loadComponent: () => import('./features/servers/server-detail/server-detail.component').then(m => m.ServerDetailComponent), canActivate: [authGuard] },
  { path: 'alerts', loadComponent: () => import('./features/alerts/alerts-list/alerts-list.component').then(m => m.AlertsListComponent), canActivate: [authGuard] },
  { path: 'reports', loadComponent: () => import('./features/reports/reports-page/reports-page.component').then(m => m.ReportsPageComponent), canActivate: [authGuard] },
  { path: 'jobs', loadComponent: () => import('./features/jobs/jobs-page/jobs-page.component').then(m => m.JobsPageComponent), canActivate: [authGuard, adminGuard] },
  { path: 'admin/users', loadComponent: () => import('./features/admin/users-page/users-page.component').then(m => m.UsersPageComponent), canActivate: [authGuard, adminGuard] },
  { path: '**', redirectTo: 'dashboard' }
];
