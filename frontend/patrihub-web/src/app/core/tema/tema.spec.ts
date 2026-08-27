import { TestBed } from '@angular/core/testing';

import { Tema } from './tema';

describe('Tema', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    TestBed.configureTestingModule({});
  });

  it('usa escuro por padrão e aplica no <html>', () => {
    const service = TestBed.inject(Tema);

    expect(service.tema()).toBe('escuro');
    expect(document.documentElement.getAttribute('data-theme')).toBe('escuro');
  });

  it('hidrata o tema salvo em localStorage', () => {
    localStorage.setItem('patrihub.tema', 'claro');

    const service = TestBed.inject(Tema);

    expect(service.tema()).toBe('claro');
    expect(document.documentElement.getAttribute('data-theme')).toBe('claro');
  });

  it('alternar troca escuro/claro, persiste e atualiza o <html>', () => {
    const service = TestBed.inject(Tema);

    service.alternar();
    expect(service.tema()).toBe('claro');
    expect(localStorage.getItem('patrihub.tema')).toBe('claro');
    expect(document.documentElement.getAttribute('data-theme')).toBe('claro');

    service.alternar();
    expect(service.tema()).toBe('escuro');
    expect(localStorage.getItem('patrihub.tema')).toBe('escuro');
    expect(document.documentElement.getAttribute('data-theme')).toBe('escuro');
  });
});
