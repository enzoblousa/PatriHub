using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class CarroTests
{
    private static Carro CarroValido(Guid? usuarioId = null, Motorizacao motorizacao = Motorizacao.Combustao) =>
        Carro.Cadastrar(
            usuarioId ?? Guid.NewGuid(),
            "Corolla",
            new DateOnly(2022, 3, 15),
            valorAquisicao: 120_000m,
            valorMercadoAtual: 100_000m,
            placa: "abc1d23",
            marca: "Toyota",
            modelo: "Corolla",
            anoFabricacao: 2022,
            anoModelo: 2022,
            valorFipeAtual: 105_000m,
            km: 30_000m,
            motorizacao: motorizacao,
            consumoMedio: 14.5m);

    [Fact]
    public void Cadastrar_com_dados_validos_cria_carro_com_status_Vago()
    {
        var usuarioId = Guid.NewGuid();

        var carro = CarroValido(usuarioId);

        Assert.NotEqual(Guid.Empty, carro.Id);
        Assert.Equal(usuarioId, carro.UsuarioId);
        Assert.Equal(TipoAtivo.Carro, carro.Tipo);
        Assert.Equal(StatusAtivo.Vago, carro.Status);
        Assert.False(carro.Financiado);
    }

    [Fact]
    public void Cadastrar_normaliza_a_placa_para_maiusculas()
    {
        var carro = CarroValido();

        Assert.Equal("ABC1D23", carro.Placa);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cadastrar_com_placa_vazia_lanca_ArgumentException(string placaInvalida)
    {
        Assert.Throws<ArgumentException>(() => Carro.Cadastrar(
            Guid.NewGuid(), "Corolla", new DateOnly(2022, 3, 15), 120_000m, 100_000m,
            placaInvalida, "Toyota", "Corolla", 2022, 2022, 105_000m, 30_000m, Motorizacao.Combustao, 14.5m));
    }

    [Fact]
    public void Cadastrar_com_ano_modelo_anterior_ao_ano_fabricacao_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Carro.Cadastrar(
            Guid.NewGuid(), "Corolla", new DateOnly(2022, 3, 15), 120_000m, 100_000m,
            "ABC1D23", "Toyota", "Corolla", anoFabricacao: 2022, anoModelo: 2021, 105_000m, 30_000m, Motorizacao.Combustao, 14.5m));
    }

    [Fact]
    public void Cadastrar_com_km_negativo_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Carro.Cadastrar(
            Guid.NewGuid(), "Corolla", new DateOnly(2022, 3, 15), 120_000m, 100_000m,
            "ABC1D23", "Toyota", "Corolla", 2022, 2022, 105_000m, km: -1m, motorizacao: Motorizacao.Combustao, consumoMedio: 14.5m));
    }

    [Fact]
    public void Atualizar_troca_ValorMercadoAtual_e_demais_campos()
    {
        var carro = CarroValido();

        carro.Atualizar(
            "Corolla Prata", new DateOnly(2022, 3, 15), 120_000m, valorMercadoAtual: 95_000m,
            "XYZ9A88", "Toyota", "Corolla", 2022, 2023, 98_000m, 45_000m, Motorizacao.Combustao, 13m, financiamento: null);

        Assert.Equal(95_000m, carro.ValorMercadoAtual);
        Assert.Equal("Corolla Prata", carro.Apelido);
        Assert.Equal("XYZ9A88", carro.Placa);
        Assert.Equal(45_000m, carro.Km);
    }

    [Fact]
    public void Cadastrar_com_Motorizacao_Eletrico_nao_exige_nada_alem_do_enum()
    {
        var carro = CarroValido(motorizacao: Motorizacao.Eletrico);

        Assert.Equal(Motorizacao.Eletrico, carro.Motorizacao);
    }

    [Fact]
    public void Atualizar_troca_a_Motorizacao()
    {
        var carro = CarroValido(motorizacao: Motorizacao.Combustao);

        carro.Atualizar(
            carro.Apelido, carro.DataAquisicao, carro.ValorAquisicao, carro.ValorMercadoAtual,
            carro.Placa, carro.Marca, carro.Modelo, carro.AnoFabricacao, carro.AnoModelo,
            carro.ValorFipeAtual, carro.Km, Motorizacao.Eletrico, carro.ConsumoMedio, financiamento: null);

        Assert.Equal(Motorizacao.Eletrico, carro.Motorizacao);
    }

    [Fact]
    public void MarcarStatusManual_com_Manutencao_altera_o_status()
    {
        var carro = CarroValido();

        carro.MarcarStatusManual(StatusAtivo.Manutencao);

        Assert.Equal(StatusAtivo.Manutencao, carro.Status);
    }

    [Fact]
    public void Excluir_marca_ExcluidoEm_e_Excluido_fica_true()
    {
        var carro = CarroValido();

        carro.Excluir();

        Assert.True(carro.Excluido);
        Assert.NotNull(carro.ExcluidoEm);
    }
}
