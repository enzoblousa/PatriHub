import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { Admin } from './admin';
import { UsuarioAdminDto } from './admin.models';

describe('Admin', () => {
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/admin`;

  const usuario: UsuarioAdminDto = {
    id: 'usuario-1',
    nome: 'Ana',
    email: 'ana@example.com',
    papel: 'User',
    ativo: true,
    criadoEm: '2026-01-01T00:00:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não tem usuários carregados por padrão', () => {
    const service = TestBed.inject(Admin);

    expect(service.usuarios()).toEqual([]);
    expect(service.carregando()).toBe(false);
  });

  it('carregarUsuarios busca GET /api/admin/usuarios e popula o signal', () => {
    const service = TestBed.inject(Admin);

    service.carregarUsuarios();
    expect(service.carregando()).toBe(true);

    const req = httpMock.expectOne(`${baseUrl}/usuarios`);
    expect(req.request.method).toBe('GET');
    req.flush([usuario]);

    expect(service.carregando()).toBe(false);
    expect(service.usuarios()).toEqual([usuario]);
  });

  it('carregarUsuarios com erro para de carregar sem popular a lista', () => {
    const service = TestBed.inject(Admin);

    service.carregarUsuarios();
    const req = httpMock.expectOne(`${baseUrl}/usuarios`);
    req.flush({ erro: 'falha' }, { status: 500, statusText: 'Server Error' });

    expect(service.carregando()).toBe(false);
    expect(service.usuarios()).toEqual([]);
  });

  it('atualizarStatus chama PATCH /api/admin/usuarios/{id}/status com o novo status', () => {
    const service = TestBed.inject(Admin);

    service.atualizarStatus('usuario-1', false).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/usuarios/usuario-1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ ativo: false });
    req.flush(null);
  });

  it('resetarSenha chama POST /api/admin/usuarios/{id}/resetar-senha com a nova senha', () => {
    const service = TestBed.inject(Admin);

    service.resetarSenha('usuario-1', 'NovaSenha123!').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/usuarios/usuario-1/resetar-senha`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ novaSenha: 'NovaSenha123!' });
    req.flush(null);
  });

  it('listarAtivosDoUsuario chama GET /api/admin/usuarios/{id}/ativos', () => {
    const service = TestBed.inject(Admin);

    let recebido: unknown;
    service.listarAtivosDoUsuario('usuario-1').subscribe((r) => (recebido = r));

    const req = httpMock.expectOne(`${baseUrl}/usuarios/usuario-1/ativos`);
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 'ativo-1' }]);

    expect(recebido).toEqual([{ id: 'ativo-1' }]);
  });

  it('listarLancamentosDoUsuario chama GET /api/admin/usuarios/{id}/lancamentos', () => {
    const service = TestBed.inject(Admin);

    let recebido: unknown;
    service.listarLancamentosDoUsuario('usuario-1').subscribe((r) => (recebido = r));

    const req = httpMock.expectOne(`${baseUrl}/usuarios/usuario-1/lancamentos`);
    expect(req.request.method).toBe('GET');
    req.flush([{ id: 'lancamento-1' }]);

    expect(recebido).toEqual([{ id: 'lancamento-1' }]);
  });
});
