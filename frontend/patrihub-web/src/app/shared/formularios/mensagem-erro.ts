import type { AbstractControl } from '@angular/forms';

/**
 * Resolve qual mensagem mostrar pra um `AbstractControl` inválido, a partir de um mapa
 * `{ chaveDoErro: mensagem }` — ver docs/adr/0008. Extraído de `ativos/ativos-validadores.ts`
 * (onde a convenção nasceu, escopada só aos formulários de Ativo) pra ser reaproveitado também
 * pelos formulários de recuperação de senha (ver ADR-0009 e docs/adr/0008 "reaplicar a
 * convenção depois via issues separadas"). Um campo com mais de um validador mostra uma
 * mensagem por *tipo* de violação, não uma genérica só. Aceita uma mensagem fixa ou uma função
 * que recebe o valor do erro (ex: `{ minimo, maximo }`), pra mensagem poder citar esse valor sem
 * precisar duplicá-lo.
 */
export function mensagemErro(
  control: Pick<AbstractControl, 'errors'> | null | undefined,
  // `any`: o payload de erro varia por validador — cada mapa de mensagens já tipa sua própria
  // função com o payload certo.
  mensagens: Record<string, string | ((erro: any) => string)>,
): string | null {
  const erros = control?.errors;
  if (!erros) {
    return null;
  }

  for (const chave of Object.keys(mensagens)) {
    if (chave in erros) {
      const mensagem = mensagens[chave];
      return typeof mensagem === 'function' ? mensagem(erros[chave]) : mensagem;
    }
  }

  return null;
}
