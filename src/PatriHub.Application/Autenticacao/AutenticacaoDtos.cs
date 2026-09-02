namespace PatriHub.Application.Autenticacao;

/// <summary>
/// `ConsentimentoLgpd` não tem valor default: o JSON precisa trazer o campo explicitamente
/// (a ausência é desserializada como `false` pelo System.Text.Json), então um cliente que não
/// manda o aceite é rejeitado do mesmo jeito que um que manda `false` — ver
/// docs/spec/01-SPEC-FUNCIONAL.md §8 e Usuario.Registrar.
/// </summary>
public sealed record RegistrarUsuarioRequest(string Nome, string Email, string Senha, bool ConsentimentoLgpd);

public sealed record LoginRequest(string Email, string Senha);

/// <summary>Pedido de link de recuperação — ver ADR-0009. Só o email, nunca revela senha/token pro chamador.</summary>
public sealed record SolicitarRecuperacaoSenhaRequest(string Email);

/// <summary>
/// `Token` é o valor opaco gerado por `UserManager.GeneratePasswordResetTokenAsync` (ver
/// ADR-0009) — vem do link do email, nunca é algo que o usuário digita de cabeça.
/// </summary>
public sealed record RedefinirSenhaRequest(string Email, string Token, string NovaSenha);

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
