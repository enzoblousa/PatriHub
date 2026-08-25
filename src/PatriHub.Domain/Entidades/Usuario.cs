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

    private Usuario(Guid id, string nome, string email, PapelUsuario papel, DateTimeOffset criadoEm)
    {
        Id = id;
        Nome = nome;
        Email = email;
        Papel = papel;
        CriadoEm = criadoEm;
    }

    public static Usuario Registrar(string nome, string email, PapelUsuario papel = PapelUsuario.User, DateTimeOffset? agora = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do usuário não pode ser vazio.", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email do usuário não pode ser vazio.", nameof(email));
        }

        return new Usuario(Guid.NewGuid(), nome.Trim(), email.Trim().ToLowerInvariant(), papel, agora ?? DateTimeOffset.UtcNow);
    }
}
