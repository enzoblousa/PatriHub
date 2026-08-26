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
    path: 'ativos',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./ativos/ativos-lista/ativos-lista').then((m) => m.AtivosLista),
      },
      {
        path: 'imoveis/novo',
        loadComponent: () =>
          import('./ativos/ativo-form-imovel/ativo-form-imovel').then((m) => m.AtivoFormImovel),
      },
      {
        path: 'imoveis/:id/editar',
        loadComponent: () =>
          import('./ativos/ativo-form-imovel/ativo-form-imovel').then((m) => m.AtivoFormImovel),
      },
      {
        path: 'carros/novo',
        loadComponent: () =>
          import('./ativos/ativo-form-carro/ativo-form-carro').then((m) => m.AtivoFormCarro),
      },
      {
        path: 'carros/:id/editar',
        loadComponent: () =>
          import('./ativos/ativo-form-carro/ativo-form-carro').then((m) => m.AtivoFormCarro),
      },
      {
        path: ':id',
        loadComponent: () =>
          import('./ativos/ativo-detalhe/ativo-detalhe').then((m) => m.AtivoDetalhe),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
