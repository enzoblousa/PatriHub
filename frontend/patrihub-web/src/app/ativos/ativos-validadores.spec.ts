import { FormControl, FormGroup, Validators } from '@angular/forms';

import {
  anoFabricacaoValidator,
  anoMaximoCarro,
  anoModeloValidator,
  cepValidator,
  mensagemErro,
  placaValidator,
  ufValidator,
} from './ativos-validadores';

describe('anoFabricacaoValidator', () => {
  it('aceita anos dentro do intervalo [1900, ano atual + 1]', () => {
    const controle = new FormControl(2022);
    expect(anoFabricacaoValidator(controle)).toBeNull();
  });

  it('rejeita ano anterior a 1900', () => {
    const controle = new FormControl(1899);
    expect(anoFabricacaoValidator(controle)).toEqual({
      anoForaDoIntervalo: { minimo: 1900, maximo: anoMaximoCarro() },
    });
  });

  it('rejeita ano acima do máximo (ano atual + 1)', () => {
    const controle = new FormControl(anoMaximoCarro() + 1);
    expect(anoFabricacaoValidator(controle)).toEqual({
      anoForaDoIntervalo: { minimo: 1900, maximo: anoMaximoCarro() },
    });
  });

  it('não valida campo vazio — isso é responsabilidade do Validators.required', () => {
    const controle = new FormControl(null);
    expect(anoFabricacaoValidator(controle)).toBeNull();
  });
});

describe('anoModeloValidator', () => {
  function grupoComAnos(anoFabricacao: number | null, anoModelo: number | null) {
    return new FormGroup({
      anoFabricacao: new FormControl(anoFabricacao),
      anoModelo: new FormControl(anoModelo),
    });
  }

  it('aceita anoModelo igual ao anoFabricacao', () => {
    const grupo = grupoComAnos(2022, 2022);
    expect(anoModeloValidator(grupo.controls.anoModelo)).toBeNull();
  });

  it('aceita anoModelo maior que anoFabricacao, até o máximo', () => {
    const grupo = grupoComAnos(2022, 2023);
    expect(anoModeloValidator(grupo.controls.anoModelo)).toBeNull();
  });

  it('rejeita anoModelo menor que anoFabricacao', () => {
    const grupo = grupoComAnos(2022, 2021);
    expect(anoModeloValidator(grupo.controls.anoModelo)).toEqual({
      anoModeloInvalido: { minimo: 2022, maximo: anoMaximoCarro() },
    });
  });

  it('rejeita anoModelo acima do máximo mesmo sendo maior que anoFabricacao', () => {
    const grupo = grupoComAnos(2022, anoMaximoCarro() + 1);
    expect(anoModeloValidator(grupo.controls.anoModelo)).toEqual({
      anoModeloInvalido: { minimo: 2022, maximo: anoMaximoCarro() },
    });
  });

  it('não valida quando o próprio campo ou o irmão anoFabricacao estão vazios', () => {
    expect(anoModeloValidator(grupoComAnos(null, 2022).controls.anoModelo)).toBeNull();
    expect(anoModeloValidator(grupoComAnos(2022, null).controls.anoModelo)).toBeNull();
  });

  it('não quebra quando o controle ainda não tem parent', () => {
    const controle = new FormControl(2022);
    expect(anoModeloValidator(controle)).toBeNull();
  });
});

/** Ver docs/adr/0008 — espelha o formato validado em `Carro.cs`. */
describe('placaValidator', () => {
  it('aceita o formato antigo (AAA-0000)', () => {
    expect(placaValidator(new FormControl('ABC-1234'))).toBeNull();
  });

  it('aceita o formato Mercosul (AAA0A00)', () => {
    expect(placaValidator(new FormControl('ABC1D23'))).toBeNull();
  });

  it('aceita minúsculas, porque a validação normaliza antes de checar', () => {
    expect(placaValidator(new FormControl('abc-1234'))).toBeNull();
  });

  it('rejeita placa incompleta ou fora de qualquer um dos dois formatos', () => {
    expect(placaValidator(new FormControl('ABC123'))).toEqual({ placaFormatoInvalido: true });
    expect(placaValidator(new FormControl('AB1234'))).toEqual({ placaFormatoInvalido: true });
  });

  it('não valida campo vazio — isso é responsabilidade do Validators.required', () => {
    expect(placaValidator(new FormControl(''))).toBeNull();
  });
});

/** Ver docs/adr/0008 — espelha os 8 dígitos validados em `Endereco.cs`. */
describe('cepValidator', () => {
  it('aceita 8 dígitos, com ou sem traço', () => {
    expect(cepValidator(new FormControl('01000-000'))).toBeNull();
    expect(cepValidator(new FormControl('01000000'))).toBeNull();
  });

  it('rejeita menos ou mais de 8 dígitos', () => {
    expect(cepValidator(new FormControl('0100-000'))).toEqual({ cepFormatoInvalido: true });
    expect(cepValidator(new FormControl('010000000'))).toEqual({ cepFormatoInvalido: true });
  });

  it('não valida campo vazio — isso é responsabilidade do Validators.required', () => {
    expect(cepValidator(new FormControl(''))).toBeNull();
  });
});

/** Ver docs/adr/0008 — espelha a lista das 27 UFs validada em `Endereco.cs`. */
describe('ufValidator', () => {
  it('aceita uma UF real, inclusive minúscula', () => {
    expect(ufValidator(new FormControl('SP'))).toBeNull();
    expect(ufValidator(new FormControl('sp'))).toBeNull();
  });

  it('rejeita 2 letras que não são uma UF de verdade', () => {
    expect(ufValidator(new FormControl('ZZ'))).toEqual({ ufInvalida: true });
  });

  it('não valida tamanho errado — isso é responsabilidade de Validators.minLength/maxLength', () => {
    expect(ufValidator(new FormControl('S'))).toBeNull();
    expect(ufValidator(new FormControl('SPP'))).toBeNull();
  });

  it('não valida campo vazio — isso é responsabilidade do Validators.required', () => {
    expect(ufValidator(new FormControl(''))).toBeNull();
  });
});

describe('mensagemErro', () => {
  it('retorna null quando o controle é válido', () => {
    const controle = new FormControl('valor');
    expect(mensagemErro(controle, { required: 'Informe o campo.' })).toBeNull();
  });

  it('retorna a mensagem da primeira chave do mapa presente em control.errors', () => {
    const controle = new FormControl('', Validators.required);
    controle.updateValueAndValidity();
    expect(
      mensagemErro(controle, {
        required: 'Informe o campo.',
        pattern: 'Formato inválido.',
      }),
    ).toBe('Informe o campo.');
  });

  it('aceita uma função que recebe o valor do erro, pra mensagem citar dados dele', () => {
    const controle = new FormControl(1899, anoFabricacaoValidator);
    controle.updateValueAndValidity();
    const mensagem = mensagemErro(controle, {
      anoForaDoIntervalo: (erro: { minimo: number; maximo: number }) =>
        `Entre ${erro.minimo} e ${erro.maximo}.`,
    });
    expect(mensagem).toBe(`Entre 1900 e ${anoMaximoCarro()}.`);
  });

  it('retorna null quando nenhuma chave do mapa está em control.errors', () => {
    const controle = new FormControl('', Validators.required);
    controle.updateValueAndValidity();
    expect(mensagemErro(controle, { pattern: 'Formato inválido.' })).toBeNull();
  });
});
