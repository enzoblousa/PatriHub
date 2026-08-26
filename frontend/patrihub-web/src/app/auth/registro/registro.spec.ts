import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { Registro } from './registro';

describe('Registro', () => {
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Registro],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não chama a API com o formulário inválido', async () => {
    const fixture = TestBed.createComponent(Registro);
    await fixture.whenStable();

    fixture.nativeElement.querySelector('button[type="submit"]').click();
    await fixture.whenStable();

    httpMock.expectNone(`${environment.apiBaseUrl}/api/auth/registrar`);
  });

  it('registra e navega pra "/" quando o cadastro é aceito', async () => {
    const fixture = TestBed.createComponent(Registro);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ nome: 'Ana', email: 'ana@example.com', senha: 'Senha123!' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/registrar`);
    req.flush({
      sucesso: true,
      erro: null,
      token: 'token-jwt-fake',
      expiraEm: '2026-09-01T00:00:00Z',
      usuario: { id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' },
    });
    await fixture.whenStable();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });

  it('mostra a mensagem de erro do backend quando o email já existe', async () => {
    const fixture = TestBed.createComponent(Registro);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ nome: 'Ana', email: 'ana@example.com', senha: 'Senha123!' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/registrar`);
    req.flush(
      { erro: 'Já existe uma conta com este email.' },
      { status: 409, statusText: 'Conflict' },
    );
    await fixture.whenStable();

    const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
    expect(mensagemErro.textContent).toContain('Já existe uma conta com este email.');
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});
