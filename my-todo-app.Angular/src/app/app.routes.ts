import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { loggedInGuard } from './core/guards/logged-in.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    canActivate: [loggedInGuard],
    loadComponent: () =>
      import('./features/user-management/login/login').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    canActivate: [loggedInGuard],
    loadComponent: () =>
      import('./features/user-management/register/register').then(m => m.RegisterComponent),
  },
  {
    path: 'forgot-password',
    canActivate: [loggedInGuard],
    loadComponent: () =>
      import('./features/user-management/forgot-password/forgot-password').then(m => m.ForgotPasswordComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/topnav/topnav').then(m => m.TopnavComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard').then(m => m.DashboardComponent),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/user-management/users/users').then(m => m.UsersComponent),
      },
      {
        path: 'tasks',
        loadComponent: () =>
          import('./features/task-management/tasks/tasks').then(m => m.TasksComponent),
      },
    ],
  },
  {
    path: '**',
    loadComponent: () =>
      import('./shared/error/error').then(m => m.ErrorComponent),
  },
];
