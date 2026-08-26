import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AtivosLista } from './ativos-lista';
import { StatusAtivo, TipoAtivo } from '../ativos.models';

describe('AtivosLista', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AtivosLista],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega a lista de Ativos ao montar', async () => {
    const fixture = TestBed.createComponent(AtivosLista);
    await fixture.whenStable();

    const req = httpMock.expectOne(baseUrl);
    req.flush([
      {
        id: 'ativo-1',
        apelido: 'Apê Centro',
        tipo: TipoAtivo.Imovel,
        status: StatusAtivo.Alugado,
        valorMercadoAtual: 350_000,
        lucroDoMes: 1_100,
      },
    ]);
    await fixture.whenStable();

    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(linhas.length).toBe(1);
    expect(linhas[0].textContent).toContain('Apê Centro');
    expect(linhas[0].textContent).toContain('Imóvel');
    expect(linhas[0].textContent).toContain('Alugado');
  });

  it('mostra mensagem quando não há Ativos cadastrados', async () => {
    const fixture = TestBed.createComponent(AtivosLista);
    await fixture.whenStable();

    httpMock.expectOne(baseUrl).flush([]);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Você ainda não cadastrou');
  });
});
