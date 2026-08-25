namespace PatriHub.Domain.Entidades;

/// <summary>
/// Pessoa física que aluga o Ativo de um usuário (dono do cadastro, via <see cref="UsuarioId"/>).
/// Ver CONTEXT.md.
/// </summary>
public sealed class Locatario
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    private Locatario()
    {
        // EF Core
    }

    private Locatario(Guid usuarioId, DateTimeOffset agora)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        CriadoEm = agora;
    }

    public static Locatario Cadastrar(Guid usuarioId, string nome, string cpf, string telefone, string email, DateTimeOffset? agora = null)
    {
        var momento = agora ?? DateTimeOffset.UtcNow;
        var locatario = new Locatario(usuarioId, momento);
        locatario.AtualizarDados(nome, cpf, telefone, email, momento);
        return locatario;
    }

    /// <summary>Edita os dados do Locatário — o usuário pode reeditar qualquer campo.</summary>
    public void Atualizar(string nome, string cpf, string telefone, string email, DateTimeOffset? agora = null)
    {
        AtualizarDados(nome, cpf, telefone, email, agora ?? DateTimeOffset.UtcNow);
    }

    private void AtualizarDados(string nome, string cpf, string telefone, string email, DateTimeOffset agora)
    {
        // Toda validação roda antes de qualquer atribuição — um CPF inválido não pode deixar o
        // Locatário com Nome/Telefone/Email já trocados e Cpf antigo (estado inconsistente).
        ExigirPreenchido(nome, nameof(nome), "Nome");
        ExigirPreenchido(telefone, nameof(telefone), "Telefone");
        ExigirPreenchido(email, nameof(email), "Email");
        var cpfNormalizado = NormalizarCpf(cpf);

        Nome = nome.Trim();
        Cpf = cpfNormalizado;
        Telefone = telefone.Trim();
        Email = email.Trim();
        AtualizadoEm = agora;
    }

    private static void ExigirPreenchido(string valor, string nomeParametro, string rotulo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"{rotulo} do locatário não pode ser vazio.", nomeParametro);
        }
    }

    /// <summary>Mantém apenas os dígitos — aceita CPF com ou sem máscara na entrada.</summary>
    private static string NormalizarCpf(string cpf)
    {
        var digitos = new string((cpf ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
        {
            throw new ArgumentException("CPF deve conter 11 dígitos.", nameof(cpf));
        }

        return digitos;
    }
}
