import type { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Validadores de `AtivoFormCarro` que hoje só existem no backend (`Carro.AtualizarDadosDoCarro`)
 * — issue #72 pede que o frontend reflita a mesma regra em vez de deixar o usuário descobrir só
 * no erro 400 da API. `anoMaximoCarro()` é calculado a cada chamada, não congelado no import,
 * porque o ano corrente pode virar numa sessão de longa duração.
 */
export const ANO_MINIMO_CARRO = 1900;

/**
 * Usa `getUTCFullYear()`, não `getFullYear()` (hora local) — o backend calcula o teto com
 * `DateTime.UtcNow.Year` (`Carro.cs`), e perto da virada do ano os dois divergem em fusos à
 * frente de UTC se um usar hora local e o outro UTC.
 */
export function anoMaximoCarro(): number {
  return new Date().getUTCFullYear() + 1;
}

function vazio(valor: unknown): boolean {
  return valor === null || valor === undefined || valor === ('' as unknown);
}

/**
 * Resolve qual mensagem mostrar pra um `AbstractControl` inválido, a partir de um mapa
 * `{ chaveDoErro: mensagem }` — ver docs/adr/0008. Um campo com mais de um validador (ex:
 * `required` + formato) mostra uma mensagem por *tipo* de violação, não uma genérica só; a
 * ordem das chaves no mapa é a ordem de prioridade quando, em teoria, mais de um erro
 * estivesse presente ao mesmo tempo (na prática isso quase não acontece, porque os
 * validadores de formato — nativos ou os `*Validator` deste arquivo — não rodam em campo
 * vazio, só `required` roda). Aceita uma mensagem fixa ou uma função que recebe o valor do
 * erro (ex: `{ minimo, maximo }` de `anoFabricacaoValidator`), pra mensagem poder citar esse
 * valor sem precisar duplicá-lo.
 */
export function mensagemErro(
  control: Pick<AbstractControl, 'errors'> | null | undefined,
  // `any`: o payload de erro varia por validador (`ErroDeIntervaloDeAno`, `boolean`, etc.) — cada
  // mapa de mensagens (`MENSAGENS_CARRO` etc.) já tipa sua própria função com o payload certo.
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

/** Formato antigo (`AAA-0000`) e Mercosul (`AAA0A00`) — espelha `Carro.cs` (ver docs/adr/0008). */
const PLACA_FORMATO_ANTIGO = /^[A-Z]{3}-\d{4}$/;
const PLACA_FORMATO_MERCOSUL = /^[A-Z]{3}\d[A-Z]\d{2}$/;

/** Espelha a validação de formato de `Carro.AtualizarDadosDoCarro` (`Carro.cs`). */
export const placaValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const valor = control.value as string | null;
  if (vazio(valor)) {
    return null;
  }

  const normalizada = valor!.trim().toUpperCase();
  return PLACA_FORMATO_ANTIGO.test(normalizada) || PLACA_FORMATO_MERCOSUL.test(normalizada)
    ? null
    : { placaFormatoInvalido: true };
};

/** Espelha a validação de 8 dígitos de `Endereco.Criar` (`Endereco.cs`, ver docs/adr/0008). */
export const cepValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const valor = control.value as string | null;
  if (vazio(valor)) {
    return null;
  }

  const digitos = valor!.replace(/\D/g, '');
  return digitos.length === 8 ? null : { cepFormatoInvalido: true };
};

/** As 27 UFs brasileiras — espelha `Endereco.cs` (ver docs/adr/0008). */
const UFS_VALIDAS = new Set([
  'AC',
  'AL',
  'AP',
  'AM',
  'BA',
  'CE',
  'DF',
  'ES',
  'GO',
  'MA',
  'MT',
  'MS',
  'MG',
  'PA',
  'PB',
  'PR',
  'PE',
  'PI',
  'RJ',
  'RN',
  'RS',
  'RO',
  'RR',
  'SC',
  'SP',
  'SE',
  'TO',
]);

/**
 * Espelha a lista de UFs de `Endereco.cs` — antes só se checava 2 caracteres
 * (`Validators.minLength`/`maxLength`, que continuam pra dar a mensagem de "tamanho errado"
 * separada da de "não é uma UF de verdade").
 */
export const ufValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const valor = control.value as string | null;
  if (vazio(valor) || valor!.trim().length !== 2) {
    return null;
  }

  return UFS_VALIDAS.has(valor!.trim().toUpperCase()) ? null : { ufInvalida: true };
};

