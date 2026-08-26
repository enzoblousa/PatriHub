import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router, UrlTree } from '@angular/router';

import { Auth } from '../auth/auth';
import { authGuard } from './auth-guard';

describe('authGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => authGuard(...guardParameters));

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('bloqueia e redireciona pro login quando não há sessão', () => {
    const router = TestBed.inject(Router);
    const resultado = executeGuard({} as never, { url: '/ativos' } as never);

    expect(resultado).toEqual(router.createUrlTree(['/login']));
  });

  it('libera acesso quando há sessão válida', () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    TestBed.inject(Auth);

    const resultado = executeGuard({} as never, { url: '/ativos' } as never);

    expect(resultado).toBe(true);
  });
});
