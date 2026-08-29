// Espelha os DTOs de PatriHub.Application.Dashboard (DashboardDtos.cs) — camelCase, mesma
// convenção documentada em `../ativos/ativos.models.ts`. Sem enum aqui: todo valor é derivado
// (calculado on-the-fly, nunca persistido — ver 01-SPEC-FUNCIONAL.md §5/§6.4).

/** Métricas derivadas de um único Ativo — Yield e ROI vêm como fração (0.12 = 12%), não percentual. */
export interface MetricasAtivoDto {
  ativoId: string;
  apelido: string;
  lucroDoMes: number;
  lucroAcumulado: number;
  yield: number;
  roiSobreValorAquisicao: number;
  roiSobreValorMercadoAtual: number;
  /** ValorAquisição − ValorMercadoAtual: negativo significa valorização, não depreciação. */
  depreciacao: number;
  /** null quando `taxaReferenciaAnual` não foi informada na consulta. */
  custoDeOportunidade: number | null;
  projecaoDeLucro: number;
  /** Terceira leitura de ROI, sobre ValorFipeAtual — null quando o Ativo não é Carro (issue #46). */
  roiSobreValorFipeAtual: number | null;
  /** Divergência entre ValorMercadoAtual e ValorFipeAtual — null quando o Ativo não é Carro. */
  divergenciaFipeAtual: number | null;
  /** Se divergenciaFipeAtual ultrapassa o limiar de alerta — null quando o Ativo não é Carro. */
  alertaDivergenciaFipe: boolean | null;
}

/** Visão consolidada do patrimônio do usuário — soma de todos os Ativos, com a lista por Ativo pra comparação. */
export interface PatrimonioConsolidadoDto {
  lucroTotalDoMes: number;
  lucroTotalAcumulado: number;
  ativos: MetricasAtivoDto[];
}
