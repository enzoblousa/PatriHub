import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./inicio/inicio').then((m) => m.Inicio),
    canActivate: [authGuard],
  },
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login').then((m) => m.Login),
  },
  {
    path: 'registro',
    loadComponent: () => import('./auth/registro/registro').then((m) => m.Registro),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
