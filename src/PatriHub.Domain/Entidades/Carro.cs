namespace PatriHub.Domain.Entidades;

/// <summary>
/// Só BEV (100% elétrico) — híbrido (HEV/PHEV) fica classificado como <see cref="Combustao"/>
/// até que o suporte a consumo misto seja necessário. Ver CONTEXT.md ("Motorização") e
/// docs/adr/0006-motorizacao-eletrica-em-carro.md.
/// </summary>
public enum Motorizacao
{
    Combustao,
    Eletrico
}

/// <summary>Especialização de Ativo. Ver 01-SPEC-FUNCIONAL.md §4.4.</summary>
public sealed class Carro : Ativo
{
    public override TipoAtivo Tipo => TipoAtivo.Carro;

    public string Placa { get; private set; } = string.Empty;
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int AnoFabricacao { get; private set; }
    public int AnoModelo { get; private set; }
    public decimal ValorFipeAtual { get; private set; }
    public decimal Km { get; private set; }
    public Motorizacao Motorizacao { get; private set; }

    /// <summary>
    /// "Quanto o carro anda por unidade de energia" — a unidade de leitura depende de
    /// <see cref="Motorizacao"/> (km/l pra Combustão, km/kWh pra Elétrico); ver CONTEXT.md.
    /// </summary>
    public decimal ConsumoMedio { get; private set; }

    private Carro()
    {
        // EF Core
    }

    private Carro(
        Guid usuarioId,
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        DadosFinanciamento? financiamento,
        DateTimeOffset agora)
        : base(usuarioId, apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, agora)
    {
    }

    public static Carro Cadastrar(
        Guid usuarioId,
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        string placa,
        string marca,
        string modelo,
        int anoFabricacao,
        int anoModelo,
        decimal valorFipeAtual,
        decimal km,
        Motorizacao motorizacao,
        decimal consumoMedio,
        DadosFinanciamento? financiamento = null,
        DateTimeOffset? agora = null)
    {
        var momento = agora ?? DateTimeOffset.UtcNow;
        var carro = new Carro(usuarioId, apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, momento);
        carro.AtualizarDadosDoCarro(placa, marca, modelo, anoFabricacao, anoModelo, valorFipeAtual, km, motorizacao, consumoMedio);
        return carro;
    }

    /// <summary>Edita os dados do Carro — o usuário pode reeditar qualquer campo, inclusive ValorMercadoAtual.</summary>
    public void Atualizar(
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        string placa,
        string marca,
        string modelo,
        int anoFabricacao,
        int anoModelo,
        decimal valorFipeAtual,
        decimal km,
        Motorizacao motorizacao,
        decimal consumoMedio,
        DadosFinanciamento? financiamento,
        DateTimeOffset? agora = null)
    {
        AtualizarDadosComuns(apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, agora);
        AtualizarDadosDoCarro(placa, marca, modelo, anoFabricacao, anoModelo, valorFipeAtual, km, motorizacao, consumoMedio);
    }

    private void AtualizarDadosDoCarro(
        string placa,
        string marca,
        string modelo,
        int anoFabricacao,
        int anoModelo,
        decimal valorFipeAtual,
        decimal km,
        Motorizacao motorizacao,
        decimal consumoMedio)
    {
        if (string.IsNullOrWhiteSpace(placa))
        {
            throw new ArgumentException("Placa não pode ser vazia.", nameof(placa));
        }

        if (string.IsNullOrWhiteSpace(marca))
        {
            throw new ArgumentException("Marca não pode ser vazia.", nameof(marca));
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException("Modelo não pode ser vazio.", nameof(modelo));
        }

        const int anoMinimo = 1900;
        var anoMaximo = DateTime.UtcNow.Year + 1;

        if (anoFabricacao < anoMinimo || anoFabricacao > anoMaximo)
        {
            throw new ArgumentException($"Ano de fabricação deve estar entre {anoMinimo} e {anoMaximo}.", nameof(anoFabricacao));
        }

        if (anoModelo < anoFabricacao || anoModelo > anoMaximo)
        {
            throw new ArgumentException("Ano do modelo deve ser maior ou igual ao ano de fabricação.", nameof(anoModelo));
        }

        if (valorFipeAtual < 0)
        {
            throw new ArgumentException("Valor FIPE não pode ser negativo.", nameof(valorFipeAtual));
        }

        if (km < 0)
        {
            throw new ArgumentException("Km não pode ser negativo.", nameof(km));
        }

        if (consumoMedio < 0)
        {
            throw new ArgumentException("Consumo médio não pode ser negativo.", nameof(consumoMedio));
        }

        Placa = placa.Trim().ToUpperInvariant();
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        AnoFabricacao = anoFabricacao;
        AnoModelo = anoModelo;
        ValorFipeAtual = valorFipeAtual;
        Km = km;
        Motorizacao = motorizacao;
        ConsumoMedio = consumoMedio;
    }
}
