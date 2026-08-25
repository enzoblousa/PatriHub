using PatriHub.Domain.Calculos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class CalculadoraFinanceiraTests
{
    private static Lancamento Receita(decimal valor, DateOnly data) =>
        Lancamento.Registrar(Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Receita, CategoriaLancamento.Aluguel, valor, data, null);

    private static Lancamento Despesa(decimal valor, DateOnly data) =>
        Lancamento.Registrar(Guid.NewGuid(), Guid.NewGuid(), TipoLancamento.Despesa, CategoriaLancamento.Iptu, valor, data, null);

    [Fact]
    public void LucroDoPeriodo_soma_receitas_e_subtrai_despesas_dentro_do_intervalo()
    {
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2026, 3, 5)),
            Despesa(300m, new DateOnly(2026, 3, 10)),
            Despesa(150m, new DateOnly(2026, 3, 20)),
        };

        var lucro = CalculadoraFinanceira.LucroDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(1_050m, lucro);
    }

    [Fact]
    public void LucroDoPeriodo_ignora_lancamentos_fora_do_intervalo()
    {
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2026, 2, 28)),
            Receita(1_500m, new DateOnly(2026, 4, 1)),
        };

        var lucro = CalculadoraFinanceira.LucroDoPeriodo(lancamentos, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(0m, lucro);
    }

    [Fact]
    public void LucroDoPeriodo_sem_lancamentos_retorna_zero()
    {
        var lucro = CalculadoraFinanceira.LucroDoPeriodo([], new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Equal(0m, lucro);
    }

    [Fact]
    public void LucroAcumulado_soma_todos_os_lancamentos_desde_a_aquisicao()
    {
        var dataAquisicao = new DateOnly(2024, 1, 1);
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2024, 2, 1)),
            Receita(1_500m, new DateOnly(2026, 3, 1)),
            Despesa(500m, new DateOnly(2025, 6, 1)),
        };

        var lucroAcumulado = CalculadoraFinanceira.LucroAcumulado(lancamentos, dataAquisicao);

        Assert.Equal(2_500m, lucroAcumulado);
    }

    [Fact]
    public void LucroAcumulado_ignora_lancamentos_anteriores_a_aquisicao()
    {
        var dataAquisicao = new DateOnly(2026, 1, 1);
        var lancamentos = new[]
        {
            Receita(1_500m, new DateOnly(2025, 12, 31)),
        };

        var lucroAcumulado = CalculadoraFinanceira.LucroAcumulado(lancamentos, dataAquisicao);

        Assert.Equal(0m, lucroAcumulado);
    }

    [Theory]
    [InlineData(300_000, 350_000, -50_000)]
    [InlineData(300_000, 250_000, 50_000)]
    [InlineData(300_000, 300_000, 0)]
    public void Depreciacao_e_ValorAquisicao_menos_ValorMercadoAtual(decimal valorAquisicao, decimal valorMercadoAtual, decimal esperado)
    {
        var depreciacao = CalculadoraFinanceira.Depreciacao(valorAquisicao, valorMercadoAtual);

        Assert.Equal(esperado, depreciacao);
    }
}
