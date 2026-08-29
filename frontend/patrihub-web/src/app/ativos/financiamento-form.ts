import { FormBuilder, Validators } from '@angular/forms';

import type { DadosFinanciamentoDto } from './ativos.models';

/**
 * Textos explicativos (ver `shared/ajuda`) dos campos de financiamento — centralizados aqui,
 * não copiados em cada tela, porque o fieldset de Financiamento é duplicado em
 * `AtivoFormCarro`/`AtivoFormImovel` (mesmo padrão de `ROTULOS_*` em `ativos-rotulos.ts`).
 */
export const TEXTOS_AJUDA_FINANCIAMENTO = {
  saldoDevedor:
    'Quanto ainda falta pagar do financiamento hoje — não é a soma das parcelas restantes, é o valor de quitação.',
  taxaJurosAnual: 'Percentual de juros cobrado por ano pelo financiamento (ex.: 12 = 12% ao ano).',
  parcelasRestantes: 'Quantas parcelas ainda faltam pagar até quitar o financiamento.',
} as const;

/**
 * Subgrupo de campos de financiamento, compartilhado entre `AtivoFormImovel` e
 * `AtivoFormCarro` — os dois têm o mesmo `DadosFinanciamentoDto` opcional (ver
 * `AtivoDtos.cs`). O grupo em si nunca é `disabled`: a tela decide se manda os valores pro
 * backend (campo `financiamento`, `null` quando "tem financiamento?" está desmarcado).
 */
export function criarFinanciamentoForm(formBuilder: FormBuilder) {
  return formBuilder.nonNullable.group({
    valorParcela: [0, [Validators.required, Validators.min(0)]],
    saldoDevedor: [0, [Validators.required, Validators.min(0)]],
    taxaJurosAnual: [0, [Validators.required, Validators.min(0)]],
    parcelasRestantes: [0, [Validators.required, Validators.min(0)]],
  });
}

export type FinanciamentoForm = ReturnType<typeof criarFinanciamentoForm>;

export function financiamentoFormParaDto(
  form: FinanciamentoForm,
  temFinanciamento: boolean,
): DadosFinanciamentoDto | null {
  return temFinanciamento ? form.getRawValue() : null;
}

export function preencherFinanciamentoForm(
  form: FinanciamentoForm,
  financiamento: DadosFinanciamentoDto | null,
): void {
  if (financiamento) {
    form.setValue(financiamento);
  }
}
