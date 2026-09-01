namespace PatriHub.Domain.Entidades;

public enum PapelUsuario
{
    User,
    Admin
}

/// <summary>
/// Dono do(s) Ativo(s): pessoa física com poucos imóveis/carros. Ver CONTEXT.md.
/// </summary>
public sealed class Usuario
{
    public Guid Id { get; }
    public string Nome { get; private set; }
    public string Email { get; }
    public PapelUsuario Papel { get; }
    public DateTimeOffset CriadoEm { get; }
    public DateTimeOffset ConsentimentoLgpdEm { get; }

    private Usuario(Guid id, string nome, string email, PapelUsuario papel, DateTimeOffset criadoEm, DateTimeOffset consentimentoLgpdEm)
    {
        Id = id;
        Nome = nome;
        Email = email;
        Papel = papel;
        CriadoEm = criadoEm;
        ConsentimentoLgpdEm = consentimentoLgpdEm;
    }

    /// <summary>
    /// `consentimentoLgpd = true` por padrão só pra não obrigar todo teste alheio ao LGPD a
    /// passar o parâmetro — o `RegistrarUsuarioRequest` da API não tem esse default (ver
    /// AutenticacaoDtos.cs), então um registro real sempre precisa do aceite explícito do
    /// cliente (ver docs/spec/01-SPEC-FUNCIONAL.md §8).
    /// </summary>
    public static Usuario Registrar(
        string nome,
        string email,
        bool consentimentoLgpd = true,
        PapelUsuario papel = PapelUsuario.User,
        DateTimeOffset? agora = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do usuário não pode ser vazio.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email do usuário não pode ser vazio.", nameof(email));
        }

        if (!consentimentoLgpd)
        {
            throw new ArgumentException(
                "É necessário aceitar o uso dos dados desta versão beta.",
                nameof(consentimentoLgpd));
        }

        var momento = agora ?? DateTimeOffset.UtcNow;
        return new Usuario(Guid.NewGuid(), nome.Trim(), NormalizarEmail(email), papel, momento, momento);
    }

    /// <summary>
    /// Mesma normalização usada ao registrar um Usuario — reaproveitada no login, para que a
    /// busca por email sempre bata com o que foi persistido.
    /// </summary>
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
