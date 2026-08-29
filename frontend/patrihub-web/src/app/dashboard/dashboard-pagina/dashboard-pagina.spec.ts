import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { DashboardPagina } from './dashboard-pagina';

const patrimonio = {
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

describe('DashboardPagina', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/dashboard`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardPagina],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega o dashboard sem taxa ao montar e mostra totais + métricas por Ativo', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    await fixture.whenStable();

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.keys().length).toBe(0);
    req.flush(patrimonio);
    await fixture.whenStable();

    const texto = fixture.nativeElement.textContent as string;
    expect(texto).toContain('Apê Centro');
    expect(texto).toContain('Lucro total do mês');
  });

  it('mostra custo de oportunidade como travessão quando null', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    await fixture.whenStable();
    httpMock.expectOne((r) => r.url === baseUrl).flush(patrimonio);
    await fixture.whenStable();

    const ultimaCelula = fixture.nativeElement.querySelector('tbody tr td:last-child');
    expect(ultimaCelula.textContent.trim()).toBe('—');
  });

  it('mostra ROI (FIPE) e Divergência FIPE como travessão quando o Ativo não é Carro', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    await fixture.whenStable();
    httpMock.expectOne((r) => r.url === baseUrl).flush(patrimonio);
    await fixture.whenStable();

    const celulas = fixture.nativeElement.querySelectorAll('tbody tr td') as NodeListOf<HTMLElement>;
    // ROI (FIPE) é a 7ª coluna, Divergência FIPE a 8ª (Ativo, Lucro do mês, Lucro acumulado,
    // Yield, ROI aquisição, ROI mercado, ROI FIPE, Divergência FIPE, …).
    expect(celulas[6].textContent?.trim()).toBe('—');
    expect(celulas[7].textContent?.trim()).toBe('—');
  });

  it('destaca a Divergência FIPE quando ultrapassa o limiar de alerta', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    await fixture.whenStable();
    httpMock.expectOne((r) => r.url === baseUrl).flush({
      ...patrimonio,
      ativos: [
        {
          ...patrimonio.ativos[0],
          roiSobreValorFipeAtual: -5_000 / 80_000,
          divergenciaFipeAtual: 0.417,
          alertaDivergenciaFipe: true,
        },
      ],
    });
    await fixture.whenStable();

    const celulas = fixture.nativeElement.querySelectorAll('tbody tr td') as NodeListOf<HTMLElement>;
    const badge = celulas[7].querySelector('.badge-status');
    expect(badge?.classList).toContain('badge-status--risco');
  });

  it('recalcular converte o percentual digitado em fração e refaz a chamada', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    const componente = fixture.componentInstance;
    await fixture.whenStable();
    httpMock.expectOne((r) => r.url === baseUrl).flush(patrimonio);
    await fixture.whenStable();

    componente['form'].controls.taxaReferenciaAnualPercentual.setValue('12');
    fixture.nativeElement.querySelector('form.taxa-referencia').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('taxaReferenciaAnual')).toBe('0.12');
    req.flush({ ...patrimonio, ativos: [{ ...patrimonio.ativos[0], custoDeOportunidade: 42_000 }] });
  });

  it('mostra mensagem quando não há Ativos cadastrados', async () => {
    const fixture = TestBed.createComponent(DashboardPagina);
    await fixture.whenStable();

    httpMock
      .expectOne((r) => r.url === baseUrl)
      .flush({ lucroTotalDoMes: 0, lucroTotalAcumulado: 0, ativos: [] });
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Você ainda não cadastrou');
  });
});
