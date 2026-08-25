namespace PatriHub.Domain.Entidades;

public enum TipoAtivo
{
    Imovel,
    Carro
}

/// <summary>
/// Campo semi-automático: `Alugado`/`Vago` são derivados do ciclo de vida do Contrato (ticket
/// #5); `Manutenção`/`À venda` só são setados manualmente pelo usuário. Ver CONTEXT.md.
/// </summary>
public enum StatusAtivo
{
    Vago,
    Alugado,
    Manutencao,
    AVenda
}

/// <summary>
/// Termo genérico para Imóvel ou Carro cadastrado por um usuário; unidade básica de
/// acompanhamento financeiro (um Ativo tem um único dono). Ver CONTEXT.md.
/// </summary>
public abstract class Ativo
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Apelido { get; private set; } = string.Empty;
    public abstract TipoAtivo Tipo { get; }
    public StatusAtivo Status { get; private set; }
    public DateOnly DataAquisicao { get; private set; }
    public decimal ValorAquisicao { get; private set; }
    public decimal ValorMercadoAtual { get; private set; }
    public DadosFinanciamento? Financiamento { get; private set; }
    public bool Financiado => Financiamento is not null;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }
    public DateTimeOffset? ExcluidoEm { get; private set; }
    public bool Excluido => ExcluidoEm is not null;

    protected Ativo()
    {
        // EF Core
    }

    protected Ativo(
        Guid usuarioId,
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        DadosFinanciamento? financiamento,
        DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Status = StatusAtivo.Vago;
        CriadoEm = agora;
        AtualizarDadosComuns(apelido, dataAquisicao, valorAquisicao, valorMercadoAtual, financiamento, agora);
    }

    /// <summary>
    /// Atualiza os campos comuns a qualquer Ativo — reaproveitado tanto no cadastro quanto na
    /// edição (chamado pelo <c>Atualizar</c> de <see cref="Imovel"/>/<see cref="Carro"/>).
    /// </summary>
    protected void AtualizarDadosComuns(
        string apelido,
        DateOnly dataAquisicao,
        decimal valorAquisicao,
        decimal valorMercadoAtual,
        DadosFinanciamento? financiamento,
        DateTimeOffset? agora = null)
    {
        if (string.IsNullOrWhiteSpace(apelido))
        {
            throw new ArgumentException("Apelido do ativo não pode ser vazio.", nameof(apelido));
        }

        if (valorAquisicao < 0)
        {
            throw new ArgumentException("Valor de aquisição não pode ser negativo.", nameof(valorAquisicao));
        }

        if (valorMercadoAtual < 0)
        {
            throw new ArgumentException("Valor de mercado atual não pode ser negativo.", nameof(valorMercadoAtual));
        }

        Apelido = apelido.Trim();
        DataAquisicao = dataAquisicao;
        ValorAquisicao = valorAquisicao;
        ValorMercadoAtual = valorMercadoAtual;
        Financiamento = financiamento;
        AtualizadoEm = agora ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marca manualmente o Ativo como Manutenção ou À venda. Alugado/Vago são derivados
    /// automaticamente do ciclo de vida do Contrato (ticket #5) — nunca setados por aqui.
    /// </summary>
    public void MarcarStatusManual(StatusAtivo status, DateTimeOffset? agora = null)
    {
        if (status is not (StatusAtivo.Manutencao or StatusAtivo.AVenda))
        {
            throw new ArgumentException(
                "Status manual só pode ser Manutenção ou À venda — Alugado/Vago são automáticos.",
                nameof(status));
        }

        Status = status;
        AtualizadoEm = agora ?? DateTimeOffset.UtcNow;
    }

    /// <summary>Soft delete: some da listagem, mas o dado permanece no banco.</summary>
    public void Excluir(DateTimeOffset? agora = null)
    {
        var momento = agora ?? DateTimeOffset.UtcNow;
        ExcluidoEm = momento;
        AtualizadoEm = momento;
    }
}
