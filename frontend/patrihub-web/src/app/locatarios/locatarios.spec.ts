import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Locatarios } from './locatarios';
import { LocatarioDto } from './locatarios.models';

describe('Locatarios', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/locatarios`;

  const locatario: LocatarioDto = {
    id: 'locatario-1',
    nome: 'João Souza',
    cpf: '12345678909',
    telefone: '(11) 99999-0000',
    email: 'joao@example.com',
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

  it('não tem locatários carregados por padrão', () => {
    const service = TestBed.inject(Locatarios);

    expect(service.lista()).toEqual([]);
    expect(service.carregando()).toBe(false);
  });

  it('carregarLista busca GET /api/locatarios e popula o signal', () => {
    const service = TestBed.inject(Locatarios);

    service.carregarLista();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([locatario]);

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([locatario]);
  });

  it('carregarLista com erro para de carregar sem popular a lista', () => {
    const service = TestBed.inject(Locatarios);

    service.carregarLista();
    const req = httpMock.expectOne(baseUrl);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.lista()).toEqual([]);
  });

  it('obterDetalhe chama GET /api/locatarios/{id}', () => {
    const service = TestBed.inject(Locatarios);

    let recebido: unknown;
    service.obterDetalhe('locatario-1').subscribe((d) => (recebido = d));

    const req = httpMock.expectOne(`${baseUrl}/locatario-1`);
    expect(req.request.method).toBe('GET');
    req.flush(locatario);

    expect(recebido).toEqual(locatario);
  });

  it('criar chama POST /api/locatarios', () => {
    const service = TestBed.inject(Locatarios);

    service.criar({} as never).subscribe();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    req.flush(locatario);
  });

  it('atualizar chama PUT /api/locatarios/{id}', () => {
    const service = TestBed.inject(Locatarios);

    service.atualizar('locatario-1', {} as never).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/locatario-1`);
    expect(req.request.method).toBe('PUT');
    req.flush(locatario);
  });
});
