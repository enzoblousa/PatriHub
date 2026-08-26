import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { AdminUsuarios } from './admin-usuarios';

const usuarios = [
  {
    id: 'usuario-1',
    nome: 'Ana',
    email: 'ana@example.com',
    papel: 'User',
    ativo: true,
    criadoEm: '2026-01-01T00:00:00Z',
  },
  {
    id: 'admin-1',
    nome: 'Admin',
    email: 'admin@example.com',
    papel: 'Admin',
    ativo: true,
    criadoEm: '2026-01-01T00:00:00Z',
  },
];

describe('AdminUsuarios', () => {
  let httpMock: HttpTestingController;
  const usuariosUrl = `${environment.apiBaseUrl}/api/admin/usuarios`;

  beforeEach(async () => {
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: 'admin-1', nome: 'Admin', email: 'admin@example.com', papel: 'Admin' }),
    );
    await TestBed.configureTestingModule({
      imports: [AdminUsuarios],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    localStorage.clear();
    httpMock.verify();
  });

  it('carrega e lista as contas', async () => {
    const fixture = TestBed.createComponent(AdminUsuarios);
    await fixture.whenStable();

    httpMock.expectOne(usuariosUrl).flush(usuarios);
    await fixture.whenStable();

    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(linhas.length).toBe(2);
    expect(linhas[0].textContent).toContain('Ana');
    expect(linhas[0].textContent).toContain('Ativa');
  });

  it('mostra a mensagem de erro (não "nenhuma conta") quando o GET falha', async () => {
    const fixture = TestBed.createComponent(AdminUsuarios);
    await fixture.whenStable();

    httpMock
      .expectOne(usuariosUrl)
      .flush({ erro: 'Falha ao carregar contas.' }, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Falha ao carregar contas.');
    expect(texto).not.toContain('Nenhuma conta cadastrada.');
  });

  it('desabilita a ação de status pra própria conta do Admin logado', async () => {
    const fixture = TestBed.createComponent(AdminUsuarios);
    await fixture.whenStable();
    httpMock.expectOne(usuariosUrl).flush(usuarios);
    await fixture.whenStable();

    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');
    const botaoStatusAdmin = linhas[1].querySelector('button');
    expect(botaoStatusAdmin.disabled).toBe(true);
  });

  it('pede confirmação antes de desativar e só chama PATCH após confirmar', async () => {
    const fixture = TestBed.createComponent(AdminUsuarios);
    await fixture.whenStable();
    httpMock.expectOne(usuariosUrl).flush(usuarios);
    await fixture.whenStable();

    const linhaAna = fixture.nativeElement.querySelectorAll('tbody tr')[0];
    const botaoDesativar = Array.from(linhaAna.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Desativar',
    ) as HTMLButtonElement;
    botaoDesativar.click();
    await fixture.whenStable();

    httpMock.expectNone(`${usuariosUrl}/usuario-1/status`);

    const botaoConfirmar = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Confirmar',
    ) as HTMLButtonElement;
    botaoConfirmar.click();
    await fixture.whenStable();

    const req = httpMock.expectOne(`${usuariosUrl}/usuario-1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ ativo: false });
    req.flush(null);

    httpMock.expectOne(usuariosUrl).flush(usuarios);
    await fixture.whenStable();
  });

  it('reseta a senha via o formulário inline', async () => {
    const fixture = TestBed.createComponent(AdminUsuarios);
    const componente = fixture.componentInstance;
    await fixture.whenStable();
    httpMock.expectOne(usuariosUrl).flush(usuarios);
    await fixture.whenStable();

    const linhaAna = fixture.nativeElement.querySelectorAll('tbody tr')[0];
    const botaoResetar = Array.from(linhaAna.querySelectorAll('button')).find(
      (b) => (b as HTMLElement).textContent?.trim() === 'Resetar senha',
    ) as HTMLButtonElement;
    botaoResetar.click();
    await fixture.whenStable();

    componente['resetSenhaForm'].controls.novaSenha.setValue('NovaSenha123!');
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    const req = httpMock.expectOne(`${usuariosUrl}/usuario-1/resetar-senha`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ novaSenha: 'NovaSenha123!' });
    req.flush(null);
  });
});
