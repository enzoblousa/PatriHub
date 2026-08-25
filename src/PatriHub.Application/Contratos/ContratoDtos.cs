using PatriHub.Domain.Entidades;

namespace PatriHub.Application.Contratos;

/// <summary>Corpo de criação (POST) de um Contrato — não há edição no MVP, apenas criação e encerramento.</summary>
public sealed record ContratoRequest(
    Guid AtivoId,
    Guid LocatarioId,
    decimal ValorAluguelMensal,
    int DiaVencimento,
    DateOnly DataInicio,
    DateOnly? DataFim);

public sealed record ContratoDto(
    Guid Id,
    Guid AtivoId,
    Guid LocatarioId,
    decimal ValorAluguelMensal,
    int DiaVencimento,
    DateOnly DataInicio,
    DateOnly? DataFim,
    StatusContrato Status,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
