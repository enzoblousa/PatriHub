import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { LocatariosLista } from './locatarios-lista';

describe('LocatariosLista', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/locatarios`;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LocatariosLista],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('carrega a lista de Locatários ao montar', async () => {
    const fixture = TestBed.createComponent(LocatariosLista);
    await fixture.whenStable();

    const req = httpMock.expectOne(baseUrl);
    req.flush([
      {
        id: 'locatario-1',
        nome: 'João Souza',
        cpf: '12345678909',
        telefone: '(11) 99999-0000',
        email: 'joao@example.com',
        criadoEm: '2026-01-01T00:00:00Z',
        atualizadoEm: '2026-01-01T00:00:00Z',
      },
    ]);
    await fixture.whenStable();

    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(linhas.length).toBe(1);
    expect(linhas[0].textContent).toContain('João Souza');
  });

  it('mostra mensagem quando não há Locatários cadastrados', async () => {
    const fixture = TestBed.createComponent(LocatariosLista);
    await fixture.whenStable();

    httpMock.expectOne(baseUrl).flush([]);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('Você ainda não cadastrou');
  });
});
