import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { LocatarioForm } from './locatario-form';

function rotaCom(id: string | null) {
  return { snapshot: { paramMap: convertToParamMap(id ? { id } : {}) } };
}

const locatarioValido = {
  nome: 'João Souza',
  cpf: '123.456.789-09',
  telefone: '(11) 99999-0000',
  email: 'joao@example.com',
};

describe('LocatarioForm', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  const baseUrl = `${environment.apiBaseUrl}/api/locatarios`;

  afterEach(() => {
    httpMock.verify();
  });

  describe('cadastro (sem id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LocatarioForm],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: rotaCom(null) },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    });

    it('não chama a API com o formulário inválido', async () => {
      const fixture = TestBed.createComponent(LocatarioForm);
      await fixture.whenStable();

      fixture.nativeElement.querySelector('button[type="submit"]').click();
      await fixture.whenStable();

      httpMock.expectNone(baseUrl);
    });

    it('cadastra e navega pra lista quando aceito', async () => {
      const fixture = TestBed.createComponent(LocatarioForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue(locatarioValido);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(locatarioValido);
      req.flush({ id: 'locatario-1' });
      await fixture.whenStable();

      expect(router.navigateByUrl).toHaveBeenCalledWith('/locatarios');
    });

    it('mostra a mensagem de erro do backend quando a API rejeita', async () => {
      const fixture = TestBed.createComponent(LocatarioForm);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue(locatarioValido);
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(baseUrl);
      req.flush({ erro: 'CPF deve conter 11 dígitos.' }, { status: 400, statusText: 'Bad Request' });
      await fixture.whenStable();

      const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
      expect(mensagemErro.textContent).toContain('CPF deve conter 11 dígitos.');
    });
  });

  describe('edição (com id na rota)', () => {
    beforeEach(async () => {
      await TestBed.configureTestingModule({
        imports: [LocatarioForm],
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideRouter([]),
          { provide: ActivatedRoute, useValue: rotaCom('locatario-1') },
        ],
      }).compileComponents();
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    });

    it('carrega o Locatário existente e preenche o formulário', async () => {
      const fixture = TestBed.createComponent(LocatarioForm);
      const componente = fixture.componentInstance;

      httpMock.expectOne(`${baseUrl}/locatario-1`).flush({
        id: 'locatario-1',
        ...locatarioValido,
        criadoEm: '2026-01-01T00:00:00Z',
        atualizadoEm: '2026-01-01T00:00:00Z',
      });
      await fixture.whenStable();

      expect(componente['form'].controls.nome.value).toBe('João Souza');
      expect(componente['form'].controls.cpf.value).toBe('123.456.789-09');
    });

    it('salva via PUT /api/locatarios/{id}', async () => {
      const fixture = TestBed.createComponent(LocatarioForm);
      const componente = fixture.componentInstance;

      httpMock.expectOne(`${baseUrl}/locatario-1`).flush({
        id: 'locatario-1',
        ...locatarioValido,
        criadoEm: '2026-01-01T00:00:00Z',
        atualizadoEm: '2026-01-01T00:00:00Z',
      });
      await fixture.whenStable();

      componente['form'].controls.nome.setValue('Maria Lima');
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${baseUrl}/locatario-1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.nome).toBe('Maria Lima');
      req.flush({ id: 'locatario-1' });
    });
  });
});
