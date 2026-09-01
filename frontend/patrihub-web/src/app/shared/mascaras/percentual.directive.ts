import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';

/** Formata um número como percentual `0,00%` (ex.: `12.5` -> `"12,50%"`). */
export function formatarPercentual(valor: number): string {
  return `${valor.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}%`;
}

/** Mesma lógica de `moedaParaNumero` — os últimos 2 dígitos digitados são as casas decimais. */
export function percentualParaNumero(bruto: string): number {
  const digitos = bruto.replace(/\D/g, '');
  return digitos === '' ? 0 : Number(digitos) / 100;
}

/**
 * Máscara de percentual (`0,00%`) em tempo real — hoje só usada em
 * `Financiamento.taxaJurosAnual` (ver ADR-0007). Mesmo padrão de `MoedaDirective`: o
 * `FormControl` guarda o número puro (`12.5`, não a string formatada), o texto exibido é que
 * leva o `%`.
 */
@Directive({
  selector: 'input[appPercentual]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PercentualDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'aoDigitar($event)',
    '(blur)': 'onTouched()',
  },
})
export class PercentualDirective implements ControlValueAccessor {
  private readonly elementoNativo = inject(ElementRef<HTMLInputElement>).nativeElement;

  private onChange: (valor: number) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(valor: number | null): void {
    this.elementoNativo.value = formatarPercentual(valor ?? 0);
  }

  registerOnChange(fn: (valor: number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.elementoNativo.disabled = disabled;
  }

  protected aoDigitar(evento: Event): void {
    const valor = percentualParaNumero((evento.target as HTMLInputElement).value);
    this.elementoNativo.value = formatarPercentual(valor);
    this.onChange(valor);
  }
}
