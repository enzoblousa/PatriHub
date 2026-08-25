namespace PatriHub.Domain.Entidades;

/// <summary>
/// Dados de financiamento de um Ativo, presentes apenas quando o Ativo é Financiado. Ver
/// 01-SPEC-FUNCIONAL.md §4.3/4.4.
/// </summary>
public sealed class DadosFinanciamento
{
    public decimal ValorParcela { get; private set; }
    public decimal SaldoDevedor { get; private set; }
    public decimal TaxaJurosAnual { get; private set; }
    public int ParcelasRestantes { get; private set; }

    private DadosFinanciamento()
    {
        // EF Core
    }

    private DadosFinanciamento(decimal valorParcela, decimal saldoDevedor, decimal taxaJurosAnual, int parcelasRestantes)
    {
        ValorParcela = valorParcela;
        SaldoDevedor = saldoDevedor;
        TaxaJurosAnual = taxaJurosAnual;
        ParcelasRestantes = parcelasRestantes;
    }

    public static DadosFinanciamento Criar(decimal valorParcela, decimal saldoDevedor, decimal taxaJurosAnual, int parcelasRestantes)
    {
        if (valorParcela < 0)
        {
            throw new ArgumentException("Valor da parcela não pode ser negativo.", nameof(valorParcela));
        }

        if (saldoDevedor < 0)
        {
            throw new ArgumentException("Saldo devedor não pode ser negativo.", nameof(saldoDevedor));
        }

        if (taxaJurosAnual < 0)
        {
            throw new ArgumentException("Taxa de juros anual não pode ser negativa.", nameof(taxaJurosAnual));
        }

        if (parcelasRestantes < 0)
        {
            throw new ArgumentException("Parcelas restantes não pode ser negativo.", nameof(parcelasRestantes));
        }

        return new DadosFinanciamento(valorParcela, saldoDevedor, taxaJurosAnual, parcelasRestantes);
    }
}
