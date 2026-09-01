using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class ImovelTests
{
    private static Endereco EnderecoValido() =>
        Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01000-000");

    private static Imovel ImovelValido(Guid? usuarioId = null) =>
        Imovel.Cadastrar(
            usuarioId ?? Guid.NewGuid(),
            "Apê Centro",
            new DateOnly(2020, 1, 10),
            valorAquisicao: 300_000m,
            valorMercadoAtual: 350_000m,
            EnderecoValido(),
            TipoImovel.Apartamento,
            areaM2: 65m,
            matricula: "12345",
            valorIptuMensal: 150m,
            valorCondominioMensal: 400m);

    [Fact]
    public void Cadastrar_com_dados_validos_cria_imovel_com_status_Vago()
    {
        var usuarioId = Guid.NewGuid();

        var imovel = ImovelValido(usuarioId);

        Assert.NotEqual(Guid.Empty, imovel.Id);
        Assert.Equal(usuarioId, imovel.UsuarioId);
        Assert.Equal(TipoAtivo.Imovel, imovel.Tipo);
        Assert.Equal(StatusAtivo.Vago, imovel.Status);
        Assert.False(imovel.Financiado);
        Assert.False(imovel.Excluido);
    }

    [Fact]
    public void Cadastrar_com_financiamento_marca_ativo_como_Financiado()
    {
        var financiamento = DadosFinanciamento.Criar(1_500m, 200_000m, 9.5m, 120);

        var imovel = Imovel.Cadastrar(
            Guid.NewGuid(), "Apê Centro", new DateOnly(2020, 1, 10), 300_000m, 350_000m,
            EnderecoValido(), TipoImovel.Apartamento, 65m, "12345", 150m, 400m,
            financiamento: financiamento);

        Assert.True(imovel.Financiado);
        Assert.Same(financiamento, imovel.Financiamento);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Cadastrar_com_apelido_vazio_lanca_ArgumentException(string apelidoInvalido)
    {
        Assert.Throws<ArgumentException>(() => Imovel.Cadastrar(
            Guid.NewGuid(), apelidoInvalido, new DateOnly(2020, 1, 10), 300_000m, 350_000m,
            EnderecoValido(), TipoImovel.Apartamento, 65m, "12345", 150m, 400m));
    }

    [Fact]
    public void Cadastrar_com_area_zero_ou_negativa_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Imovel.Cadastrar(
            Guid.NewGuid(), "Apê Centro", new DateOnly(2020, 1, 10), 300_000m, 350_000m,
            EnderecoValido(), TipoImovel.Apartamento, areaM2: 0m, "12345", 150m, 400m));
    }

    [Fact]
    public void Cadastrar_com_valor_de_aquisicao_negativo_lanca_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Imovel.Cadastrar(
            Guid.NewGuid(), "Apê Centro", new DateOnly(2020, 1, 10), valorAquisicao: -1m, 350_000m,
            EnderecoValido(), TipoImovel.Apartamento, 65m, "12345", 150m, 400m));
    }

    /// <summary>Ver docs/adr/0008 — antes só se checava `Length == 2`, aceitando qualquer par de letras.</summary>
    [Theory]
    [InlineData("ZZ")]
    [InlineData("XX")]
    public void Endereco_Criar_com_uf_fora_da_lista_das_27_lanca_ArgumentException(string ufInvalida)
    {
        Assert.Throws<ArgumentException>(() =>
            Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", ufInvalida, "01000-000"));
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPP")]
    public void Endereco_Criar_com_uf_de_tamanho_errado_lanca_ArgumentException(string ufInvalida)
    {
        Assert.Throws<ArgumentException>(() =>
            Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", ufInvalida, "01000-000"));
    }

    [Fact]
    public void Endereco_Criar_normaliza_uf_minuscula_para_maiuscula()
    {
        var endereco = Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", "sp", "01000-000");

        Assert.Equal("SP", endereco.Uf);
    }

    /// <summary>Ver docs/adr/0008 — antes só se checava não-vazio, sem validar a quantidade de dígitos.</summary>
    [Theory]
    [InlineData("0100")] // poucos dígitos
    [InlineData("010000000")] // dígitos demais
    public void Endereco_Criar_com_cep_fora_de_8_digitos_lanca_ArgumentException(string cepInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", cepInvalido));
    }

    [Fact]
    public void Endereco_Criar_aceita_cep_com_ou_sem_traco()
    {
        var comTraco = Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01000-000");
        var semTraco = Endereco.Criar("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01000000");

        Assert.Equal("01000-000", comTraco.Cep);
        Assert.Equal("01000000", semTraco.Cep);
    }

    [Fact]
    public void Atualizar_troca_ValorMercadoAtual_e_demais_campos()
    {
        var imovel = ImovelValido();
        var novoEndereco = Endereco.Criar("Av. Paulista", "1000", "Ap 51", "Bela Vista", "São Paulo", "SP", "01310-000");

        imovel.Atualizar(
            "Apê Paulista", new DateOnly(2020, 1, 10), 300_000m, valorMercadoAtual: 420_000m,
            novoEndereco, TipoImovel.Apartamento, 70m, "54321", 180m, 500m, financiamento: null);

        Assert.Equal(420_000m, imovel.ValorMercadoAtual);
        Assert.Equal("Apê Paulista", imovel.Apelido);
        Assert.Equal("54321", imovel.Matricula);
        Assert.Same(novoEndereco, imovel.Endereco);
    }

    [Fact]
    public void MarcarStatusManual_com_Manutencao_altera_o_status()
    {
        var imovel = ImovelValido();

        imovel.MarcarStatusManual(StatusAtivo.Manutencao);

        Assert.Equal(StatusAtivo.Manutencao, imovel.Status);
    }

    [Fact]
    public void MarcarStatusManual_com_AVenda_altera_o_status()
    {
        var imovel = ImovelValido();

        imovel.MarcarStatusManual(StatusAtivo.AVenda);

        Assert.Equal(StatusAtivo.AVenda, imovel.Status);
    }

    [Theory]
    [InlineData(StatusAtivo.Alugado)]
    [InlineData(StatusAtivo.Vago)]
    public void MarcarStatusManual_com_Alugado_ou_Vago_lanca_ArgumentException(StatusAtivo statusAutomatico)
    {
        var imovel = ImovelValido();

        Assert.Throws<ArgumentException>(() => imovel.MarcarStatusManual(statusAutomatico));
    }

    [Fact]
    public void MarcarAlugado_altera_o_status_para_Alugado()
    {
        var imovel = ImovelValido();

        imovel.MarcarAlugado();

        Assert.Equal(StatusAtivo.Alugado, imovel.Status);
    }

    [Fact]
    public void MarcarAlugado_sobrepoe_um_status_manual_anterior()
    {
        var imovel = ImovelValido();
        imovel.MarcarStatusManual(StatusAtivo.Manutencao);

        imovel.MarcarAlugado();

        Assert.Equal(StatusAtivo.Alugado, imovel.Status);
    }

    [Fact]
    public void MarcarVago_altera_o_status_para_Vago()
    {
        var imovel = ImovelValido();
        imovel.MarcarAlugado();

        imovel.MarcarVago();

        Assert.Equal(StatusAtivo.Vago, imovel.Status);
    }

    [Fact]
    public void Excluir_marca_ExcluidoEm_e_Excluido_fica_true()
    {
        var imovel = ImovelValido();

        imovel.Excluir();

        Assert.True(imovel.Excluido);
        Assert.NotNull(imovel.ExcluidoEm);
    }
}
