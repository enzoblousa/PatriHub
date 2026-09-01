import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { CepDirective, formatarCep } from './cep.directive';

@Component({
  imports: [ReactiveFormsModule, CepDirective],
  template: `<input type="text" [formControl]="controle" appCep />`,
})
class HostDeTeste {
  readonly controle = new FormControl('', { nonNullable: true });
}

describe('formatarCep', () => {
  it('insere o traço depois do 5º dígito', () => {
    expect(formatarCep('01000000')).toBe('01000-000');
  });

  it('não insere o traço enquanto tem 5 dígitos ou menos', () => {
    expect(formatarCep('0100')).toBe('0100');
    expect(formatarCep('01000')).toBe('01000');
  });

  it('ignora tudo que não é dígito e limita a 8 dígitos', () => {
    expect(formatarCep('01.000-000extra1234')).toBe('01000-000');
  });
});

describe('CepDirective', () => {
  it('exibe o valor inicial do FormControl já formatado', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    fixture.componentInstance.controle.setValue('01000000');
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('01000-000');
  });

  it('formata em tempo real e guarda a string formatada no FormControl', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = '01000000';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(input.value).toBe('01000-000');
    expect(fixture.componentInstance.controle.value).toBe('01000-000');
  });
});
