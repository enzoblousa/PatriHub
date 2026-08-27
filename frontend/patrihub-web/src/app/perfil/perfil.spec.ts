import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { environment } from '../../environments/environment';
import { Perfil } from './perfil';

describe('Perfil', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const meUrl = `${environment.apiBaseUrl}/api/auth/me`;
  const contaUrl = `${environment.apiBaseUrl}/api/auth/conta`;

  const usuario = { id: 'usuario-1', nome: 'Ana', email: 'ana@example.com', papel: 'User' };

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [Perfil],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega e mostra nome/email/papel via GET /api/auth/me', async () => {
    const fixture = TestBed.createComponent(Perfil);
    await fixture.whenStable();
    httpMock.expectOne(meUrl).flush(usuario);
    await fixture.whenStable();

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('Ana');
    expect(texto).toContain('ana@example.com');
    expect(texto).toContain('User');
  });

  it('mostra a mensagem de erro quando GET /api/auth/me falha', async () => {
    const fixture = TestBed.createComponent(Perfil);
    await fixture.whenStable();
    httpMock.expectOne(meUrl).flush(null, { status: 500, statusText: 'Erro' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.erro')?.textContent).toContain(
      'Não foi possível carregar os dados da conta.',
    );
  });

  it('pede confirmação antes de excluir e só chama DELETE após confirmar', async () => {
    const fixture = TestBed.createComponent(Perfil);
    await fixture.whenStable();
    httpMock.expectOne(meUrl).flush(usuario);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Excluir minha conta',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    httpMock.expectNone(contaUrl);
    expect(fixture.nativeElement.textContent).toContain('Tem certeza');

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.includes('Confirmar exclusão'),
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(contaUrl);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    await fixture.whenStable();

    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { contaExcluida: 'true' },
    });
  });

  it('cancelar exclusão não chama DELETE', async () => {
    const fixture = TestBed.createComponent(Perfil);
    await fixture.whenStable();
    httpMock.expectOne(meUrl).flush(usuario);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Excluir minha conta',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    const botaoCancelar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Cancelar',
    ) as HTMLButtonElement;
    botaoCancelar.click();
    await fixture.whenStable();

    httpMock.expectNone(contaUrl);
    expect(fixture.nativeElement.textContent).not.toContain('Tem certeza');
  });

  it('mostra erro e mantém a confirmação fechada quando o DELETE falha', async () => {
    const fixture = TestBed.createComponent(Perfil);
    await fixture.whenStable();
    httpMock.expectOne(meUrl).flush(usuario);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Excluir minha conta',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.includes('Confirmar exclusão'),
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    httpMock.expectOne(contaUrl).flush(null, { status: 500, statusText: 'Erro' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.erro')?.textContent).toContain(
      'Não foi possível excluir sua conta.',
    );
    expect(fixture.nativeElement.textContent).not.toContain('Tem certeza');
    expect(router.navigate).not.toHaveBeenCalled();
  });
});
