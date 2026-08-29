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

/**
 * Textos explicativos (ver `shared/ajuda`) de campos de Carro/Imóvel usados tanto no
 * formulário quanto no detalhe — centralizados aqui pra não divergir entre as duas telas,
 * mesmo racional dos `ROTULOS_*` acima.
 */
export const TEXTOS_AJUDA_CARRO = {
  motorizacao:
    'Se o carro é 100% elétrico (só recarga) ou a combustão — determina a unidade do Consumo médio (km/l ou km/kWh).',
  consumoMedio:
    'Quantos km o carro roda por litro de combustível (ou por kWh de bateria, se for Elétrico).',
} as const;

export const TEXTOS_AJUDA_IMOVEL = {
  matricula:
    'Número de registro do imóvel no Cartório de Registro de Imóveis — identifica oficialmente o imóvel, não é o número do IPTU nem do endereço.',
  areaM2: 'Área construída ou útil do imóvel, em metros quadrados.',
  iptuMensal:
    'Valor mensal do Imposto Predial e Territorial Urbano — o imposto municipal cobrado sobre o imóvel.',
} as const;
