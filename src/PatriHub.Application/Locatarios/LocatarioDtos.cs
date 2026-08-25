namespace PatriHub.Application.Locatarios;

/// <summary>Corpo de cadastro (POST) e edição (PUT) de um Locatário — os mesmos campos são exigidos nos dois casos.</summary>
public sealed record LocatarioRequest(string Nome, string Cpf, string Telefone, string Email);

public sealed record LocatarioDto(
    Guid Id,
    string Nome,
    string Cpf,
    string Telefone,
    string Email,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);
