import { FormControl, FormGroup } from '@angular/forms';

import { anoFabricacaoValidator, anoMaximoCarro, anoModeloValidator } from './ativos-validadores';

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
