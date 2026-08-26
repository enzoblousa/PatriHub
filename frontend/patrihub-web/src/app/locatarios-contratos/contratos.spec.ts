import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Contratos } from './contratos';
import { ContratoDto, StatusContrato } from './contratos.models';

describe('Contratos', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/contratos`;

  const contrato: ContratoDto = {
    id: 'contrato-1',
    ativoId: 'ativo-1',
    locatarioId: 'locatario-1',
    valorAluguelMensal: 1_500,
    diaVencimento: 10,
    dataInicio: '2026-01-01',
    dataFim: null,
    status: StatusContrato.Ativo,
    criadoEm: '2026-01-01T00:00:00Z',
    atualizadoEm: '2026-01-01T00:00:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não tem contratos carregados por padrão', () => {
    const service = TestBed.inject(Contratos);

    expect(service.lista()).toEqual([]);
    expect(service.carregando()).toBe(false);
  });

  it('carregarLista busca GET /api/contratos e popula o signal', () => {
    const service = TestBed.inject(Contratos);

    service.carregarLista();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([contrato]);

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([contrato]);
  });

  it('carregarLista com erro para de carregar sem popular a lista', () => {
    const service = TestBed.inject(Contratos);

    service.carregarLista();
    const req = httpMock.expectOne(baseUrl);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([]);
  });

  it('criar chama POST /api/contratos', () => {
    const service = TestBed.inject(Contratos);

    service.criar({} as never).subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    req.flush(contrato);
  });

  it('encerrar chama POST /api/contratos/{id}/encerrar', () => {
    const service = TestBed.inject(Contratos);

    service.encerrar('contrato-1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/contrato-1/encerrar`);
    expect(req.request.method).toBe('POST');
    req.flush(contrato);
  });
});
