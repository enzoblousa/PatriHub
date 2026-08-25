namespace PatriHub.Application.Common;

public enum TipoErroOperacao
{
    Validacao,
    NaoEncontrado
}

/// <summary>
/// Resultado de uma operação sem retorno de dado (ex.: exclusão). Distingue erro de validação
/// (400) de "não encontrado" (404) — inclui aqui o caso de um Ativo de outro usuário, que a
/// query já filtra por `UsuarioId` e portanto aparenta não existir (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public sealed record ResultadoOperacao(bool Sucesso, string? Erro, TipoErroOperacao? TipoErro)
{
    public static ResultadoOperacao ComSucesso() => new(true, null, null);

    public static ResultadoOperacao ComErro(string erro, TipoErroOperacao tipoErro) => new(false, erro, tipoErro);
}

/// <summary>Mesma semântica de <see cref="ResultadoOperacao"/>, com um dado de retorno em caso de sucesso.</summary>
public sealed record ResultadoOperacao<T>(bool Sucesso, string? Erro, TipoErroOperacao? TipoErro, T? Dado)
{
    public static ResultadoOperacao<T> ComSucesso(T dado) => new(true, null, null, dado);

    public static ResultadoOperacao<T> ComErro(string erro, TipoErroOperacao tipoErro) => new(false, erro, tipoErro, default);
}
