import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Lancamentos } from './lancamentos';
import { CategoriaLancamento, LancamentoDto, TipoLancamento } from './lancamentos.models';

describe('Lancamentos', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/lancamentos`;

  const lancamento: LancamentoDto = {
    id: 'lancamento-1',
    ativoId: 'ativo-1',
    contratoId: null,
    tipo: TipoLancamento.Receita,
    categoria: CategoriaLancamento.Aluguel,
    valor: 1_500,
    data: '2026-03-10',
    descricao: 'Aluguel de março',
    criadoEm: '2026-03-10T00:00:00Z',
    atualizadoEm: '2026-03-10T00:00:00Z',
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

  it('não tem lançamentos carregados por padrão', () => {
    const service = TestBed.inject(Lancamentos);

    expect(service.lista()).toEqual([]);
    expect(service.carregando()).toBe(false);
  });

  it('carregarLista sem filtro busca GET /api/lancamentos sem query string', () => {
    const service = TestBed.inject(Lancamentos);

    service.carregarLista();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([lancamento]);

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([lancamento]);
  });

  it('carregarLista com filtro monta a query string com ativoId, período e tipo', () => {
    const service = TestBed.inject(Lancamentos);

    service.carregarLista({
      ativoId: 'ativo-1',
      dataInicio: '2026-03-01',
      dataFim: '2026-03-31',
      tipo: TipoLancamento.Receita,
    });

    const req = httpMock.expectOne(
      (r) => r.url === baseUrl && r.params.get('ativoId') === 'ativo-1',
    );
    expect(req.request.params.get('dataInicio')).toBe('2026-03-01');
    expect(req.request.params.get('dataFim')).toBe('2026-03-31');
    expect(req.request.params.get('tipo')).toBe('0');
    req.flush([lancamento]);
  });

  it('carregarLista com erro para de carregar sem popular a lista', () => {
    const service = TestBed.inject(Lancamentos);

    service.carregarLista();
    const req = httpMock.expectOne(baseUrl);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([]);
  });

  it('obterDetalhe chama GET /api/lancamentos/{id}', () => {
    const service = TestBed.inject(Lancamentos);

    let recebido: unknown;
    service.obterDetalhe('lancamento-1').subscribe((d) => (recebido = d));

    const req = httpMock.expectOne(`${baseUrl}/lancamento-1`);
    expect(req.request.method).toBe('GET');
    req.flush(lancamento);

    expect(recebido).toEqual(lancamento);
  });

  it('criar chama POST /api/lancamentos', () => {
    const service = TestBed.inject(Lancamentos);

    service.criar({} as never).subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    req.flush(lancamento);
  });

  it('atualizar chama PUT /api/lancamentos/{id}', () => {
    const service = TestBed.inject(Lancamentos);

    service.atualizar('lancamento-1', {} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/lancamento-1`);
    expect(req.request.method).toBe('PUT');
    req.flush(lancamento);
  });

  it('excluir chama DELETE /api/lancamentos/{id}', () => {
    const service = TestBed.inject(Lancamentos);

    service.excluir('lancamento-1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/lancamento-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
