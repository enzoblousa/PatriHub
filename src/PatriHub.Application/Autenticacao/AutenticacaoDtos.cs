namespace PatriHub.Application.Autenticacao;

public sealed record RegistrarUsuarioRequest(string Nome, string Email, string Senha);

public sealed record LoginRequest(string Email, string Senha);

public sealed record UsuarioDto(Guid Id, string Nome, string Email, string Papel);

public sealed record ResultadoAutenticacao(
    bool Sucesso,
    string? Erro,
    string? Token,
    DateTimeOffset? ExpiraEm,
    UsuarioDto? Usuario)
{
    public static ResultadoAutenticacao ComSucesso(string token, DateTimeOffset expiraEm, UsuarioDto usuario) =>
        new(true, null, token, expiraEm, usuario);

    public static ResultadoAutenticacao ComErro(string erro) =>
        new(false, erro, null, null, null);
}
