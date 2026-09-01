import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';

/**
 * Formata um número como moeda BRL (ex.: `1234.5` -> `"R$ 1.234,50"`). Troca o espaço
 * não-quebrável que o `Intl.NumberFormat` coloca entre "R$" e o número por um espaço normal —
 * mais previsível pra comparação em teste e pra copiar/colar o valor.
 */
export function formatarMoeda(valor: number): string {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
    .format(valor)
    .replace(' ', ' ');
}

/**
 * Extrai o valor em reais a partir do que foi digitado — os últimos 2 dígitos são sempre os
 * centavos (mesma UX de app de banco: digitar "12345" vira "R$ 123,45").
 */
export function moedaParaNumero(bruto: string): number {
  const digitos = bruto.replace(/\D/g, '');
  return digitos === '' ? 0 : Number(digitos) / 100;
}

/**
 * Máscara de moeda (R$) em tempo real pros campos monetários de Ativo/Financiamento (ver
 * ADR-0007) — sem depender de lib de terceiros (`ngx-mask` etc., ver ADR). Implementa
 * `ControlValueAccessor` pra manter o `FormControl` com o valor numérico puro (`1234.5`)
 * enquanto exibe o texto formatado ("R$ 1.234,50") no input; por isso o input precisa ser
 * `type="text"` (não `type="number"`, que rejeita os caracteres da formatação).
 *
 * Limitação aceita: o cursor sempre volta pro fim do texto a cada tecla, porque o valor exibido
 * é recalculado do zero a cada dígito — suficiente pra esses campos, que são digitados da
 * esquerda pra direita sem edição no meio.
 */
@Directive({
  selector: 'input[appMoeda]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MoedaDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'aoDigitar($event)',
    '(blur)': 'onTouched()',
  },
})
export class MoedaDirective implements ControlValueAccessor {
  private readonly elementoNativo = inject(ElementRef<HTMLInputElement>).nativeElement;

  private onChange: (valor: number) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(valor: number | null): void {
    this.elementoNativo.value = formatarMoeda(valor ?? 0);
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
    const valor = moedaParaNumero((evento.target as HTMLInputElement).value);
    this.elementoNativo.value = formatarMoeda(valor);
    this.onChange(valor);
  }
}
