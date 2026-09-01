import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import {
  formatarPercentual,
  percentualParaNumero,
  PercentualDirective,
} from './percentual.directive';

@Component({
  imports: [ReactiveFormsModule, PercentualDirective],
  template: `<input type="text" [formControl]="controle" appPercentual />`,
})
class HostDeTeste {
  readonly controle = new FormControl(0, { nonNullable: true });
}

describe('formatarPercentual', () => {
  it('formata com 2 casas decimais e o sinal de %', () => {
    expect(formatarPercentual(0)).toBe('0,00%');
    expect(formatarPercentual(12.5)).toBe('12,50%');
  });
});

describe('percentualParaNumero', () => {
  it('trata os 2 últimos dígitos digitados como casas decimais', () => {
    expect(percentualParaNumero('')).toBe(0);
    expect(percentualParaNumero('1250')).toBe(12.5);
  });
});

describe('PercentualDirective', () => {
  it('exibe o valor inicial do FormControl já formatado', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    fixture.componentInstance.controle.setValue(12.5);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('12,50%');
  });

  it('formata em tempo real e mantém o FormControl com o valor numérico puro', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = '1234';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(input.value).toBe('12,34%');
    expect(fixture.componentInstance.controle.value).toBe(12.34);
  });
});
