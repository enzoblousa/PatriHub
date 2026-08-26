import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AtivoDetalhe } from './ativo-detalhe';

const detalheImovel = {
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
};

describe('AtivoDetalhe', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AtivoDetalhe],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'ativo-1' }) } },
        },
      ],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega e mostra o detalhe do Ativo', async () => {
    const fixture = TestBed.createComponent(AtivoDetalhe);
    await fixture.whenStable();

    httpMock.expectOne(`${baseUrl}/ativo-1`).flush(detalheImovel);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Apê Centro');
    expect(fixture.nativeElement.textContent).toContain('12345');
  });

  it('marca status Manutenção via PATCH', async () => {
    const fixture = TestBed.createComponent(AtivoDetalhe);
    await fixture.whenStable();
    httpMock.expectOne(`${baseUrl}/ativo-1`).flush(detalheImovel);
    await fixture.whenStable();

    const botaoManutencao = Array.from(
      fixture.nativeElement.querySelectorAll('button'),
    ).find((b) => (b as HTMLElement).textContent?.includes('Manutenção')) as HTMLButtonElement;
    botaoManutencao.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(`${baseUrl}/ativo-1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status: 2 });
    req.flush({ ...detalheImovel, status: 2 });
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Manutenção');
  });

  it('pede confirmação antes de excluir e só chama DELETE após confirmar', async () => {
    const fixture = TestBed.createComponent(AtivoDetalhe);
    await fixture.whenStable();
    httpMock.expectOne(`${baseUrl}/ativo-1`).flush(detalheImovel);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find((b) =>
      (b as HTMLElement).textContent?.trim() === 'Excluir',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    httpMock.expectNone(`${baseUrl}/ativo-1`);
    expect(fixture.nativeElement.textContent).toContain('Tem certeza');

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.includes('Confirmar exclusão'),
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(`${baseUrl}/ativo-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
    await fixture.whenStable();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/ativos');
  });

  it('cancelar exclusão não chama DELETE', async () => {
    const fixture = TestBed.createComponent(AtivoDetalhe);
    await fixture.whenStable();
    httpMock.expectOne(`${baseUrl}/ativo-1`).flush(detalheImovel);
    await fixture.whenStable();

    const botaoExcluir = Array.from(fixture.nativeElement.querySelectorAll('button')).find((b) =>
      (b as HTMLElement).textContent?.trim() === 'Excluir',
    ) as HTMLButtonElement;
    botaoExcluir.click();
    await fixture.whenStable();

    const botaoCancelar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Cancelar',
    ) as HTMLButtonElement;
    botaoCancelar.click();
    await fixture.whenStable();

    httpMock.expectNone(`${baseUrl}/ativo-1`);
    expect(fixture.nativeElement.textContent).not.toContain('Tem certeza');
  });
});
