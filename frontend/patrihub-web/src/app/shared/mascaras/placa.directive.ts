import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { NG_VALUE_ACCESSOR, type ControlValueAccessor } from '@angular/forms';

/**
 * Formata uma placa de carro brasileira, aceitando os dois formatos em uso (ver ADR-0007):
 * antigo (`AAA-0000`) e Mercosul (`AAA0A00`). Como os dois têm o mesmo prefixo de 3 letras + 1
 * dígito, não dá pra saber qual formato é até o 5º caractere ser digitado — enquanto isso é
 * ambíguo, assume-se o formato antigo (com traço); assim que aparece uma letra na 5ª posição
 * (só possível no Mercosul), o traço some sozinho.
 */
export function formatarPlaca(bruto: string): string {
  const limpo = bruto
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '')
    .slice(0, 7);
  if (limpo.length <= 3) {
    return limpo;
  }

  const letras = limpo.slice(0, 3);
  const resto = limpo.slice(3);
  const pareceFormatoAntigo = /^\d+$/.test(resto);
  return pareceFormatoAntigo ? `${letras}-${resto}` : `${letras}${resto}`;
}

/**
 * Máscara de placa (auto-uppercase + formato antigo/Mercosul) — usada em `Carro.placa` (ver
 * ADR-0007). O `FormControl` guarda a string já formatada, incluindo o traço quando aplicável;
 * o backend (`Carro.AtualizarDadosDoCarro`) só faz trim + uppercase, então mandar o traço não
 * quebra nada do lado dele.
 */
@Directive({
  selector: 'input[appPlaca]',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PlacaDirective),
      multi: true,
    },
  ],
  host: {
    '(input)': 'aoDigitar($event)',
    '(blur)': 'onTouched()',
  },
})
export class PlacaDirective implements ControlValueAccessor {
  private readonly elementoNativo = inject(ElementRef<HTMLInputElement>).nativeElement;

  private onChange: (valor: string) => void = () => {};
  protected onTouched: () => void = () => {};

  writeValue(valor: string | null): void {
    this.elementoNativo.value = formatarPlaca(valor ?? '');
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
    const valor = formatarPlaca((evento.target as HTMLInputElement).value);
    this.elementoNativo.value = valor;
    this.onChange(valor);
  }
}
