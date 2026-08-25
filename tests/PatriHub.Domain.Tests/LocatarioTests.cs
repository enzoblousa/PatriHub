using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class LocatarioTests
{
    private static Locatario LocatarioValido(Guid? usuarioId = null) =>
        Locatario.Cadastrar(usuarioId ?? Guid.NewGuid(), "João Souza", "123.456.789-09", "(11) 99999-0000", "joao@example.com");

    [Fact]
    public void Cadastrar_com_dados_validos_cria_locatario()
    {
        var usuarioId = Guid.NewGuid();

        var locatario = Locatario.Cadastrar(usuarioId, "João Souza", "123.456.789-09", "(11) 99999-0000", "joao@example.com");

        Assert.NotEqual(Guid.Empty, locatario.Id);
        Assert.Equal(usuarioId, locatario.UsuarioId);
        Assert.Equal("João Souza", locatario.Nome);
        Assert.Equal("(11) 99999-0000", locatario.Telefone);
        Assert.Equal("joao@example.com", locatario.Email);
    }

    [Fact]
    public void Cadastrar_normaliza_CPF_removendo_a_mascara()
    {
        var locatario = Locatario.Cadastrar(Guid.NewGuid(), "João Souza", "123.456.789-09", "11999990000", "joao@example.com");

        Assert.Equal("12345678909", locatario.Cpf);
    }

    [Theory]
    [InlineData("123.456.789-0")]
    [InlineData("123456789")]
    [InlineData("")]
    public void Cadastrar_com_CPF_sem_11_digitos_lanca_ArgumentException(string cpfInvalido)
    {
        Assert.Throws<ArgumentException>(() => Locatario.Cadastrar(Guid.NewGuid(), "João Souza", cpfInvalido, "11999990000", "joao@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cadastrar_com_nome_vazio_lanca_ArgumentException(string nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() => Locatario.Cadastrar(Guid.NewGuid(), nomeInvalido, "123.456.789-09", "11999990000", "joao@example.com"));
    }

    [Fact]
    public void Cadastrar_com_email_vazio_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Locatario.Cadastrar(Guid.NewGuid(), "João Souza", "123.456.789-09", "11999990000", " "));
    }

    [Fact]
    public void Atualizar_troca_todos_os_campos_editaveis()
    {
        var locatario = LocatarioValido();

        locatario.Atualizar("Maria Lima", "987.654.321-00", "(21) 98888-1111", "maria@example.com");

        Assert.Equal("Maria Lima", locatario.Nome);
        Assert.Equal("98765432100", locatario.Cpf);
        Assert.Equal("(21) 98888-1111", locatario.Telefone);
        Assert.Equal("maria@example.com", locatario.Email);
    }

    [Fact]
    public void Atualizar_com_CPF_invalido_lanca_ArgumentException_e_nao_altera_estado()
    {
        var locatario = LocatarioValido();

        Assert.Throws<ArgumentException>(() => locatario.Atualizar("Maria Lima", "123", "(21) 98888-1111", "maria@example.com"));

        Assert.Equal("João Souza", locatario.Nome);
        Assert.Equal("12345678909", locatario.Cpf);
    }
}
