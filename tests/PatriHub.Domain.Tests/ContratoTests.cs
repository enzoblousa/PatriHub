using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class ContratoTests
{
    private static Contrato ContratoValido(
        Guid? usuarioId = null,
        Guid? ativoId = null,
        Guid? locatarioId = null,
        DateOnly? dataInicio = null,
        DateOnly? dataFim = null) =>
        Contrato.Cadastrar(
            usuarioId ?? Guid.NewGuid(),
            ativoId ?? Guid.NewGuid(),
            locatarioId ?? Guid.NewGuid(),
            valorAluguelMensal: 1_500m,
            diaVencimento: 10,
            dataInicio: dataInicio ?? new DateOnly(2026, 1, 1),
            dataFim: dataFim);

    [Fact]
    public void Cadastrar_com_dados_validos_cria_contrato_com_status_Ativo()
    {
        var usuarioId = Guid.NewGuid();
        var ativoId = Guid.NewGuid();
        var locatarioId = Guid.NewGuid();

        var contrato = Contrato.Cadastrar(usuarioId, ativoId, locatarioId, 1_500m, 10, new DateOnly(2026, 1, 1), null);

        Assert.NotEqual(Guid.Empty, contrato.Id);
        Assert.Equal(usuarioId, contrato.UsuarioId);
        Assert.Equal(ativoId, contrato.AtivoId);
        Assert.Equal(locatarioId, contrato.LocatarioId);
        Assert.Equal(1_500m, contrato.ValorAluguelMensal);
        Assert.Equal(10, contrato.DiaVencimento);
        Assert.Equal(new DateOnly(2026, 1, 1), contrato.DataInicio);
        Assert.Null(contrato.DataFim);
        Assert.Equal(StatusContrato.Ativo, contrato.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Cadastrar_com_valor_de_aluguel_zero_ou_negativo_lanca_ArgumentException(decimal valorInvalido)
    {
        Assert.Throws<ArgumentException>(() => Contrato.Cadastrar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), valorInvalido, 10, new DateOnly(2026, 1, 1), null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void Cadastrar_com_dia_de_vencimento_fora_do_intervalo_lanca_ArgumentException(int diaInvalido)
    {
        Assert.Throws<ArgumentException>(() => Contrato.Cadastrar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1_500m, diaInvalido, new DateOnly(2026, 1, 1), null));
    }

    [Fact]
    public void Cadastrar_com_DataFim_anterior_a_DataInicio_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Contrato.Cadastrar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1_500m, 10,
            dataInicio: new DateOnly(2026, 3, 1), dataFim: new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Encerrar_um_contrato_Ativo_muda_status_para_Encerrado()
    {
        var contrato = ContratoValido();

        contrato.Encerrar();

        Assert.Equal(StatusContrato.Encerrado, contrato.Status);
    }

    [Fact]
    public void Encerrar_um_contrato_ja_Encerrado_lanca_ArgumentException()
    {
        var contrato = ContratoValido();
        contrato.Encerrar();

        Assert.Throws<ArgumentException>(() => contrato.Encerrar());
    }

    [Fact]
    public void MarcarInadimplente_um_contrato_Ativo_muda_status_para_Inadimplente()
    {
        var contrato = ContratoValido();

        contrato.MarcarInadimplente();

        Assert.Equal(StatusContrato.Inadimplente, contrato.Status);
    }

    [Fact]
    public void MarcarInadimplente_um_contrato_Encerrado_lanca_ArgumentException()
    {
        var contrato = ContratoValido();
        contrato.Encerrar();

        Assert.Throws<ArgumentException>(() => contrato.MarcarInadimplente());
    }

    [Fact]
    public void MarcarInadimplente_um_contrato_ja_Inadimplente_lanca_ArgumentException()
    {
        var contrato = ContratoValido();
        contrato.MarcarInadimplente();

        Assert.Throws<ArgumentException>(() => contrato.MarcarInadimplente());
    }
}
