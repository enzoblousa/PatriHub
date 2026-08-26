import { categoriasPermitidas } from './lancamentos-categorias';
import { CategoriaLancamento, TipoLancamento } from './lancamentos.models';

describe('categoriasPermitidas', () => {
  it('Receita permite Aluguel, TaxaDeServico, MultaPorAtraso e Outras', () => {
    expect(categoriasPermitidas(TipoLancamento.Receita)).toEqual([
      CategoriaLancamento.Aluguel,
      CategoriaLancamento.TaxaDeServico,
      CategoriaLancamento.MultaPorAtraso,
      CategoriaLancamento.Outras,
    ]);
  });

  it('Despesa não permite categorias exclusivas de Receita', () => {
    const categorias = categoriasPermitidas(TipoLancamento.Despesa);

    expect(categorias).not.toContain(CategoriaLancamento.Aluguel);
    expect(categorias).not.toContain(CategoriaLancamento.TaxaDeServico);
    expect(categorias).not.toContain(CategoriaLancamento.MultaPorAtraso);
  });

  it('Outras é permitida nos dois tipos', () => {
    expect(categoriasPermitidas(TipoLancamento.Receita)).toContain(CategoriaLancamento.Outras);
    expect(categoriasPermitidas(TipoLancamento.Despesa)).toContain(CategoriaLancamento.Outras);
  });
});
