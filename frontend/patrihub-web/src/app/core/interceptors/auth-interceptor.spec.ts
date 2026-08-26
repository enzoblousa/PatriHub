import { HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { Auth } from '../auth/auth';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let http: HttpClient;
  let router: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    localStorage.clear();
    router = { navigate: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: router },
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
    http = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não injeta Authorization quando não há token', () => {
    TestBed.inject(Auth);
    http.get(`${environment.apiBaseUrl}/api/auth/me`).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/me`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('injeta "Authorization: Bearer <token>" quando há sessão', () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    // Auth lê o localStorage na criação do service — precisa ser injetado depois do setItem.
    TestBed.inject(Auth);

    http.get(`${environment.apiBaseUrl}/api/ativos`).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/ativos`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-jwt-fake');
    req.flush({});
  });

  it('em resposta 401, limpa a sessão e redireciona pro login', () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    const auth = TestBed.inject(Auth);

    let erro: HttpErrorResponse | undefined;
    http.get(`${environment.apiBaseUrl}/api/ativos`).subscribe({
      error: (e) => (erro = e),
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/ativos`);
    req.flush({ erro: 'Não autorizado.' }, { status: 401, statusText: 'Unauthorized' });

    expect(erro?.status).toBe(401);
    expect(auth.estaAutenticado()).toBe(false);
    expect(localStorage.getItem('patrihub.token')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
