import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'projects', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/projects/projects-dashboard/projects-dashboard.component').then(
            m => m.ProjectsDashboardComponent
          )
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./features/projects/project-detail/project-detail.component').then(
            m => m.ProjectDetailComponent
          )
      }
    ]
  },
  { path: '**', redirectTo: 'projects' }
];
