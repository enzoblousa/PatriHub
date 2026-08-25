namespace PatriHub.Domain.Entidades;

public enum TipoImovel
{
    Apartamento,
    Casa,
    Comercial,
    Terreno
}

/// <summary>Especialização de Ativo. Ver 01-SPEC-FUNCIONAL.md §4.3.</summary>
public sealed class Imovel : Ativo
{
    public override TipoAtivo Tipo => TipoAtivo.Imovel;

    public Endereco Endereco { get; private set; } = null!;
    public TipoImovel TipoImovel { get; private set; }
    public decimal AreaM2 { get; private set; }
    public string Matricula { get; private set; } = string.Empty;
    public decimal ValorIptuMensal { get; private set; }
    public decimal ValorCondominioMensal { get; private set; }

    private Imovel()
    {
        // EF Core
    }

    private Imovel(
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

    public static Imovel Cadastrar(
        Guid usuarioId,
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        Endereco endereco,
        TipoImovel tipoImovel,
        decimal areaM2,
        string matricula,
        decimal valorIptuMensal,
        decimal valorCondominioMensal,
        DadosFinanciamento? financiamento = null,
        DateTimeOffset? agora = null)
    {
        var momento = agora ?? DateTimeOffset.UtcNow;
        var imovel = new Imovel(usuarioId, apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, momento);
        imovel.AtualizarDadosDoImovel(endereco, tipoImovel, areaM2, matricula, valorIptuMensal, valorCondominioMensal);
        return imovel;
    }

    /// <summary>Edita os dados do Imóvel — o usuário pode reeditar qualquer campo, inclusive ValorMercadoAtual.</summary>
    public void Atualizar(
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        Endereco endereco,
        TipoImovel tipoImovel,
        decimal areaM2,
        string matricula,
        decimal valorIptuMensal,
        decimal valorCondominioMensal,
        DadosFinanciamento? financiamento,
        DateTimeOffset? agora = null)
    {
        AtualizarDadosComuns(apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, agora);
        AtualizarDadosDoImovel(endereco, tipoImovel, areaM2, matricula, valorIptuMensal, valorCondominioMensal);
    }

    private void AtualizarDadosDoImovel(
        Endereco endereco,
        TipoImovel tipoImovel,
        decimal areaM2,
        string matricula,
        decimal valorIptuMensal,
        decimal valorCondominioMensal)
    {
        ArgumentNullException.ThrowIfNull(endereco);

        if (areaM2 <= 0)
        {
            throw new ArgumentException("Área do imóvel deve ser maior que zero.", nameof(areaM2));
        }

        if (string.IsNullOrWhiteSpace(matricula))
        {
            throw new ArgumentException("Matrícula não pode ser vazia.", nameof(matricula));
        }

        if (valorIptuMensal < 0)
        {
            throw new ArgumentException("Valor de IPTU mensal não pode ser negativo.", nameof(valorIptuMensal));
        }

        if (valorCondominioMensal < 0)
        {
            throw new ArgumentException("Valor de condomínio mensal não pode ser negativo.", nameof(valorCondominioMensal));
        }

        Endereco = endereco;
        TipoImovel = tipoImovel;
        AreaM2 = areaM2;
        Matricula = matricula.Trim();
        ValorIptuMensal = valorIptuMensal;
        ValorCondominioMensal = valorCondominioMensal;
    }
}
