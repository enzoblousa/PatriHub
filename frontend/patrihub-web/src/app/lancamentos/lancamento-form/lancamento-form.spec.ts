import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  Router,
  convertToParamMap,
  provideRouter,
} from '@angular/router';

import { environment } from '../../../environments/environment';
import { LancamentoForm } from './lancamento-form';

function rotaCom(id: string | null, queryParams: Record<string, string> = {}) {
  return {
    snapshot: {
      paramMap: convertToParamMap(id ? { id } : {}),
      queryParamMap: convertToParamMap(queryParams),
    },
  };
}

describe('LancamentoForm', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const baseUrl = `${environment.apiBaseUrl}/api/lancamentos`;
  const ativosUrl = `${environment.apiBaseUrl}/api/ativos`;

  afterEach(() => {
    httpMock.verify();
  });

  describe('cadastro (sem id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LancamentoForm],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: rotaCom(null, { ativoId: 'ativo-1' }) },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('pré-seleciona o Ativo vindo de ?ativoId= e carrega a lista de Ativos', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
      await fixture.whenStable();

      expect(componente['form'].controls.ativoId.value).toBe('ativo-1');
    });

    it('troca a categoria pra primeira válida quando o Tipo muda pra Despesa', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();
      httpMock.expectOne(ativosUrl).flush([]);
      await fixture.whenStable();

      expect(componente['form'].controls.categoria.value).toBe(0); // Aluguel (Receita)

      componente['form'].controls.tipo.setValue(1); // Despesa
      await fixture.whenStable();

      expect(componente['form'].controls.categoria.value).toBe(3); // Iptu (primeira de Despesa)
    });

    it('lança e navega pra lista filtrada pelo Ativo quando aceito', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();
      httpMock.expectOne(ativosUrl).flush([{ id: 'ativo-1', apelido: 'Apê Centro' }]);
      await fixture.whenStable();

      componente['form'].patchValue({ valor: 1_500, data: '2026-03-10', descricao: 'Aluguel de março' });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        ativoId: 'ativo-1',
        tipo: 0,
        categoria: 0,
        valor: 1_500,
        data: '2026-03-10',
        descricao: 'Aluguel de março',
        contratoId: null,
      });
      req.flush({ id: 'lancamento-1' });
      await fixture.whenStable();

      expect(router.navigate).toHaveBeenCalledWith(['/lancamentos'], {
        queryParams: { ativoId: 'ativo-1' },
      });
    });

    it('não chama a API com o formulário inválido (sem Ativo selecionado)', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();
      httpMock.expectOne(ativosUrl).flush([]);
      await fixture.whenStable();

      componente['form'].controls.ativoId.setValue('');
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      await fixture.whenStable();

      httpMock.expectNone(baseUrl);
    });
  });

  describe('edição (com id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LancamentoForm],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: rotaCom('lancamento-1') },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('carrega o Lançamento existente e preenche o formulário', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      httpMock.expectOne(ativosUrl).flush([]);
      httpMock.expectOne(`${baseUrl}/lancamento-1`).flush({
        id: 'lancamento-1',
        ativoId: 'ativo-1',
        contratoId: null,
        tipo: 1,
        categoria: 4,
        valor: 400,
        data: '2026-03-12',
        descricao: 'Condomínio de março',
        criadoEm: '2026-03-12T00:00:00Z',
        atualizadoEm: '2026-03-12T00:00:00Z',
      });
      await fixture.whenStable();

      expect(componente['form'].controls.ativoId.value).toBe('ativo-1');
      expect(componente['form'].controls.tipo.value).toBe(1);
      expect(componente['form'].controls.categoria.value).toBe(4);
      expect(componente['form'].controls.valor.value).toBe(400);
    });

    it('trava o campo Ativo na edição (backend rejeita PUT que troca o AtivoId)', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      httpMock.expectOne(ativosUrl).flush([]);
      httpMock.expectOne(`${baseUrl}/lancamento-1`).flush({
        id: 'lancamento-1',
        ativoId: 'ativo-1',
        contratoId: null,
        tipo: 1,
        categoria: 4,
        valor: 400,
        data: '2026-03-12',
        descricao: 'Condomínio de março',
        criadoEm: '2026-03-12T00:00:00Z',
        atualizadoEm: '2026-03-12T00:00:00Z',
      });
      await fixture.whenStable();

      expect(componente['form'].controls.ativoId.disabled).toBe(true);
    });

    it('salva via PUT /api/lancamentos/{id} preservando o ContratoId original', async () => {
      const fixture = TestBed.createComponent(LancamentoForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      httpMock.expectOne(ativosUrl).flush([]);
      httpMock.expectOne(`${baseUrl}/lancamento-1`).flush({
        id: 'lancamento-1',
        ativoId: 'ativo-1',
        contratoId: 'contrato-1',
        tipo: 1,
        categoria: 4,
        valor: 400,
        data: '2026-03-12',
        descricao: 'Condomínio de março',
        criadoEm: '2026-03-12T00:00:00Z',
        atualizadoEm: '2026-03-12T00:00:00Z',
      });
      await fixture.whenStable();

      componente['form'].controls.valor.setValue(450);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/lancamento-1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.valor).toBe(450);
      expect(req.request.body.ativoId).toBe('ativo-1');
      expect(req.request.body.contratoId).toBe('contrato-1');
      req.flush({ id: 'lancamento-1' });
    });
  });
});
