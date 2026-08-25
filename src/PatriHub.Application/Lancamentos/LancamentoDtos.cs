using PatriHub.Domain.Entidades;

namespace PatriHub.Application.Lancamentos;

/// <summary>Corpo de criação (POST) e edição (PUT) de um Lançamento — os mesmos campos são exigidos nos dois casos.</summary>
public sealed record LancamentoRequest(
    Guid AtivoId,
    TipoLancamento Tipo,
    CategoriaLancamento Categoria,
    decimal Valor,
    DateOnly Data,
    string? Descricao,
    Guid? ContratoId);

public sealed record LancamentoDto(
    Guid Id,
    Guid AtivoId,
    Guid? ContratoId,
    TipoLancamento Tipo,
    CategoriaLancamento Categoria,
    decimal Valor,
    DateOnly Data,
    string? Descricao,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);

/// <summary>Filtros da listagem (AC "lista/filtra Lançamentos por Ativo, período e tipo") — todos opcionais.</summary>
public sealed record LancamentoFiltro(Guid? AtivoId, DateOnly? DataInicio, DateOnly? DataFim, TipoLancamento? Tipo);
