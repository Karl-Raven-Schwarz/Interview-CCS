import { Routes } from '@angular/router';

import { authGuard } from './core/auth.guard';
import { AppShellComponent } from './layout/app-shell.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    component: AppShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'entities' },
      {
        path: 'entities',
        loadComponent: () =>
          import('./features/entities/entity-list.component').then((m) => m.EntityListComponent)
      },
      {
        path: 'entities/new',
        loadComponent: () =>
          import('./features/entities/entity-form.component').then((m) => m.EntityFormComponent)
      },
      {
        path: 'entities/:id/edit',
        loadComponent: () =>
          import('./features/entities/entity-form.component').then((m) => m.EntityFormComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'entities' }
];
