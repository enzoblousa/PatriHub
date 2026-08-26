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

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('não mostra usuário nem botão de sair quando não há sessão', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.usuario')).toBeNull();
    expect(compiled.querySelector('button')).toBeNull();
  });

  it('mostra o nome do usuário e o botão de sair quando há sessão', async () => {
    localStorage.setItem('patrihub.token', 'token-jwt-fake');
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );

    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.usuario')?.textContent).toContain('Ana');
    expect(compiled.querySelector('button')?.textContent).toContain('Sair');
  });
});
