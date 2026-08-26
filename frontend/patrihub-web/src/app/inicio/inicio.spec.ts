import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { Inicio } from './inicio';

describe('Inicio', () => {
  beforeEach(async () => {
    localStorage.setItem(
      'patrihub.usuario',
      JSON.stringify({ id: '1', nome: 'Ana', email: 'ana@example.com', papel: 'User' }),
    );
    await TestBed.configureTestingModule({
      imports: [Inicio],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('cumprimenta o usuário logado pelo nome', async () => {
    const fixture = TestBed.createComponent(Inicio);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Ana');
  });
});
