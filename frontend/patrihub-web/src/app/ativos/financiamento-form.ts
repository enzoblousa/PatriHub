import { FormBuilder, Validators } from '@angular/forms';

import type { DadosFinanciamentoDto } from './ativos.models';

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
