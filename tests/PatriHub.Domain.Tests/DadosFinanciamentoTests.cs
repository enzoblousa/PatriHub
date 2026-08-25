using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class DadosFinanciamentoTests
{
    [Fact]
    public void Criar_com_dados_validos_preenche_todos_os_campos()
    {
        var financiamento = DadosFinanciamento.Criar(1_500m, 200_000m, 9.5m, 120);

        Assert.Equal(1_500m, financiamento.ValorParcela);
        Assert.Equal(200_000m, financiamento.SaldoDevedor);
        Assert.Equal(9.5m, financiamento.TaxaJurosAnual);
        Assert.Equal(120, financiamento.ParcelasRestantes);
    }

    [Fact]
    public void Criar_com_saldo_devedor_negativo_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DadosFinanciamento.Criar(1_500m, -1m, 9.5m, 120));
    }

    [Fact]
    public void Criar_com_parcelas_restantes_negativas_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => DadosFinanciamento.Criar(1_500m, 200_000m, 9.5m, -1));
    }
}
