import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { RedefinirSenha } from './redefinir-senha';

function configurarTestBed(queryParams: Record<string, string>) {
  return TestBed.configureTestingModule({
    imports: [RedefinirSenha],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideRouter([]),
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { queryParamMap: convertToParamMap(queryParams) } },
      },
    ],
  }).compileComponents();
}

describe('RedefinirSenha', () => {
  let httpMock: HttpTestingController;
  let router: Router;

  afterEach(() => {
    httpMock.verify();
  });

  it('mostra "link inválido" quando faltam email ou token na URL', async () => {
    await configurarTestBed({});
    httpMock = TestBed.inject(HttpTestingController);

    const fixture = TestBed.createComponent(RedefinirSenha);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('.erro')?.textContent).toContain(
      'Link de recuperação inválido',
    );
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });

  describe('com email e token válidos na URL', () => {
    beforeEach(async () => {
      await configurarTestBed({ email: 'ana@example.com', token: 'token-abc' });
      httpMock = TestBed.inject(HttpTestingController);
      router = TestBed.inject(Router);
      vi.spyOn(router, 'navigate').mockResolvedValue(true);
    });

    it('não chama a API com uma senha fraca', async () => {
      const fixture = TestBed.createComponent(RedefinirSenha);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({ novaSenha: 'fraca' });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      httpMock.expectNone(`${environment.apiBaseUrl}/api/auth/redefinir-senha`);
    });

    it('mostra a mensagem específica por regra de senha violada', async () => {
      const fixture = TestBed.createComponent(RedefinirSenha);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({ novaSenha: 'fraca' });
      componente['form'].controls.novaSenha.markAsTouched();
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('.campo-erro')?.textContent).toContain(
        'pelo menos 8 caracteres',
      );
    });

    it('redefine a senha e navega pro login com ?senhaRedefinida=true', async () => {
      const fixture = TestBed.createComponent(RedefinirSenha);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({ novaSenha: 'SenhaForte123!' });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/redefinir-senha`);
      expect(req.request.body).toEqual({
        email: 'ana@example.com',
        token: 'token-abc',
        novaSenha: 'SenhaForte123!',
      });
      req.flush(null);
      await fixture.whenStable();

      expect(router.navigate).toHaveBeenCalledWith(['/login'], {
        queryParams: { senhaRedefinida: 'true' },
      });
    });

    it('mostra a mensagem de erro do backend quando o token é inválido', async () => {
      const fixture = TestBed.createComponent(RedefinirSenha);
      const componente = fixture.componentInstance;
      await fixture.whenStable();

      componente['form'].setValue({ novaSenha: 'SenhaForte123!' });
      fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
      await fixture.whenStable();

      httpMock
        .expectOne(`${environment.apiBaseUrl}/api/auth/redefinir-senha`)
        .flush({ erro: 'Token inválido.' }, { status: 400, statusText: 'Bad Request' });
      await fixture.whenStable();

      expect(fixture.nativeElement.querySelector('.erro')?.textContent).toContain('Token inválido.');
      expect(router.navigate).not.toHaveBeenCalled();
    });
  });
});
