import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Dashboard } from './dashboard';
import { PatrimonioConsolidadoDto } from './dashboard.models';

describe('Dashboard', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/dashboard`;

  const patrimonio: PatrimonioConsolidadoDto = {
    lucroTotalDoMes: 3_800,
    lucroTotalAcumulado: 3_800,
    ativos: [
      {
        ativoId: 'ativo-1',
        apelido: 'Apê Centro',
        lucroDoMes: 3_800,
        lucroAcumulado: 3_800,
        yield: 3_000 / 350_000,
        roiSobreValorAquisicao: 53_800 / 300_000,
        roiSobreValorMercadoAtual: 53_800 / 350_000,
        depreciacao: -50_000,
        custoDeOportunidade: null,
        projecaoDeLucro: 2_000,
        roiSobreValorFipeAtual: null,
        divergenciaFipeAtual: null,
        alertaDivergenciaFipe: null,
      },
    ],
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

  it('não tem dashboard carregado por padrão', () => {
    const service = TestBed.inject(Dashboard);

    expect(service.dado()).toBeNull();
    expect(service.carregando()).toBe(false);
  });

  it('carregar sem taxa busca GET /api/dashboard sem query string e popula o signal', () => {
    const service = TestBed.inject(Dashboard);

    service.carregar();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys().length).toBe(0);
    req.flush(patrimonio);

    expect(service.carregando()).toBe(false);
    expect(service.dado()).toEqual(patrimonio);
  });

  it('carregar com taxa manda taxaReferenciaAnual na query string', () => {
    const service = TestBed.inject(Dashboard);

    service.carregar(0.12);

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('taxaReferenciaAnual')).toBe('0.12');
    req.flush(patrimonio);
  });

  it('carregar com erro para de carregar sem popular o dado', () => {
    const service = TestBed.inject(Dashboard);

    service.carregar();
    const req = httpMock.expectOne(baseUrl);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.dado()).toBeNull();
  });
});
