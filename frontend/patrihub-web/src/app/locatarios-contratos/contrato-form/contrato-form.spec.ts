import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { ContratoForm } from './contrato-form';

function rotaCom(queryParams: Record<string, string> = {}) {
  return { snapshot: { queryParamMap: convertToParamMap(queryParams) } };
}

describe('ContratoForm', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const contratosUrl = `${environment.apiBaseUrl}/api/contratos`;
  const ativosUrl = `${environment.apiBaseUrl}/api/ativos`;
  const locatariosUrl = `${environment.apiBaseUrl}/api/locatarios`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContratoForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: rotaCom({ ativoId: 'ativo-1' }) },
      ],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('pré-seleciona o Ativo vindo de ?ativoId= e carrega Ativos e Locatários', async () => {
    const fixture = TestBed.createComponent(ContratoForm);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne(locatariosUrl).flush([{ id: 'locatario-1', nome: 'João Souza' }]);
    await fixture.whenStable();

    expect(componente['form'].controls.ativoId.value).toBe('ativo-1');
  });

  it('mostra o aviso de que o Ativo vira Alugado automaticamente', async () => {
    const fixture = TestBed.createComponent(ContratoForm);
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([]);
    httpMock.expectOne(locatariosUrl).flush([]);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Alugado');
  });

  it('cria o Contrato via POST /api/contratos e navega pra lista', async () => {
    const fixture = TestBed.createComponent(ContratoForm);
    const componente = fixture.componentInstance;
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne(locatariosUrl).flush([{ id: 'locatario-1', nome: 'João Souza' }]);
    await fixture.whenStable();

    componente['form'].patchValue({
      locatarioId: 'locatario-1',
      valorAluguelMensal: 1_500,
      diaVencimento: 10,
      dataInicio: '2026-01-01',
    });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(contratosUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      ativoId: 'ativo-1',
      locatarioId: 'locatario-1',
      valorAluguelMensal: 1_500,
      diaVencimento: 10,
      dataInicio: '2026-01-01',
      dataFim: null,
    });
    req.flush({ id: 'contrato-1' });
    await fixture.whenStable();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/contratos');
  });

  it('não chama a API com o formulário inválido', async () => {
    const fixture = TestBed.createComponent(ContratoForm);
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([]);
    httpMock.expectOne(locatariosUrl).flush([]);
    await fixture.whenStable();

    fixture.nativeElement.querySelector('button[type="submit"]').click();
    await fixture.whenStable();

    httpMock.expectNone(contratosUrl);
  });

  it('mostra a mensagem de erro do backend (ex: já existe Contrato Ativo pro Ativo)', async () => {
    const fixture = TestBed.createComponent(ContratoForm);
    const componente = fixture.componentInstance;
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne(locatariosUrl).flush([{ id: 'locatario-1', nome: 'João Souza' }]);
    await fixture.whenStable();

    componente['form'].patchValue({
      locatarioId: 'locatario-1',
      valorAluguelMensal: 1_500,
      diaVencimento: 10,
      dataInicio: '2026-01-01',
    });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    httpMock
      .expectOne(contratosUrl)
      .flush({ erro: 'Ativo já possui um Contrato Ativo.' }, { status: 400, statusText: 'Bad Request' });
    await fixture.whenStable();

    const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
    expect(mensagemErro.textContent).toContain('Ativo já possui um Contrato Ativo.');
  });
});
