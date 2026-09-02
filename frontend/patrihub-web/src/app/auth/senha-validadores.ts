import type { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Espelha as regras de senha do ASP.NET Core Identity configuradas em
 * `DependencyInjection.cs` (`options.Password.*`) — só `RequiredLength = 8` é explícito lá, o
 * resto (`RequireDigit`/`RequireLowercase`/`RequireUppercase`/`RequireNonAlphanumeric`) são os
 * defaults do Identity, todos `true`. Usado hoje só pela tela de "definir nova senha" do fluxo
 * de recuperação (ver ADR-0009 e docs/adr/0008) — o cadastro (`registro.ts`) continua sem essa
 * validação de propósito (fora de escopo, ver ADR-0009).
 */
export const SENHA_MINIMO_CARACTERES = 8;

const REGEX_DIGITO = /\d/;
const REGEX_MINUSCULA = /[a-z]/;
const REGEX_MAIUSCULA = /[A-Z]/;
const REGEX_NAO_ALFANUMERICO = /[^a-zA-Z0-9]/;

/**
 * Um único validador (não um por regra) porque todas as violações de uma senha devem aparecer
 * — via `mensagemErro`, ver shared/formularios/mensagem-erro.ts — uma de cada vez conforme o
 * usuário corrige, na mesma ordem que este objeto declara as chaves. Não roda em campo vazio:
 * `required` cobre esse caso separadamente (mesmo padrão de `ativos-validadores.ts`).
 */
export const senhaForteValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const valor = control.value as string | null;
  if (!valor) {
    return null;
  }

  const erros: ValidationErrors = {};
  if (valor.length < SENHA_MINIMO_CARACTERES) {
    erros['senhaCurta'] = true;
  }
  if (!REGEX_MAIUSCULA.test(valor)) {
    erros['semMaiuscula'] = true;
  }
  if (!REGEX_MINUSCULA.test(valor)) {
    erros['semMinuscula'] = true;
  }
  if (!REGEX_DIGITO.test(valor)) {
    erros['semDigito'] = true;
  }
  if (!REGEX_NAO_ALFANUMERICO.test(valor)) {
    erros['semCaractereEspecial'] = true;
  }

  return Object.keys(erros).length > 0 ? erros : null;
};

/** Mensagens por tipo de violação da nova senha — ver docs/adr/0008. */
export const MENSAGENS_SENHA = {
  required: 'Informe a nova senha.',
  senhaCurta: `A senha precisa ter pelo menos ${SENHA_MINIMO_CARACTERES} caracteres.`,
  semMaiuscula: "A senha precisa ter pelo menos uma letra maiúscula ('A'-'Z').",
  semMinuscula: "A senha precisa ter pelo menos uma letra minúscula ('a'-'z').",
  semDigito: "A senha precisa ter pelo menos um número ('0'-'9').",
  semCaractereEspecial: 'A senha precisa ter pelo menos um caractere que não seja letra nem número.',
} as const;
