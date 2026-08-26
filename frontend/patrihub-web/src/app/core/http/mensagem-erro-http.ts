import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extrai a mensagem de erro do backend (`{ erro: string }`, ver `ErroParaResposta` nos
 * controllers de `PatriHub.Api`), com fallback pro caso de erro de rede/500 sem corpo
 * estruturado. Compartilhado por toda feature que chama a API (`ativos/`, `lancamentos/`),
 * pra não duplicar esse parse tela a tela.
 */
export function mensagemErroHttp(erro: unknown, padrao: string): string {
  return erro instanceof HttpErrorResponse && typeof erro.error?.erro === 'string'
    ? erro.error.erro
    : padrao;
}
