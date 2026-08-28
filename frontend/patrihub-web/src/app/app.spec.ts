import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
  });

  function botaoComTexto(compiled: HTMLElement, texto: string): HTMLButtonElement | undefined {
    return Array.from(compiled.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === texto,
    );
  }

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('não mostra usuário, botão de sair nem sidebar quando não há sessão', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.usuario')).toBeNull();
    expect(botaoComTexto(compiled, 'Sair')).toBeUndefined();
    expect(compiled.querySelector('.sidebar')).toBeNull();
    expect(compiled.querySelector('.botao-sidebar')).toBeNull();
  });

  it('mostra o nome do usuário, o botão de sair e a sidebar quando há sessão', async () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.usuario')?.textContent).toContain('Ana');
    expect(botaoComTexto(compiled, 'Sair')).toBeTruthy();
    expect(compiled.querySelector('.sidebar')).toBeTruthy();
  });

  it('esconde o link de Admin na sidebar quando o usuário não é Admin', async () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.sidebar a[href="/admin"]')).toBeNull();
  });

  it('mostra o link de Admin na sidebar quando o usuário é Admin', async () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'Admin' }),
    );

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.sidebar a[href="/admin"]')).toBeTruthy();
  });

  it('clicar no botão de sidebar colapsa e persiste a preferência', async () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const botaoSidebar = compiled.querySelector('.botao-sidebar') as HTMLButtonElement;
    const sidebar = compiled.querySelector('.sidebar') as HTMLElement;
    expect(sidebar.classList.contains('sidebar--colapsada')).toBe(false);

    botaoSidebar.click();
    await fixture.whenStable();

    expect(sidebar.classList.contains('sidebar--colapsada')).toBe(true);
    expect(localStorage.getItem('patrihub.sidebarColapsada')).toBe('true');
  });
});
