import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';

/** Converte pra maiúsculas — sem remover nem reordenar nada. */
export function formatarMaiusculas(bruto: string): string {
  return bruto.toUpperCase();
}

/**
 * Auto-uppercase ao digitar — hoje só usada em `Imovel.endereco.uf` (ver ADR-0007), que hoje só
 * tinha `maxlength="2"` sem nenhuma transformação. Mais simples que moeda/CEP/placa porque não
 * há formatação de posição, só caixa alta.
 */
@Directive({
  selector: 'input[appMaiusculas]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => MaiusculasDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'aoDigitar($event)',
    '(blur)': 'onTouched()',
  },
})
export class MaiusculasDirective implements ControlValueAccessor {
  private readonly elementoNativo = inject(ElementRef<HTMLInputElement>).nativeElement;

  private onChange: (valor: string) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(valor: string | null): void {
    this.elementoNativo.value = formatarMaiusculas(valor ?? '');
  }

  registerOnChange(fn: (valor: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.elementoNativo.disabled = disabled;
  }

  protected aoDigitar(evento: Event): void {
    const valor = formatarMaiusculas((evento.target as HTMLInputElement).value);
    this.elementoNativo.value = valor;
    this.onChange(valor);
  }
}
