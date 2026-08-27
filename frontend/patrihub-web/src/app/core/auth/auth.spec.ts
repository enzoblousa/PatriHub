import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { Auth } from './auth';
import type { ResultadoAutenticacao } from './auth.models';

describe('Auth', () => {
  let httpMock: HttpTestingController;
  let router: { navigate: ReturnType<typeof vi.fn> };

  const resultadoComSucesso: ResultadoAutenticacao = {
    sucesso: true,
    erro: null,
    token: 'token-jwt-fake',
    expiraEm: '2026-09-01T00:00:00Z',
    usuario: { id: 'abc-123', nome: 'Ana', email: 'ana@example.com', papel: 'User' },
  };

  beforeEach(() => {
    localStorage.clear();
    router = { navigate: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não tem sessão por padrão', () => {
    const service = TestBed.inject(Auth);

    expect(service.estaAutenticado()).toBe(false);
    expect(service.usuario()).toBeNull();
    expect(service.obterToken()).toBeNull();
  });

  it('hidrata a sessão a partir do localStorage já existente', () => {
    localStorage.setItem('patrihub.token', resultadoComSucesso.token!);
    localStorage.setItem('patrihub.usuario', JSON.stringify(resultadoComSucesso.usuario));

    const service = TestBed.inject(Auth);

    expect(service.estaAutenticado()).toBe(true);
    expect(service.usuario()).toEqual(resultadoComSucesso.usuario);
  });

  it('login chama POST /api/auth/login e persiste token+usuario', () => {
    const service = TestBed.inject(Auth);

    let resultado: ResultadoAutenticacao | undefined;
    service
      .login({ email: 'ana@example.com', senha: 'Senha123!' })
      .subscribe((r) => (resultado = r));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(resultadoComSucesso);

    expect(resultado).toEqual(resultadoComSucesso);
    expect(service.estaAutenticado()).toBe(true);
    expect(service.usuario()).toEqual(resultadoComSucesso.usuario);
    expect(service.obterToken()).toBe('token-jwt-fake');
    expect(localStorage.getItem('patrihub.token')).toBe('token-jwt-fake');
    expect(localStorage.getItem('patrihub.usuario')).toBe(
      JSON.stringify(resultadoComSucesso.usuario),
    );
  });

  it('login com credenciais inválidas propaga o erro e não persiste sessão', () => {
    const service = TestBed.inject(Auth);

    let erro: unknown;
    service.login({ email: 'ana@example.com', senha: 'errada' }).subscribe({
      error: (e) => (erro = e),
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
    req.flush({ erro: 'Email ou senha inválidos.' }, { status: 401, statusText: 'Unauthorized' });

    expect(erro).toBeTruthy();
    expect(service.estaAutenticado()).toBe(false);
    expect(localStorage.getItem('patrihub.token')).toBeNull();
  });

  it('registrar chama POST /api/auth/registrar e persiste token+usuario', () => {
    const service = TestBed.inject(Auth);

    let resultado: ResultadoAutenticacao | undefined;
    service
      .registrar({ nome: 'Ana', email: 'ana@example.com', senha: 'Senha123!', consentimentoLgpd: true })
      .subscribe((r) => (resultado = r));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/registrar`);
    expect(req.request.method).toBe('POST');
    req.flush(resultadoComSucesso);

    expect(resultado).toEqual(resultadoComSucesso);
    expect(service.estaAutenticado()).toBe(true);
    expect(service.usuario()).toEqual(resultadoComSucesso.usuario);
  });

  it('registrar com email já existente propaga o erro e não persiste sessão', () => {
    const service = TestBed.inject(Auth);

    let erro: unknown;
    service
      .registrar({ nome: 'Ana', email: 'ana@example.com', senha: 'Senha123!', consentimentoLgpd: true })
      .subscribe({ error: (e) => (erro = e) });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/registrar`);
    req.flush(
      { erro: 'Já existe uma conta com este email.' },
      { status: 409, statusText: 'Conflict' },
    );

    expect(erro).toBeTruthy();
    expect(service.estaAutenticado()).toBe(false);
  });

  it('logout limpa o localStorage, os signals e redireciona pro login', () => {
    localStorage.setItem('patrihub.token', resultadoComSucesso.token!);
    localStorage.setItem('patrihub.usuario', JSON.stringify(resultadoComSucesso.usuario));
    const service = TestBed.inject(Auth);
    expect(service.estaAutenticado()).toBe(true);

    service.logout();

    expect(service.estaAutenticado()).toBe(false);
    expect(service.usuario()).toBeNull();
    expect(localStorage.getItem('patrihub.token')).toBeNull();
    expect(localStorage.getItem('patrihub.usuario')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('me chama GET /api/auth/me', () => {
    const service = TestBed.inject(Auth);

    let usuario: unknown;
    service.me().subscribe((u) => (usuario = u));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/me`);
    expect(req.request.method).toBe('GET');
    req.flush(resultadoComSucesso.usuario);

    expect(usuario).toEqual(resultadoComSucesso.usuario);
  });

  it('excluirConta chama DELETE /api/auth/conta e limpa a sessão, sem navegar', () => {
    localStorage.setItem('patrihub.token', resultadoComSucesso.token!);
    localStorage.setItem('patrihub.usuario', JSON.stringify(resultadoComSucesso.usuario));
    const service = TestBed.inject(Auth);

    let concluido = false;
    service.excluirConta().subscribe(() => (concluido = true));

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/conta`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    expect(concluido).toBe(true);
    expect(service.estaAutenticado()).toBe(false);
    expect(localStorage.getItem('patrihub.token')).toBeNull();
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
