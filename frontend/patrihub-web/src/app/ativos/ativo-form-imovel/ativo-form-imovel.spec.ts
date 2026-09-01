import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AtivoFormImovel } from './ativo-form-imovel';

function ativaRotaCom(id: string | null) {
  return {
    snapshot: { paramMap: convertToParamMap(id ? { id } : {}) },
  };
}

describe('AtivoFormImovel', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  afterEach(() => {
    httpMock.verify();
  });

  describe('cadastro (sem id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [AtivoFormImovel],
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
      const fixture = TestBed.createComponent(AtivoFormImovel);
      await fixture.whenStable();

      fixture.nativeElement.querySelector('button[type="submit"]').click();
      await fixture.whenStable();

      httpMock.expectNone(`${baseUrl}/imoveis`);
    });

    it('mostra a legenda de campos obrigatórios e o asterisco nos labels obrigatórios', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      await fixture.whenStable();

      const legenda: HTMLElement = fixture.nativeElement.querySelector('.legenda-obrigatorios');
      expect(legenda.textContent).toContain('campos obrigatórios');

      const asteriscos = fixture.nativeElement.querySelectorAll('.obrigatorio');
      expect(asteriscos.length).toBeGreaterThan(0);
    });

    it('não mostra erro/borda vermelha antes de o campo ser tocado', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      await fixture.whenStable();

      expect(fixture.nativeElement.querySelector('#matricula-erro')).toBeNull();
      const input: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="matricula"]',
      );
      expect(input.classList.contains('campo-invalido')).toBe(false);
    });

    it('mostra erro inline com aria-invalid/aria-describedby quando o campo é tocado e está inválido', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.matricula.markAsTouched();
      componente['form'].controls.matricula.updateValueAndValidity();
      await fixture.whenStable();

      const input: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="matricula"]',
      );
      expect(input.classList.contains('campo-invalido')).toBe(true);
      expect(input.getAttribute('aria-invalid')).toBe('true');
      expect(input.getAttribute('aria-describedby')).toBe('matricula-erro');

      const erro: HTMLElement = fixture.nativeElement.querySelector('#matricula-erro');
      expect(erro.textContent).toContain('matrícula');
    });

    /** Ver docs/adr/0008 — antes a máscara só formatava, não impedia submeter um CEP incompleto. */
    it('rejeita CEP com menos de 8 dígitos e mostra mensagem de formato', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.endereco.controls.cep.setValue('0100');
      componente['form'].controls.endereco.controls.cep.markAsTouched();
      await fixture.whenStable();

      const erro: HTMLElement = fixture.nativeElement.querySelector('#endereco-cep-erro');
      expect(erro.textContent).toContain('8 dígitos');
    });

    it('mostra "Informe o CEP." (não a mensagem de formato) quando o campo está vazio', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.endereco.controls.cep.markAsTouched();
      await fixture.whenStable();

      const erro: HTMLElement = fixture.nativeElement.querySelector('#endereco-cep-erro');
      expect(erro.textContent).toContain('Informe o CEP.');
    });

    /** Ver docs/adr/0008 — antes qualquer par de letras passava, sem checar a lista real de UFs. */
    it('rejeita UF que não está na lista das 27, mesmo com 2 letras', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].controls.endereco.controls.uf.setValue('ZZ');
      componente['form'].controls.endereco.controls.uf.markAsTouched();
      await fixture.whenStable();

      const erro: HTMLElement = fixture.nativeElement.querySelector('#endereco-uf-erro');
      expect(erro.textContent).toContain('UF inválida');
    });

    it('aplica a máscara de moeda em tempo real no valor de aquisição', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      const input: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="valorAquisicao"]',
      );
      input.value = '30000000';
      input.dispatchEvent(new Event('input'));
      await fixture.whenStable();

      expect(input.value).toBe('R$ 300.000,00');
      expect(componente['form'].controls.valorAquisicao.value).toBe(300_000);
    });

    it('aplica a máscara de CEP e deixa a UF em maiúsculas em tempo real', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      const cep: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="cep"]',
      );
      cep.value = '01000000';
      cep.dispatchEvent(new Event('input'));

      const uf: HTMLInputElement = fixture.nativeElement.querySelector(
        'input[formcontrolname="uf"]',
      );
      uf.value = 'sp';
      uf.dispatchEvent(new Event('input'));
      await fixture.whenStable();

      expect(cep.value).toBe('01000-000');
      expect(componente['form'].controls.endereco.controls.cep.value).toBe('01000-000');
      expect(uf.value).toBe('SP');
      expect(componente['form'].controls.endereco.controls.uf.value).toBe('SP');
    });

    it('cadastra e navega pro detalhe quando aceito', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({
        apelido: 'Apê Centro',
        dataAquisicao: '2020-01-10',
        valorAquisicao: 300_000,
        valorMercadoAtual: 350_000,
        tipoImovel: 0,
        areaM2: 65,
        matricula: '12345',
        valorIptuMensal: 150,
        valorCondominioMensal: 400,
        endereco: {
          rua: 'Rua das Flores',
          numero: '123',
          complemento: '',
          bairro: 'Centro',
          cidade: 'São Paulo',
          uf: 'SP',
          cep: '01000-000',
        },
      });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/imoveis`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.financiamento).toBeNull();
      req.flush({ id: 'ativo-1' });
      await fixture.whenStable();

      expect(router.navigate).toHaveBeenCalledWith(['/ativos', 'ativo-1']);
    });

    it('mostra a mensagem de erro do backend quando a API rejeita', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({
        apelido: 'Apê Centro',
        dataAquisicao: '2020-01-10',
        valorAquisicao: 300_000,
        valorMercadoAtual: 350_000,
        tipoImovel: 0,
        areaM2: 65,
        matricula: '12345',
        valorIptuMensal: 150,
        valorCondominioMensal: 400,
        endereco: {
          rua: 'Rua das Flores',
          numero: '123',
          complemento: '',
          bairro: 'Centro',
          cidade: 'São Paulo',
          uf: 'SP',
          cep: '01000-000',
        },
      });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/imoveis`);
      req.flush(
        { erro: 'Apelido do ativo não pode ser vazio.' },
        { status: 400, statusText: 'Bad Request' },
      );
      await fixture.whenStable();

      const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
      expect(mensagemErro.textContent).toContain('Apelido do ativo não pode ser vazio.');
    });
  });

  describe('edição (com id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [AtivoFormImovel],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: ativaRotaCom('ativo-1') },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('carrega o Imóvel existente e preenche o formulário', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;

      const req = httpMock.expectOne(`${baseUrl}/ativo-1`);
      req.flush({
        id: 'ativo-1',
        apelido: 'Apê Centro',
        tipo: 0,
        status: 0,
        dataAquisicao: '2020-01-10',
        valorAquisicao: 300_000,
        valorMercadoAtual: 350_000,
        financiado: false,
        financiamento: null,
        criadoEm: '2020-01-10T00:00:00Z',
        atualizadoEm: '2020-01-10T00:00:00Z',
        imovel: {
          endereco: {
            rua: 'Rua das Flores',
            numero: '123',
            complemento: null,
            bairro: 'Centro',
            cidade: 'São Paulo',
            uf: 'SP',
            cep: '01000-000',
          },
          tipoImovel: 0,
          areaM2: 65,
          matricula: '12345',
          valorIptuMensal: 150,
          valorCondominioMensal: 400,
        },
        carro: null,
      });
      await fixture.whenStable();

      expect(componente['form'].controls.matricula.value).toBe('12345');
      expect(componente['form'].controls.endereco.controls.cidade.value).toBe('São Paulo');
    });

    it('salva via PUT /api/ativos/imoveis/{id}', async () => {
      const fixture = TestBed.createComponent(AtivoFormImovel);
      const componente = fixture.componentInstance;

      httpMock.expectOne(`${baseUrl}/ativo-1`).flush({
        id: 'ativo-1',
        apelido: 'Apê Centro',
        tipo: 0,
        status: 0,
        dataAquisicao: '2020-01-10',
        valorAquisicao: 300_000,
        valorMercadoAtual: 350_000,
        financiado: false,
        financiamento: null,
        criadoEm: '2020-01-10T00:00:00Z',
        atualizadoEm: '2020-01-10T00:00:00Z',
        imovel: {
          endereco: {
            rua: 'Rua das Flores',
            numero: '123',
            complemento: null,
            bairro: 'Centro',
            cidade: 'São Paulo',
            uf: 'SP',
            cep: '01000-000',
          },
          tipoImovel: 0,
          areaM2: 65,
          matricula: '12345',
          valorIptuMensal: 150,
          valorCondominioMensal: 400,
        },
        carro: null,
      });
      await fixture.whenStable();

      componente['form'].controls.valorMercadoAtual.setValue(420_000);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/imoveis/ativo-1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.valorMercadoAtual).toBe(420_000);
      req.flush({ id: 'ativo-1' });
    });
  });
});
