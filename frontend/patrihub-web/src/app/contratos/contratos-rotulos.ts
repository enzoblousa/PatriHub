import { StatusContrato } from './contratos.models';

/** Rótulo em pt-BR pro Status do Contrato — mesmo nome do enum, centralizado pra não divergir entre telas. */
export const ROTULOS_STATUS: Record<StatusContrato, string> = {
  [StatusContrato.Ativo]: 'Ativo',
  [StatusContrato.Encerrado]: 'Encerrado',
  [StatusContrato.Inadimplente]: 'Inadimplente',
};
