import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AdminUsuarioLancamentos } from './admin-usuario-lancamentos';

describe('AdminUsuarioLancamentos', () => {
  let httpMock: HttpTestingController;
  const lancamentosUrl = `${environment.apiBaseUrl}/api/admin/usuarios/usuario-1/lancamentos`;
  const ativosUrl = `${environment.apiBaseUrl}/api/admin/usuarios/usuario-1/ativos`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUsuarioLancamentos],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'usuario-1' }) } },
        },
      ],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega Lançamentos e Ativos do usuário alvo e mostra o apelido do Ativo', async () => {
    const fixture = TestBed.createComponent(AdminUsuarioLancamentos);
    await fixture.whenStable();

    httpMock.expectOne(lancamentosUrl).flush([
      {
        id: 'lancamento-1',
        ativoId: 'ativo-1',
        contratoId: null,
        tipo: 0,
        categoria: 0,
        valor: 1_500,
        data: '2026-03-10',
        descricao: 'Aluguel de março',
        criadoEm: '2026-03-10T00:00:00Z',
        atualizadoEm: '2026-03-10T00:00:00Z',
      },
    ]);
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    await fixture.whenStable();

    const linha = fixture.nativeElement.querySelector('tbody tr');
    expect(linha.textContent).toContain('Apê Centro');
    expect(linha.textContent).toContain('Receita');
    expect(linha.textContent).toContain('Aluguel');
  });

  it('mostra mensagem de erro quando a chamada falha', async () => {
    const fixture = TestBed.createComponent(AdminUsuarioLancamentos);
    await fixture.whenStable();

    httpMock.expectOne(ativosUrl).flush([]);
    httpMock
      .expectOne(lancamentosUrl)
      .flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.erro')).toBeTruthy();
  });
});
