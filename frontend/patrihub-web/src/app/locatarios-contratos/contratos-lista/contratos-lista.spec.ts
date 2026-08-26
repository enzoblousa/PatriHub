import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { ContratosLista } from './contratos-lista';

const contrato = {
  id: 'contrato-1',
  ativoId: 'ativo-1',
  locatarioId: 'locatario-1',
  valorAluguelMensal: 1_500,
  diaVencimento: 10,
  dataInicio: '2026-01-01',
  dataFim: null,
  status: 0,
  criadoEm: '2026-01-01T00:00:00Z',
  atualizadoEm: '2026-01-01T00:00:00Z',
};

describe('ContratosLista', () => {
  let httpMock: HttpTestingController;
  const contratosUrl = `${environment.apiBaseUrl}/api/contratos`;
  const ativosUrl = `${environment.apiBaseUrl}/api/ativos`;
  const locatariosUrl = `${environment.apiBaseUrl}/api/locatarios`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContratosLista],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega e mostra Ativo/Locatário/Status legíveis', async () => {
    const fixture = TestBed.createComponent(ContratosLista);
    await fixture.whenStable();

    httpMock.expectOne(contratosUrl).flush([contrato]);
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne(locatariosUrl).flush([{ id: 'locatario-1', nome: 'João Souza' }]);
    await fixture.whenStable();

    const linha = fixture.nativeElement.querySelector('tbody tr');
    expect(linha.textContent).toContain('Apê Centro');
    expect(linha.textContent).toContain('João Souza');
    expect(linha.textContent).toContain('Ativo');
  });

  it('não mostra ação de Encerrar pra Contrato já Encerrado', async () => {
    const fixture = TestBed.createComponent(ContratosLista);
    await fixture.whenStable();

    httpMock.expectOne(contratosUrl).flush([{ ...contrato, status: 1 }]);
    httpMock.expectOne(ativosUrl).flush([]);
    httpMock.expectOne(locatariosUrl).flush([]);
    await fixture.whenStable();

    const botoes = fixture.nativeElement.querySelectorAll('tbody button');
    expect(botoes.length).toBe(0);
  });

  it('pede confirmação antes de encerrar e só chama POST /encerrar após confirmar', async () => {
    const fixture = TestBed.createComponent(ContratosLista);
    await fixture.whenStable();

    httpMock.expectOne(contratosUrl).flush([contrato]);
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne(locatariosUrl).flush([{ id: 'locatario-1', nome: 'João Souza' }]);
    await fixture.whenStable();

    const botaoEncerrar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Encerrar',
    ) as HTMLButtonElement;
    botaoEncerrar.click();
    await fixture.whenStable();

    httpMock.expectNone(`${contratosUrl}/contrato-1/encerrar`);

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Confirmar',
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(`${contratosUrl}/contrato-1/encerrar`);
    expect(req.request.method).toBe('POST');
    req.flush({ ...contrato, status: 1 });

    httpMock.expectOne(contratosUrl).flush([{ ...contrato, status: 1 }]);
    httpMock.expectOne(ativosUrl).flush([]);
    await fixture.whenStable();
  });
});
