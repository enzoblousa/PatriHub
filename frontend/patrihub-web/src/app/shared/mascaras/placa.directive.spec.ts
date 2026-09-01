import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { formatarPlaca, PlacaDirective } from './placa.directive';

@Component({
  imports: [ReactiveFormsModule, PlacaDirective],
  template: `<input type="text" [formControl]="controle" appPlaca />`,
})
class HostDeTeste {
  readonly controle = new FormControl('', { nonNullable: true });
}

describe('formatarPlaca', () => {
  it('deixa sem traço enquanto só há as 3 letras', () => {
    expect(formatarPlaca('ab')).toBe('AB');
    expect(formatarPlaca('abc')).toBe('ABC');
  });

  it('assume o formato antigo (com traço) enquanto o resto for só dígitos', () => {
    expect(formatarPlaca('abc1')).toBe('ABC-1');
    expect(formatarPlaca('abc1234')).toBe('ABC-1234');
  });

  it('reconhece o Mercosul (sem traço) assim que aparece uma letra na 5ª posição', () => {
    expect(formatarPlaca('abc1d')).toBe('ABC1D');
    expect(formatarPlaca('abc1d23')).toBe('ABC1D23');
  });

  it('faz auto-uppercase, ignora caracteres inválidos e limita a 7 caracteres úteis', () => {
    expect(formatarPlaca('a!b#c-123456')).toBe('ABC-1234');
  });
});

describe('PlacaDirective', () => {
  it('exibe o valor inicial do FormControl já formatado', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    fixture.componentInstance.controle.setValue('abc1234');
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('ABC-1234');
  });

  it('formata em tempo real e guarda a string formatada no FormControl', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'abc1d23';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(input.value).toBe('ABC1D23');
    expect(fixture.componentInstance.controle.value).toBe('ABC1D23');
  });
});
