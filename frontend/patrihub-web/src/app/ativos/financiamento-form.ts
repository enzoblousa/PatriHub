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
 * Mensagens por tipo de violação (ver docs/adr/0008) dos campos de Financiamento — usadas via
 * `mensagemErro()` (`ativos-validadores.ts`) nos dois formulários que compartilham este
 * fieldset.
 */
export const MENSAGENS_FINANCIAMENTO = {
  valorParcela: {
    required: 'Informe o valor da parcela.',
    min: 'O valor da parcela não pode ser negativo.',
  },
  saldoDevedor: {
    required: 'Informe o saldo devedor.',
    min: 'O saldo devedor não pode ser negativo.',
  },
  taxaJurosAnual: {
    required: 'Informe a taxa de juros anual.',
    min: 'A taxa de juros não pode ser negativa.',
  },
  parcelasRestantes: {
    required: 'Informe quantas parcelas ainda restam.',
    min: 'Parcelas restantes não pode ser negativo.',
  },
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
