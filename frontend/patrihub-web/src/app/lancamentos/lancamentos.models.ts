// Espelha os DTOs de PatriHub.Application.Lancamentos (LancamentoDtos.cs) e o enum
// `CategoriaLancamento`/`TipoLancamento` de PatriHub.Domain.Entidades.Lancamento — mesma
// convenção de camelCase + enum numérico documentada em `../ativos/ativos.models.ts`.

export enum TipoLancamento {
  Receita = 0,
  Despesa = 1,
}

/**
 * Ordem espelha exatamente a declaração do enum C# (`Lancamento.cs`) — a posição é o valor
 * serializado, então a ordem aqui não pode ser reordenada sem quebrar em sincronia com o
 * backend. Ver `categoriasPermitidas` em `lancamentos-categorias.ts` pra saber quais valores
 * são válidos por `TipoLancamento`.
 */
export enum CategoriaLancamento {
  Aluguel = 0,
  TaxaDeServico = 1,
  MultaPorAtraso = 2,
  Iptu = 3,
  Condominio = 4,
  Manutencao = 5,
  Reforma = 6,
  Seguro = 7,
  Ipva = 8,
  Multa = 9,
  Financiamento = 10,
  Administracao = 11,
  ImpostoDeRenda = 12,
  Abastecimento = 13,
  Outras = 14,
}

/** Corpo de criação (POST) e edição (PUT) de um Lançamento — os mesmos campos são exigidos nos dois casos. */
export interface LancamentoRequest {
  ativoId: string;
  tipo: TipoLancamento;
  categoria: CategoriaLancamento;
  valor: number;
  data: string;
  descricao: string | null;
  contratoId: string | null;
}

export interface LancamentoDto {
  id: string;
  ativoId: string;
  contratoId: string | null;
  tipo: TipoLancamento;
  categoria: CategoriaLancamento;
  valor: number;
  data: string;
  descricao: string | null;
  criadoEm: string;
  atualizadoEm: string;
}

/** Filtros da listagem (todos opcionais) — vira query string em `GET /api/lancamentos`. */
export interface LancamentoFiltro {
  ativoId?: string;
  dataInicio?: string;
  dataFim?: string;
  tipo?: TipoLancamento;
}
