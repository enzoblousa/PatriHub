import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { formatarMoeda, moedaParaNumero, MoedaDirective } from './moeda.directive';

@Component({
  imports: [ReactiveFormsModule, MoedaDirective],
  template: `<input type="text" [formControl]="controle" appMoeda />`,
})
class HostDeTeste {
  readonly controle = new FormControl(0, { nonNullable: true });
}

describe('formatarMoeda', () => {
  it('formata como moeda BRL com 2 casas decimais', () => {
    expect(formatarMoeda(0)).toBe('R$ 0,00');
    expect(formatarMoeda(1234.5)).toBe('R$ 1.234,50');
    expect(formatarMoeda(1_000_000)).toBe('R$ 1.000.000,00');
  });
});

describe('moedaParaNumero', () => {
  it('trata os 2 últimos dígitos digitados como centavos', () => {
    expect(moedaParaNumero('')).toBe(0);
    expect(moedaParaNumero('123')).toBe(1.23);
    expect(moedaParaNumero('150000')).toBe(1500);
  });

  it('ignora tudo que não é dígito', () => {
    expect(moedaParaNumero('R$ 12,00')).toBe(12);
    expect(moedaParaNumero('abc')).toBe(0);
  });
});

describe('MoedaDirective', () => {
  it('exibe o valor inicial do FormControl já formatado', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    fixture.componentInstance.controle.setValue(1234.5);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('R$ 1.234,50');
  });

  it('formata em tempo real e mantém o FormControl com o valor numérico puro', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = '150000';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(input.value).toBe('R$ 1.500,00');
    expect(fixture.componentInstance.controle.value).toBe(1500);
  });
});
