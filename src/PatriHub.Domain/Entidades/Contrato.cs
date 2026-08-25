namespace PatriHub.Domain.Entidades;

/// <summary>
/// `Inadimplente` é atribuído automaticamente pelo job periódico do ticket #6 — nenhum método
/// deste ticket (#5) o define. Ver CONTEXT.md.
/// </summary>
public enum StatusContrato
{
    Ativo,
    Encerrado,
    Inadimplente
}

/// <summary>
/// Vínculo de locação entre um Ativo e um Locatário. A existência de um Contrato `Ativo` dirige
/// automaticamente o <see cref="Ativo.Status"/> correspondente — ver
/// <see cref="Ativo.MarcarAlugado"/>/<see cref="Ativo.MarcarVago"/>. Ver CONTEXT.md.
/// </summary>
public sealed class Contrato
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid AtivoId { get; private set; }
    public Guid LocatarioId { get; private set; }
    public decimal ValorAluguelMensal { get; private set; }
    public int DiaVencimento { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly? DataFim { get; private set; }
    public StatusContrato Status { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    private Contrato()
    {
        // EF Core
    }

    private Contrato(Guid usuarioId, Guid ativoId, Guid locatarioId, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        AtivoId = ativoId;
        LocatarioId = locatarioId;
        Status = StatusContrato.Ativo;
        CriadoEm = agora;
    }

    /// <summary>
    /// Cria um Contrato já com Status `Ativo` — a regra de "um Ativo só pode ter um Contrato
    /// `Ativo` por vez" é responsabilidade do ContratoService (precisa olhar outros Contratos,
    /// não só o que está sendo criado).
    /// </summary>
    public static Contrato Cadastrar(
        Guid usuarioId,
        Guid ativoId,
        Guid locatarioId,
        decimal valorAluguelMensal,
        int diaVencimento,
        DateOnly dataInicio,
        DateOnly? dataFim,
        DateTimeOffset? agora = null)
    {
        if (valorAluguelMensal <= 0)
        {
            throw new ArgumentException("Valor de aluguel mensal deve ser maior que zero.", nameof(valorAluguelMensal));
        }

        if (diaVencimento is < 1 or > 31)
        {
            throw new ArgumentException("Dia de vencimento deve estar entre 1 e 31.", nameof(diaVencimento));
        }

        if (dataFim is { } fim && fim < dataInicio)
        {
            throw new ArgumentException("Data de fim não pode ser anterior à data de início.", nameof(dataFim));
        }

        var momento = agora ?? DateTimeOffset.UtcNow;
        var contrato = new Contrato(usuarioId, ativoId, locatarioId, momento)
        {
            ValorAluguelMensal = valorAluguelMensal,
            DiaVencimento = diaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            AtualizadoEm = momento
        };
        return contrato;
    }

    /// <summary>Encerra o Contrato — reverter o Ativo para Vago é responsabilidade do ContratoService.</summary>
    public void Encerrar(DateTimeOffset? agora = null)
    {
        if (Status != StatusContrato.Ativo)
        {
            throw new ArgumentException("Somente um contrato com status Ativo pode ser encerrado.", nameof(Status));
        }

        Status = StatusContrato.Encerrado;
        AtualizadoEm = agora ?? DateTimeOffset.UtcNow;
    }
}
