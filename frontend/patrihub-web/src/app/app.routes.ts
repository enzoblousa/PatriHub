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
    path: 'lancamentos',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./lancamentos/lancamentos-lista/lancamentos-lista').then(
            (m) => m.LancamentosLista,
          ),
      },
      {
        path: 'novo',
        loadComponent: () =>
          import('./lancamentos/lancamento-form/lancamento-form').then((m) => m.LancamentoForm),
      },
      {
        path: ':id/editar',
        loadComponent: () =>
          import('./lancamentos/lancamento-form/lancamento-form').then((m) => m.LancamentoForm),
      },
    ],
  },
  {
    path: 'locatarios',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./locatarios-contratos/locatarios-lista/locatarios-lista').then(
            (m) => m.LocatariosLista,
          ),
      },
      {
        path: 'novo',
        loadComponent: () =>
          import('./locatarios-contratos/locatario-form/locatario-form').then(
            (m) => m.LocatarioForm,
          ),
      },
      {
        path: ':id/editar',
        loadComponent: () =>
          import('./locatarios-contratos/locatario-form/locatario-form').then(
            (m) => m.LocatarioForm,
          ),
      },
    ],
  },
  {
    path: 'contratos',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./locatarios-contratos/contratos-lista/contratos-lista').then(
            (m) => m.ContratosLista,
          ),
      },
      {
        path: 'novo',
        loadComponent: () =>
          import('./locatarios-contratos/contrato-form/contrato-form').then(
            (m) => m.ContratoForm,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
