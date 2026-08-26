import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';

import { Auth } from '../auth/auth';
import { adminGuard } from './admin-guard';

describe('adminGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => adminGuard(...guardParameters));

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('bloqueia e redireciona pro login quando não há sessão', () => {
    const router = TestBed.inject(Router);
    const resultado = executeGuard({} as never, { url: '/admin' } as never);

    expect(resultado).toEqual(router.createUrlTree(['/login']));
  });

  it('bloqueia usuário autenticado sem papel Admin', () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );
    TestBed.inject(Auth);
    const router = TestBed.inject(Router);

    const resultado = executeGuard({} as never, { url: '/admin' } as never);

    expect(resultado).toEqual(router.createUrlTree(['/login']));
  });

  it('libera acesso pra usuário com papel Admin', () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Admin', email: 'admin@patrihub.local', papel: 'Admin' }),
    );
    TestBed.inject(Auth);

    const resultado = executeGuard({} as never, { url: '/admin' } as never);

    expect(resultado).toBe(true);
  });
});
