import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { Login } from './login';

describe('Login', () => {
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Login],
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
    const fixture = TestBed.createComponent(Login);
    await fixture.whenStable();

    const botao = fixture.nativeElement.querySelector('button[type="submit"]') as HTMLButtonElement;
    botao.click();
    await fixture.whenStable();

    httpMock.expectNone(`${environment.apiBaseUrl}/api/auth/login`);
  });

  it('faz login e navega pra "/" quando as credenciais são válidas', async () => {
    const fixture = TestBed.createComponent(Login);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ email: 'ana@example.com', senha: 'Senha123!' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
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

  it('mostra a mensagem de erro do backend quando o login falha', async () => {
    const fixture = TestBed.createComponent(Login);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ email: 'ana@example.com', senha: 'errada' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/login`);
    req.flush({ erro: 'Email ou senha inválidos.' }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();

    const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
    expect(mensagemErro.textContent).toContain('Email ou senha inválidos.');
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});

describe('Login vindo da exclusão de conta', () => {
  it('mostra a mensagem de confirmação quando ?contaExcluida=true', async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ contaExcluida: 'true' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Login);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.confirmacao')?.textContent).toContain(
      'Sua conta foi excluída com sucesso.',
    );
  });
});

describe('Login vindo da redefinição de senha', () => {
  it('mostra a mensagem de confirmação quando ?senhaRedefinida=true', async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ senhaRedefinida: 'true' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Login);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.confirmacao')?.textContent).toContain(
      'Senha redefinida com sucesso.',
    );
  });
});
