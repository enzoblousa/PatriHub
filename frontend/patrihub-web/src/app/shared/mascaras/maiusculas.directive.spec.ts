import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { formatarMaiusculas, MaiusculasDirective } from './maiusculas.directive';

@Component({
  imports: [ReactiveFormsModule, MaiusculasDirective],
  template: `<input type="text" [formControl]="controle" appMaiusculas />`,
})
class HostDeTeste {
  readonly controle = new FormControl('', { nonNullable: true });
}

describe('formatarMaiusculas', () => {
  it('converte pra maiúsculas', () => {
    expect(formatarMaiusculas('sp')).toBe('SP');
  });
});

describe('MaiusculasDirective', () => {
  it('exibe o valor inicial do FormControl já em maiúsculas', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    fixture.componentInstance.controle.setValue('sp');
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    expect(input.value).toBe('SP');
  });

  it('converte em tempo real e guarda em maiúsculas no FormControl', async () => {
    const fixture = TestBed.createComponent(HostDeTeste);
    await fixture.whenStable();

    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    input.value = 'sp';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();

    expect(input.value).toBe('SP');
    expect(fixture.componentInstance.controle.value).toBe('SP');
  });
});
