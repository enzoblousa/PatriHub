import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AtivoFormCarro } from './ativo-form-carro';

function ativaRotaCom(id: string | null) {
  return {
    snapshot: { paramMap: convertToParamMap(id ? { id } : {}) },
  };
}

const carroValido = {
  apelido: 'Corolla',
  dataAquisicao: '2022-03-15',
  valorAquisicao: 120_000,
  valorMercadoAtual: 100_000,
  placa: 'ABC1D23',
  marca: 'Toyota',
  modelo: 'Corolla',
  anoFabricacao: 2022,
  anoModelo: 2022,
  valorFipeAtual: 105_000,
  km: 30_000,
  motorizacao: 0,
  consumoMedio: 14.5,
};

describe('AtivoFormCarro', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  afterEach(() => {
    httpMock.verify();
  });

  describe('cadastro (sem id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [AtivoFormCarro],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: ativaRotaCom(null) },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('não chama a API com o formulário inválido', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.apelido.setValue('');
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      await fixture.whenStable();

      httpMock.expectNone(`${baseUrl}/carros`);
    });

    it('mostra a legenda de campos obrigatórios e o asterisco nos labels obrigatórios', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      await fixture.whenStable();

      const legenda: HTMLElement = fixture.nativeElement.querySelector('.legenda-obrigatorios');
      expect(legenda.textContent).toContain('campos obrigatórios');

      const asteriscos = fixture.nativeElement.querySelectorAll('.obrigatorio');
      expect(asteriscos.length).toBeGreaterThan(0);
    });

    it('aplica a máscara de placa: assume formato antigo e reconhece Mercosul ao digitar', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      const input: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="placa"]',
      );

      input.value = 'abc1234';
      input.dispatchEvent(new Event('input'));
      await fixture.whenStable();
      expect(input.value).toBe('ABC-1234');
      expect(componente['form'].controls.placa.value).toBe('ABC-1234');

      input.value = 'abc1d23';
      input.dispatchEvent(new Event('input'));
      await fixture.whenStable();
      expect(input.value).toBe('ABC1D23');
      expect(componente['form'].controls.placa.value).toBe('ABC1D23');
    });

    it('rejeita anoModelo menor que anoFabricacao e mostra o erro inline', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({ ...carroValido, anoFabricacao: 2022, anoModelo: 2021 });
      componente['form'].controls.anoModelo.markAsTouched();
      fixture.nativeElement.querySelector('button[type="submit"]').click();
      await fixture.whenStable();

      httpMock.expectNone(`${baseUrl}/carros`);

      const input: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="anoModelo"]',
      );
      expect(input.classList.contains('campo-invalido')).toBe(true);
      const erro: HTMLElement = fixture.nativeElement.querySelector('#anoModelo-erro');
      expect(erro.textContent).toContain('ano de fabricação');
    });

    it('rejeita anoFabricacao fora do intervalo [1900, ano atual + 1]', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.anoFabricacao.setValue(1899);
      componente['form'].controls.anoFabricacao.markAsTouched();
      await fixture.whenStable();

      expect(componente['form'].controls.anoFabricacao.invalid).toBe(true);
      const erro: HTMLElement = fixture.nativeElement.querySelector('#anoFabricacao-erro');
      expect(erro.textContent).toContain('1900');
    });

    it('cadastra e navega pro detalhe quando aceito', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue(carroValido);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/carros`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.financiamento).toBeNull();
      req.flush({ id: 'ativo-2' });
      await fixture.whenStable();

      expect(router.navigate).toHaveBeenCalledWith(['/ativos', 'ativo-2']);
    });
  });

  describe('edição (com id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [AtivoFormCarro],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: ativaRotaCom('ativo-2') },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('carrega o Carro existente e preenche o formulário', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;

      const req = httpMock.expectOne(`${baseUrl}/ativo-2`);
      req.flush({
        id: 'ativo-2',
        apelido: 'Corolla',
        tipo: 1,
        status: 0,
        dataAquisicao: '2022-03-15',
        valorAquisicao: 120_000,
        valorMercadoAtual: 100_000,
        financiado: false,
        financiamento: null,
        criadoEm: '2022-03-15T00:00:00Z',
        atualizadoEm: '2022-03-15T00:00:00Z',
        imovel: null,
        carro: {
          placa: 'ABC1D23',
          marca: 'Toyota',
          modelo: 'Corolla',
          anoFabricacao: 2022,
          anoModelo: 2022,
          valorFipeAtual: 105_000,
          km: 30_000,
          motorizacao: 0,
          consumoMedio: 14.5,
        },
      });
      await fixture.whenStable();

      expect(componente['form'].controls.placa.value).toBe('ABC1D23');
      expect(componente['form'].controls.marca.value).toBe('Toyota');
    });

    it('salva via PUT /api/ativos/carros/{id}', async () => {
      const fixture = TestBed.createComponent(AtivoFormCarro);
      const componente = fixture.componentInstance;

      httpMock.expectOne(`${baseUrl}/ativo-2`).flush({
        id: 'ativo-2',
        apelido: 'Corolla',
        tipo: 1,
        status: 0,
        dataAquisicao: '2022-03-15',
        valorAquisicao: 120_000,
        valorMercadoAtual: 100_000,
        financiado: false,
        financiamento: null,
        criadoEm: '2022-03-15T00:00:00Z',
        atualizadoEm: '2022-03-15T00:00:00Z',
        imovel: null,
        carro: {
          placa: 'ABC1D23',
          marca: 'Toyota',
          modelo: 'Corolla',
          anoFabricacao: 2022,
          anoModelo: 2022,
          valorFipeAtual: 105_000,
          km: 30_000,
          motorizacao: 0,
          consumoMedio: 14.5,
        },
      });
      await fixture.whenStable();

      componente['form'].controls.km.setValue(35_000);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/carros/ativo-2`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.km).toBe(35_000);
      req.flush({ id: 'ativo-2' });
    });
  });
});
