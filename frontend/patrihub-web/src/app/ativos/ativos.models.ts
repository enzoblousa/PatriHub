// Espelha os DTOs de PatriHub.Application.Ativos (AtivoDtos.cs). O System.Text.Json do backend
// serializa propriedades em camelCase e enums como número (posição no enum C#, sem
// JsonStringEnumConverter) — os tipos aqui replicam as duas convenções.

export enum TipoAtivo {
  Imovel = 0,
  Carro = 1,
}

/**
 * Só `Manutencao`/`AVenda` podem ser setados manualmente (ver `MarcarStatusAtivoRequest`) —
 * `Vago`/`Alugado` são derivados do ciclo de vida do Contrato (issue #5) e o backend rejeita
 * tentativa manual de setá-los.
 */
export enum StatusAtivo {
  Vago = 0,
  Alugado = 1,
  Manutencao = 2,
  AVenda = 3,
}

export enum TipoImovel {
  Apartamento = 0,
  Casa = 1,
  Comercial = 2,
  Terreno = 3,
}

export interface EnderecoDto {
  rua: string;
  numero: string;
  complemento: string | null;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
}

export interface DadosFinanciamentoDto {
  valorParcela: number;
  saldoDevedor: number;
  taxaJurosAnual: number;
  parcelasRestantes: number;
}

/** Corpo de cadastro (POST) e edição (PUT) de um Imóvel — os mesmos campos são exigidos nos dois casos. */
export interface ImovelRequest {
  apelido: string;
  dataAquisicao: string;
  valorAquisicao: number;
  valorMercadoAtual: number;
  endereco: EnderecoDto;
  tipoImovel: TipoImovel;
  areaM2: number;
  matricula: string;
  valorIptuMensal: number;
  valorCondominioMensal: number;
  financiamento: DadosFinanciamentoDto | null;
}

/** Corpo de cadastro (POST) e edição (PUT) de um Carro — os mesmos campos são exigidos nos dois casos. */
export interface CarroRequest {
  apelido: string;
  dataAquisicao: string;
  valorAquisicao: number;
  valorMercadoAtual: number;
  placa: string;
  marca: string;
  modelo: string;
  anoFabricacao: number;
  anoModelo: number;
  valorFipeAtual: number;
  km: number;
  consumoMedio: number;
  financiamento: DadosFinanciamentoDto | null;
}

export interface MarcarStatusAtivoRequest {
  status: StatusAtivo;
}

/** Visão resumida para a listagem de Ativos. */
export interface AtivoResumoDto {
  id: string;
  apelido: string;
  tipo: TipoAtivo;
  status: StatusAtivo;
  valorMercadoAtual: number;
  lucroDoMes: number;
}

export interface ImovelDetalheDto {
  endereco: EnderecoDto;
  tipoImovel: TipoImovel;
  areaM2: number;
  matricula: string;
  valorIptuMensal: number;
  valorCondominioMensal: number;
}

export interface CarroDetalheDto {
  placa: string;
  marca: string;
  modelo: string;
  anoFabricacao: number;
  anoModelo: number;
  valorFipeAtual: number;
  km: number;
  consumoMedio: number;
}

/**
 * Detalhe completo de um Ativo. Exatamente um entre `imovel` e `carro` vem preenchido,
 * conforme `tipo`.
 */
export interface AtivoDetalheDto {
  id: string;
  apelido: string;
  tipo: TipoAtivo;
  status: StatusAtivo;
  dataAquisicao: string;
  valorAquisicao: number;
  valorMercadoAtual: number;
  financiado: boolean;
  financiamento: DadosFinanciamentoDto | null;
  criadoEm: string;
  atualizadoEm: string;
  imovel: ImovelDetalheDto | null;
  carro: CarroDetalheDto | null;
}
