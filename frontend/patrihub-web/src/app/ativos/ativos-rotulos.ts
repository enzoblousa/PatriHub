import { Motorizacao, StatusAtivo, TipoAtivo } from './ativos.models';

/**
 * Rótulos em pt-BR pra `TipoAtivo`/`StatusAtivo`, usados tanto na listagem quanto no detalhe —
 * centralizados aqui pra não divergir entre as duas telas.
 */
export const ROTULOS_TIPO: Record<TipoAtivo, string> = {
  [TipoAtivo.Imovel]: 'Imóvel',
  [TipoAtivo.Carro]: 'Carro',
};

export const ROTULOS_STATUS: Record<StatusAtivo, string> = {
  [StatusAtivo.Vago]: 'Vago',
  [StatusAtivo.Alugado]: 'Alugado',
  [StatusAtivo.Manutencao]: 'Manutenção',
  [StatusAtivo.AVenda]: 'À venda',
};

export const ROTULOS_MOTORIZACAO: Record<Motorizacao, string> = {
  [Motorizacao.Combustao]: 'Combustão',
  [Motorizacao.Eletrico]: 'Elétrico',
};

/** Unidade de leitura de `consumoMedio` — depende da Motorização (ver CONTEXT.md). */
export const UNIDADE_CONSUMO_MEDIO: Record<Motorizacao, string> = {
  [Motorizacao.Combustao]: 'km/l',
  [Motorizacao.Eletrico]: 'km/kWh',
};
