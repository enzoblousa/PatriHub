import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { environment } from '../../../environments/environment';
import { EsqueciSenha } from './esqueci-senha';

describe('EsqueciSenha', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EsqueciSenha],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('não chama a API com o formulário inválido', async () => {
    const fixture = TestBed.createComponent(EsqueciSenha);
    await fixture.whenStable();

    fixture.nativeElement.querySelector('button[type="submit"]').click();
    await fixture.whenStable();

    httpMock.expectNone(`${environment.apiBaseUrl}/api/auth/esqueci-senha`);
  });

  it('mostra a confirmação e esconde o formulário quando o email é enviado com sucesso', async () => {
    const fixture = TestBed.createComponent(EsqueciSenha);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ email: 'ana@example.com' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    httpMock.expectOne(`${environment.apiBaseUrl}/api/auth/esqueci-senha`).flush(null);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.confirmacao')?.textContent).toContain(
      'Enviamos um link de recuperação',
    );
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });

  /** Q3 da recuperação de senha (ADR-0009): revela quando o email não existe, em vez da mensagem genérica. */
  it('mostra a mensagem do backend quando o email não existe', async () => {
    const fixture = TestBed.createComponent(EsqueciSenha);
    const componente = fixture.componentInstance;
    await fixture.whenStable();

    componente['form'].setValue({ email: 'ana@example.com' });
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    httpMock
      .expectOne(`${environment.apiBaseUrl}/api/auth/esqueci-senha`)
      .flush({ erro: 'Não existe conta com este email.' }, { status: 404, statusText: 'Not Found' });
    await fixture.whenStable();

    const mensagemErro = fixture.nativeElement.querySelector('.erro') as HTMLElement;
    expect(mensagemErro.textContent).toContain('Não existe conta com este email.');
  });
});
