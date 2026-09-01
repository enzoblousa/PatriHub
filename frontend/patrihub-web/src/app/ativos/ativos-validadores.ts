import type { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Validadores de `AtivoFormCarro` que hoje só existem no backend (`Carro.AtualizarDadosDoCarro`)
 * — issue #72 pede que o frontend reflita a mesma regra em vez de deixar o usuário descobrir só
 * no erro 400 da API. `anoMaximoCarro()` é calculado a cada chamada, não congelado no import,
 * porque o ano corrente pode virar numa sessão de longa duração.
 */
export const ANO_MINIMO_CARRO = 1900;

/**
 * Usa `getUTCFullYear()`, não `getFullYear()` (hora local) — o backend calcula o teto com
 * `DateTime.UtcNow.Year` (`Carro.cs`), e perto da virada do ano os dois divergem em fusos à
 * frente de UTC se um usar hora local e o outro UTC.
 */
export function anoMaximoCarro(): number {
  return new Date().getUTCFullYear() + 1;
}

function vazio(valor: unknown): boolean {
  return valor === null || valor === undefined || valor === ('' as unknown);
}

/** Espelha `anoFabricacao < anoMinimo || anoFabricacao > anoMaximo` do backend. */
export const anoFabricacaoValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const valor = control.value as number;
  if (vazio(valor)) {
    return null;
  }

  const maximo = anoMaximoCarro();
  return valor < ANO_MINIMO_CARRO || valor > maximo
    ? { anoForaDoIntervalo: { minimo: ANO_MINIMO_CARRO, maximo } }
    : null;
};

/**
 * Espelha `anoModelo < anoFabricacao || anoModelo > anoMaximo` do backend — validação cruzada
 * que lê o irmão `anoFabricacao` via `control.parent` (só disponível depois que o `FormGroup` é
 * montado, mas antes da primeira `updateValueAndValidity`, que é quando os validadores rodam
 * pela 1ª vez). O componente precisa reacionar essa validação manualmente quando
 * `anoFabricacao` mudar (ver `AtivoFormCarro`), porque Angular só reroda o validador de um
 * controle quando o valor dele mesmo muda.
 */
export const anoModeloValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const valor = control.value as number;
  const anoFabricacao = control.parent?.get('anoFabricacao')?.value as number | undefined;
  if (vazio(valor) || vazio(anoFabricacao)) {
    return null;
  }

  const maximo = anoMaximoCarro();
  return valor < anoFabricacao! || valor > maximo
    ? { anoModeloInvalido: { minimo: anoFabricacao, maximo } }
    : null;
};
