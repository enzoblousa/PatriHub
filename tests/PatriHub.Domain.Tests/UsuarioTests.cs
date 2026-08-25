using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class UsuarioTests
{
    [Fact]
    public void Registrar_com_nome_e_email_validos_cria_usuario_com_papel_User_por_padrao()
    {
        var usuario = Usuario.Registrar("Maria Silva", "maria@example.com");

        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("Maria Silva", usuario.Nome);
        Assert.Equal("maria@example.com", usuario.Email);
        Assert.Equal(PapelUsuario.User, usuario.Papel);
    }

    [Fact]
    public void Registrar_normaliza_o_email_para_minusculas_e_remove_espacos()
    {
        var usuario = Usuario.Registrar("Maria Silva", "  Maria@Example.com  ");

        Assert.Equal("maria@example.com", usuario.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registrar_com_nome_vazio_lanca_ArgumentException(string nomeInvalido)
    {
        Assert.Throws<ArgumentException>(() => Usuario.Registrar(nomeInvalido, "maria@example.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registrar_com_email_vazio_lanca_ArgumentException(string emailInvalido)
    {
        Assert.Throws<ArgumentException>(() => Usuario.Registrar("Maria Silva", emailInvalido));
    }
}
