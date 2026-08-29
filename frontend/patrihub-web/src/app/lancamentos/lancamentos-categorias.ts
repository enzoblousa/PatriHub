import { CategoriaLancamento, TipoLancamento } from './lancamentos.models';

/** Espelha `Lancamento.CategoriasReceita` (`Lancamento.cs`). */
const CATEGORIAS_RECEITA: readonly CategoriaLancamento[] = [
  CategoriaLancamento.Aluguel,
  CategoriaLancamento.TaxaDeServico,
  CategoriaLancamento.MultaPorAtraso,
  CategoriaLancamento.Outras,
];

/** Espelha `Lancamento.CategoriasDespesa` (`Lancamento.cs`). */
const CATEGORIAS_DESPESA: readonly CategoriaLancamento[] = [
  CategoriaLancamento.Iptu,
  CategoriaLancamento.Condominio,
  CategoriaLancamento.Manutencao,
  CategoriaLancamento.Reforma,
  CategoriaLancamento.Seguro,
  CategoriaLancamento.Ipva,
  CategoriaLancamento.Multa,
  CategoriaLancamento.Financiamento,
  CategoriaLancamento.Administracao,
  CategoriaLancamento.ImpostoDeRenda,
  CategoriaLancamento.Abastecimento,
  CategoriaLancamento.Outras,
];

/**
 * Lista fixa de categorias válidas por Tipo — espelha `Lancamento.CategoriasPermitidas`
 * (backend rejeita com 400 fora dessa lista). O formulário usa isso pra só oferecer opções
 * válidas (AC "Categoria disponível no formulário respeita a lista fixa por Tipo").
 */
export function categoriasPermitidas(tipo: TipoLancamento): readonly CategoriaLancamento[] {
  return tipo === TipoLancamento.Receita ? CATEGORIAS_RECEITA : CATEGORIAS_DESPESA;
}

export const ROTULOS_TIPO: Record<TipoLancamento, string> = {
  [TipoLancamento.Receita]: 'Receita',
  [TipoLancamento.Despesa]: 'Despesa',
};

export const ROTULOS_CATEGORIA: Record<CategoriaLancamento, string> = {
  [CategoriaLancamento.Aluguel]: 'Aluguel',
  [CategoriaLancamento.TaxaDeServico]: 'Taxa de serviço',
  [CategoriaLancamento.MultaPorAtraso]: 'Multa por atraso',
  [CategoriaLancamento.Iptu]: 'IPTU',
  [CategoriaLancamento.Condominio]: 'Condomínio',
  [CategoriaLancamento.Manutencao]: 'Manutenção',
  [CategoriaLancamento.Reforma]: 'Reforma',
  [CategoriaLancamento.Seguro]: 'Seguro',
  [CategoriaLancamento.Ipva]: 'IPVA',
  [CategoriaLancamento.Multa]: 'Multa',
  [CategoriaLancamento.Financiamento]: 'Financiamento',
  [CategoriaLancamento.Administracao]: 'Administração',
  [CategoriaLancamento.ImpostoDeRenda]: 'Imposto de renda',
  [CategoriaLancamento.Abastecimento]: 'Abastecimento',
  [CategoriaLancamento.Outras]: 'Outras',
};
