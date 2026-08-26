// Espelha os DTOs de PatriHub.Application.Contratos (ContratoDtos.cs) e o enum
// `StatusContrato` de PatriHub.Domain.Entidades.Contrato — mesma convenção de camelCase +
// enum numérico documentada em `../ativos/ativos.models.ts`.

/**
 * `Inadimplente` é atribuído automaticamente pelo job periódico (issue #6) — nunca setado
 * manualmente pelo frontend. Ordem espelha exatamente o enum C# (`Contrato.cs`).
 */
export enum StatusContrato {
  Ativo = 0,
  Encerrado = 1,
  Inadimplente = 2,
}

/** Corpo de criação (POST) de um Contrato — não há edição no MVP, apenas criação e encerramento. */
export interface ContratoRequest {
  ativoId: string;
  locatarioId: string;
  valorAluguelMensal: number;
  diaVencimento: number;
  dataInicio: string;
  dataFim: string | null;
}

export interface ContratoDto {
  id: string;
  ativoId: string;
  locatarioId: string;
  valorAluguelMensal: number;
  diaVencimento: number;
  dataInicio: string;
  dataFim: string | null;
  status: StatusContrato;
  criadoEm: string;
  atualizadoEm: string;
}