type ErroDeIntervaloDeAno = { minimo: number; maximo: number };

/**
 * Mensagens por tipo de violação (ver docs/adr/0008) dos campos comuns a Imóvel e Carro —
 * usadas via `mensagemErro()` em `AtivoFormImovel`/`AtivoFormCarro`. Campos com um único
 * validador (`apelido`, `dataAquisicao`) continuam com a mensagem fixa direto no template, sem
 * precisar de mapa.
 */
export const MENSAGENS_ATIVO_COMUM = {
  valorAquisicao: {
    required: 'Informe um valor de aquisição.',
    min: 'O valor de aquisição não pode ser negativo.',
  },
  valorMercadoAtual: {
    required: 'Informe um valor de mercado atual.',
    min: 'O valor de mercado não pode ser negativo.',
  },
} as const;

/** Mensagens por tipo de violação dos campos exclusivos de Carro — ver docs/adr/0008. */
export const MENSAGENS_CARRO = {
  placa: {
    required: 'Informe a placa.',
    placaFormatoInvalido: 'Placa em formato inválido (ex: AAA-0000 ou AAA0A00).',
  },
  anoFabricacao: {
    required: 'Informe o ano de fabricação.',
    anoForaDoIntervalo: (erro: ErroDeIntervaloDeAno) =>
      `O ano de fabricação deve estar entre ${erro.minimo} e ${erro.maximo}.`,
  },
  anoModelo: {
    required: 'Informe o ano do modelo.',
    anoModeloInvalido: (erro: ErroDeIntervaloDeAno) =>
      `O ano do modelo deve ser maior ou igual ao ano de fabricação (${erro.minimo}) e no máximo ${erro.maximo}.`,
  },
  valorFipeAtual: {
    required: 'Informe um valor FIPE.',
    min: 'O valor FIPE não pode ser negativo.',
  },
  km: {
    required: 'Informe a quilometragem.',
    min: 'A quilometragem não pode ser negativa.',
  },
  consumoMedio: {
    required: 'Informe o consumo médio.',
    min: 'O consumo médio não pode ser negativo.',
  },
} as const;

/** Mensagens por tipo de violação dos campos exclusivos de Imóvel — ver docs/adr/0008. */
export const MENSAGENS_IMOVEL = {
  areaM2: {
    required: 'Informe a área.',
    min: 'A área deve ser maior que zero.',
  },
  valorIptuMensal: {
    required: 'Informe o IPTU mensal.',
    min: 'O IPTU não pode ser negativo.',
  },
  valorCondominioMensal: {
    required: 'Informe o condomínio mensal.',
    min: 'O condomínio não pode ser negativo.',
  },
  uf: {
    required: 'Informe a UF.',
    minlength: 'Informe a UF com as 2 letras (ex: SP).',
    maxlength: 'Informe a UF com as 2 letras (ex: SP).',
    ufInvalida: 'UF inválida.',
  },
  cep: {
    required: 'Informe o CEP.',
    cepFormatoInvalido: 'CEP deve conter 8 dígitos.',
  },
} as const;

/** Espelha `anoFabricacao < anoMinimo || anoFabricacao > anoMaximo` do backend. */
export const anoFabricacaoValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const valor = control.value as number;
  if (vazio(valor)) {
    return null;
  }

  const maximo = anoMaximoCarro();
  return valor < ANO_MINIMO_CARRO || valor > maximo
    ? { anoForaDoIntervalo: { minimo: ANO_MINIMO_CARRO, maximo } }
    : null;
};

/**
 * Espelha `anoModelo < anoFabricacao || anoModelo > anoMaximo` do backend — validação cruzada
 * que lê o irmão `anoFabricacao` via `control.parent` (só disponível depois que o `FormGroup` é
 * montado, mas antes da primeira `updateValueAndValidity`, que é quando os validadores rodam
 * pela 1ª vez). O componente precisa reacionar essa validação manualmente quando
 * `anoFabricacao` mudar (ver `AtivoFormCarro`), porque Angular só reroda o validador de um
 * controle quando o valor dele mesmo muda.
 */
export const anoModeloValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const valor = control.value as number;
  const anoFabricacao = control.parent?.get('anoFabricacao')?.value as number | undefined;
  if (vazio(valor) || vazio(anoFabricacao)) {
    return null;
  }

  const maximo = anoMaximoCarro();
  return valor < anoFabricacao! || valor > maximo
    ? { anoModeloInvalido: { minimo: anoFabricacao, maximo } }
    : null;
};
