import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { LancamentosLista } from './lancamentos-lista';

const lancamento = {
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
};

describe('LancamentosLista', () => {
  let httpMock: HttpTestingController;
  const lancamentosUrl = `${environment.apiBaseUrl}/api/lancamentos`;
  const ativosUrl = `${environment.apiBaseUrl}/api/ativos`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LancamentosLista],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({}) } },
        },
      ],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega a lista sem filtro ao montar e mostra o apelido do Ativo', async () => {
    const fixture = TestBed.createComponent(LancamentosLista);
    await fixture.whenStable();

    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne((r) => r.url === lancamentosUrl).flush([lancamento]);
    await fixture.whenStable();

    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(linhas.length).toBe(1);
    expect(linhas[0].textContent).toContain('Apê Centro');
    expect(linhas[0].textContent).toContain('Receita');
    expect(linhas[0].textContent).toContain('Aluguel');
  });

  it('pede confirmação antes de excluir e só chama DELETE após confirmar', async () => {
    const fixture = TestBed.createComponent(LancamentosLista);
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
    httpMock.expectOne((r) => r.url === lancamentosUrl).flush([lancamento]);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Excluir',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    httpMock.expectNone(`${lancamentosUrl}/lancamento-1`);

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Confirmar',
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(`${lancamentosUrl}/lancamento-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);

    httpMock.expectOne((r) => r.url === lancamentosUrl).flush([]);
    await fixture.whenStable();
  });

  it('filtrar envia os valores do formulário de filtro pro service', async () => {
    const fixture = TestBed.createComponent(LancamentosLista);
    await fixture.whenStable();
    httpMock.expectOne(ativosUrl).flush([]);
    httpMock.expectOne((r) => r.url === lancamentosUrl).flush([]);
    await fixture.whenStable();

    fixture.nativeElement.querySelector('select[formControlName="tipo"]').value = '1';
    fixture.nativeElement
      .querySelector('select[formControlName="tipo"]')
      .dispatchEvent(new Event('change'));
    fixture.nativeElement.querySelector('form.filtro').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne((r) => r.url === lancamentosUrl && r.params.get('tipo') === '1');
    req.flush([]);
  });
});
