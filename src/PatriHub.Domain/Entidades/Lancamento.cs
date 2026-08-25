namespace PatriHub.Domain.Entidades;

public enum TipoLancamento
{
    Receita,
    Despesa
}

/// <summary>
/// Categorias fixas de Lançamento (MVP) — as de Receita e as de Despesa compartilham o mesmo
/// enum porque `Outras` existe nas duas listas; <see cref="Lancamento.CategoriasPermitidas"/>
/// decide, por <see cref="TipoLancamento"/>, quais valores são válidos. Ver
/// 02-PLANO-TECNICO.md §4.7.
/// </summary>
public enum CategoriaLancamento
{
    // Categorias de Receita
    Aluguel,
    TaxaDeServico,
    MultaPorAtraso,

    // Categorias de Despesa
    Iptu,
    Condominio,
    Manutencao,
    Reforma,
    Seguro,
    Ipva,
    Multa,
    Financiamento,
    Administracao,
    ImpostoDeRenda,

    // Comum a Receita e Despesa
    Outras
}

/// <summary>
/// Registro financeiro de receita ou despesa associado a um Ativo, e opcionalmente a um
/// Contrato (via <see cref="ContratoId"/>, usado pela detecção de inadimplência do ticket #6).
/// Ver CONTEXT.md.
/// </summary>
public sealed class Lancamento
{
    private static readonly IReadOnlySet<CategoriaLancamento> CategoriasReceita = new HashSet<CategoriaLancamento>
    {
        CategoriaLancamento.Aluguel,
        CategoriaLancamento.TaxaDeServico,
        CategoriaLancamento.MultaPorAtraso,
        CategoriaLancamento.Outras
    };

    private static readonly IReadOnlySet<CategoriaLancamento> CategoriasDespesa = new HashSet<CategoriaLancamento>
    {
        CategoriaLancamento.Iptu,
        CategoriaLancamento.Condominio,
        CategoriaLancamento.Manutencao,
        CategoriaLancamento.Reforma,
        CategoriaLancamento.Seguro,
        CategoriaLancamento.Ipva,
        CategoriaLancamento.Multa,
        CategoriaLancamento.Financiamento,
        CategoriaLancamento.Administracao,
        CategoriaLancamento.ImpostoDeRenda,
        CategoriaLancamento.Outras
    };

    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid AtivoId { get; private set; }

    /// <summary>Vincula a receita ao Contrato correspondente (ticket #5); ausente em qualquer outro lançamento.</summary>
    public Guid? ContratoId { get; private set; }

    public TipoLancamento Tipo { get; private set; }
    public CategoriaLancamento Categoria { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string? Descricao { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    private Lancamento()
    {
        // EF Core
    }

    private Lancamento(Guid usuarioId, Guid ativoId, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        AtivoId = ativoId;
        CriadoEm = agora;
    }

    public static Lancamento Registrar(
        Guid usuarioId,
        Guid ativoId,
        TipoLancamento tipo,
        CategoriaLancamento categoria,
        decimal valor,
        DateOnly data,
        string? descricao,
        Guid? contratoId = null,
        DateTimeOffset? agora = null)
    {
        var momento = agora ?? DateTimeOffset.UtcNow;
        var lancamento = new Lancamento(usuarioId, ativoId, momento);
        lancamento.AtualizarDados(tipo, categoria, valor, data, descricao, contratoId, momento);
        return lancamento;
    }

    /// <summary>Edita os dados do Lançamento — o usuário pode reeditar qualquer campo, inclusive Tipo e Categoria.</summary>
    public void Atualizar(
        TipoLancamento tipo,
        CategoriaLancamento categoria,
        decimal valor,
        DateOnly data,
        string? descricao,
        Guid? contratoId,
        DateTimeOffset? agora = null)
    {
        AtualizarDados(tipo, categoria, valor, data, descricao, contratoId, agora ?? DateTimeOffset.UtcNow);
    }

    private void AtualizarDados(
        TipoLancamento tipo,
        CategoriaLancamento categoria,
        decimal valor,
        DateOnly data,
        string? descricao,
        Guid? contratoId,
        DateTimeOffset agora)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("Valor do lançamento deve ser maior que zero.", nameof(valor));
        }

        if (!CategoriasPermitidas(tipo).Contains(categoria))
        {
            throw new ArgumentException(
                $"Categoria '{categoria}' não é válida para lançamento do tipo '{tipo}'.",
                nameof(categoria));
        }

        Tipo = tipo;
        Categoria = categoria;
        Valor = valor;
        Data = data;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        ContratoId = contratoId;
        AtualizadoEm = agora;
    }

    /// <summary>Lista fixa de categorias válidas para um Tipo de lançamento — ver 02-PLANO-TECNICO.md §4.7.</summary>
    public static IReadOnlySet<CategoriaLancamento> CategoriasPermitidas(TipoLancamento tipo) =>
        tipo == TipoLancamento.Receita ? CategoriasReceita : CategoriasDespesa;
}
