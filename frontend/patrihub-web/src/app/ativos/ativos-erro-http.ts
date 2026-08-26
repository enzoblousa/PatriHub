import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extrai a mensagem de erro do backend (`{ erro: string }`, ver `ErroParaResposta` em
 * `AtivosController`), com fallback pro caso de erro de rede/500 sem corpo estruturado.
 * Reaproveitado pelas telas de formulário e de detalhe de Ativo.
 */
export function mensagemErroAtivo(erro: unknown, padrao: string): string {
  return erro instanceof HttpErrorResponse && typeof erro.error?.erro === 'string'
    ? erro.error.erro
    : padrao;
}
