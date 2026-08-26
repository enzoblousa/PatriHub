import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AdminUsuarioAtivos } from './admin-usuario-ativos';

describe('AdminUsuarioAtivos', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/admin/usuarios/usuario-1/ativos`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminUsuarioAtivos],
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

  it('carrega e lista os Ativos do usuário alvo, sem link de edição', async () => {
    const fixture = TestBed.createComponent(AdminUsuarioAtivos);
    await fixture.whenStable();

    httpMock.expectOne(baseUrl).flush([
      {
        id: 'ativo-1',
        apelido: 'Apê Centro',
        tipo: 0,
        status: 1,
        valorMercadoAtual: 350_000,
        lucroDoMes: 1_100,
      },
    ]);
    await fixture.whenStable();

    const linha = fixture.nativeElement.querySelector('tbody tr');
    expect(linha.textContent).toContain('Apê Centro');
    expect(linha.textContent).toContain('Alugado');
    expect(fixture.nativeElement.querySelectorAll('a[href*="editar"]').length).toBe(0);
  });

  it('mostra mensagem de erro quando a chamada falha', async () => {
    const fixture = TestBed.createComponent(AdminUsuarioAtivos);
    await fixture.whenStable();

    httpMock.expectOne(baseUrl).flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.erro')).toBeTruthy();
  });
});
