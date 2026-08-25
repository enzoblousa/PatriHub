using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class LancamentoTests
{
    private static Lancamento LancamentoValido(
        TipoLancamento tipo = TipoLancamento.Receita,
        CategoriaLancamento categoria = CategoriaLancamento.Aluguel,
        Guid? usuarioId = null,
        Guid? ativoId = null) =>
        Lancamento.Registrar(
            usuarioId ?? Guid.NewGuid(),
            ativoId ?? Guid.NewGuid(),
            tipo,
            categoria,
            valor: 1_500m,
            data: new DateOnly(2026, 3, 10),
            descricao: "Aluguel de março");

    [Fact]
    public void Registrar_com_dados_validos_cria_lancamento()
    {
        var usuarioId = Guid.NewGuid();
        var ativoId = Guid.NewGuid();

        var lancamento = Lancamento.Registrar(
            usuarioId, ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel,
            1_500m, new DateOnly(2026, 3, 10), "Aluguel de março");

        Assert.NotEqual(Guid.Empty, lancamento.Id);
        Assert.Equal(usuarioId, lancamento.UsuarioId);
        Assert.Equal(ativoId, lancamento.AtivoId);
        Assert.Equal(TipoLancamento.Receita, lancamento.Tipo);
        Assert.Equal(CategoriaLancamento.Aluguel, lancamento.Categoria);
        Assert.Equal(1_500m, lancamento.Valor);
        Assert.Equal("Aluguel de março", lancamento.Descricao);
        Assert.Null(lancamento.ContratoId);
    }

    [Fact]
    public void Registrar_com_ContratoId_vincula_o_lancamento_ao_contrato()
    {
        var contratoId = Guid.NewGuid();

        var lancamento = Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, CategoriaLancamento.Aluguel,
            1_500m, new DateOnly(2026, 3, 10), "Aluguel", contratoId);

        Assert.Equal(contratoId, lancamento.ContratoId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Registrar_com_valor_zero_ou_negativo_lanca_ArgumentException(decimal valorInvalido)
    {
        Assert.Throws<ArgumentException>(() => Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, CategoriaLancamento.Aluguel,
            valorInvalido, new DateOnly(2026, 3, 10), "Aluguel"));
    }

    [Fact]
    public void Registrar_com_descricao_vazia_normaliza_para_null()
    {
        var lancamento = Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Despesa, CategoriaLancamento.Iptu,
            300m, new DateOnly(2026, 3, 10), "   ");

        Assert.Null(lancamento.Descricao);
    }

    [Theory]
    [InlineData(CategoriaLancamento.Iptu)]
    [InlineData(CategoriaLancamento.Condominio)]
    [InlineData(CategoriaLancamento.Manutencao)]
    public void Registrar_Receita_com_categoria_de_despesa_lanca_ArgumentException(CategoriaLancamento categoriaDeDespesa)
    {
        Assert.Throws<ArgumentException>(() => Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, categoriaDeDespesa,
            1_500m, new DateOnly(2026, 3, 10), null));
    }

    [Theory]
    [InlineData(CategoriaLancamento.Aluguel)]
    [InlineData(CategoriaLancamento.TaxaDeServico)]
    [InlineData(CategoriaLancamento.MultaPorAtraso)]
    public void Registrar_Despesa_com_categoria_de_receita_lanca_ArgumentException(CategoriaLancamento categoriaDeReceita)
    {
        Assert.Throws<ArgumentException>(() => Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Despesa, categoriaDeReceita,
            300m, new DateOnly(2026, 3, 10), null));
    }

    [Theory]
    [InlineData(TipoLancamento.Receita)]
    [InlineData(TipoLancamento.Despesa)]
    public void Registrar_com_categoria_Outras_e_valido_para_qualquer_tipo(TipoLancamento tipo)
    {
        var lancamento = Lancamento.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), tipo, CategoriaLancamento.Outras,
            100m, new DateOnly(2026, 3, 10), null);

        Assert.Equal(CategoriaLancamento.Outras, lancamento.Categoria);
    }

    [Fact]
    public void Atualizar_troca_todos_os_campos_editaveis()
    {
        var lancamento = LancamentoValido();
        var contratoId = Guid.NewGuid();

        lancamento.Atualizar(
            TipoLancamento.Despesa, CategoriaLancamento.Manutencao, 800m,
            new DateOnly(2026, 4, 1), "Reparo hidráulico", contratoId);

        Assert.Equal(TipoLancamento.Despesa, lancamento.Tipo);
        Assert.Equal(CategoriaLancamento.Manutencao, lancamento.Categoria);
        Assert.Equal(800m, lancamento.Valor);
        Assert.Equal(new DateOnly(2026, 4, 1), lancamento.Data);
        Assert.Equal("Reparo hidráulico", lancamento.Descricao);
        Assert.Equal(contratoId, lancamento.ContratoId);
    }

    [Fact]
    public void Atualizar_com_valor_invalido_lanca_ArgumentException_e_nao_altera_estado()
    {
        var lancamento = LancamentoValido();

        Assert.Throws<ArgumentException>(() => lancamento.Atualizar(
            TipoLancamento.Receita, CategoriaLancamento.Aluguel, 0m,
            new DateOnly(2026, 4, 1), "Aluguel", null));

        Assert.Equal(1_500m, lancamento.Valor);
    }

    [Fact]
    public void CategoriasPermitidas_de_Receita_inclui_Outras_mas_nao_categorias_de_despesa()
    {
        var categorias = Lancamento.CategoriasPermitidas(TipoLancamento.Receita);

        Assert.Contains(CategoriaLancamento.Aluguel, categorias);
        Assert.Contains(CategoriaLancamento.Outras, categorias);
        Assert.DoesNotContain(CategoriaLancamento.Iptu, categorias);
    }

    [Fact]
    public void CategoriasPermitidas_de_Despesa_inclui_Outras_mas_nao_categorias_de_receita()
    {
        var categorias = Lancamento.CategoriasPermitidas(TipoLancamento.Despesa);

        Assert.Contains(CategoriaLancamento.Iptu, categorias);
        Assert.Contains(CategoriaLancamento.Outras, categorias);
        Assert.DoesNotContain(CategoriaLancamento.Aluguel, categorias);
    }
}
