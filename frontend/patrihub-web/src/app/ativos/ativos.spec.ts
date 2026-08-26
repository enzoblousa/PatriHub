import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Ativos } from './ativos';
import { AtivoResumoDto, StatusAtivo, TipoAtivo } from './ativos.models';

describe('Ativos', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  const resumo: AtivoResumoDto = {
    id: 'ativo-1',
    apelido: 'Apê Centro',
    tipo: TipoAtivo.Imovel,
    status: StatusAtivo.Vago,
    valorMercadoAtual: 350_000,
    lucroDoMes: 1_100,
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

  it('não tem ativos carregados por padrão', () => {
    const service = TestBed.inject(Ativos);

    expect(service.lista()).toEqual([]);
    expect(service.carregando()).toBe(false);
  });

  it('carregarLista busca GET /api/ativos e popula o signal', () => {
    const service = TestBed.inject(Ativos);

    service.carregarLista();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([resumo]);

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([resumo]);
  });

  it('carregarLista com erro para de carregar sem popular a lista', () => {
    const service = TestBed.inject(Ativos);

    service.carregarLista();
    const req = httpMock.expectOne(baseUrl);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([]);
  });

  it('obterDetalhe chama GET /api/ativos/{id}', () => {
    const service = TestBed.inject(Ativos);

    let recebido: unknown;
    service.obterDetalhe('ativo-1').subscribe((detalhe) => (recebido = detalhe));

    const req = httpMock.expectOne(`${baseUrl}/ativo-1`);
    expect(req.request.method).toBe('GET');
    req.flush({ id: 'ativo-1' });

    expect(recebido).toEqual({ id: 'ativo-1' });
  });

  it('criarImovel chama POST /api/ativos/imoveis', () => {
    const service = TestBed.inject(Ativos);

    service.criarImovel({} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/imoveis`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('criarCarro chama POST /api/ativos/carros', () => {
    const service = TestBed.inject(Ativos);

    service.criarCarro({} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/carros`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('atualizarImovel chama PUT /api/ativos/imoveis/{id}', () => {
    const service = TestBed.inject(Ativos);

    service.atualizarImovel('ativo-1', {} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/imoveis/ativo-1`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('atualizarCarro chama PUT /api/ativos/carros/{id}', () => {
    const service = TestBed.inject(Ativos);

    service.atualizarCarro('ativo-1', {} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/carros/ativo-1`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('marcarStatus chama PATCH /api/ativos/{id}/status com o status no corpo', () => {
    const service = TestBed.inject(Ativos);

    service.marcarStatus('ativo-1', StatusAtivo.Manutencao).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/ativo-1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: StatusAtivo.Manutencao });
    req.flush({});
  });

  it('excluir chama DELETE /api/ativos/{id}', () => {
    const service = TestBed.inject(Ativos);

    service.excluir('ativo-1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/ativo-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
