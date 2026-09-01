import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';

/** Formata um CEP como `00000-000` a partir dos dígitos digitados (até 8). */
export function formatarCep(bruto: string): string {
  const digitos = bruto.replace(/\D/g, '').slice(0, 8);
  return digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
}

/**
 * Máscara de CEP (`00000-000`) em tempo real — usada em `Imovel.endereco.cep` (ver ADR-0007).
 * O `FormControl` guarda a string já formatada (com traço): diferente de moeda/percentual, CEP
 * não tem um valor numérico "puro" que faça sentido pro domínio, é só texto.
 */
@Directive({
  selector: 'input[appCep]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CepDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'aoDigitar($event)',
    '(blur)': 'onTouched()',
  },
})
export class CepDirective implements ControlValueAccessor {
  private readonly elementoNativo = inject(ElementRef<HTMLInputElement>).nativeElement;

  private onChange: (valor: string) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(valor: string | null): void {
    this.elementoNativo.value = formatarCep(valor ?? '');
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
    const valor = formatarCep((evento.target as HTMLInputElement).value);
    this.elementoNativo.value = valor;
    this.onChange(valor);
  }
}
