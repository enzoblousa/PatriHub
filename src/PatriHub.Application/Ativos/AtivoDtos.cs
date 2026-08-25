using PatriHub.Domain.Entidades;

namespace PatriHub.Application.Ativos;

public sealed record EnderecoDto(string Rua, string Numero, string? Complemento, string Bairro, string Cidade, string Uf, string Cep);

public sealed record DadosFinanciamentoDto(decimal ValorParcela, decimal SaldoDevedor, decimal TaxaJurosAnual, int ParcelasRestantes);

/// <summary>Corpo de cadastro (POST) e edição (PUT) de um Imóvel — os mesmos campos são exigidos nos dois casos.</summary>
public sealed record ImovelRequest(
    string Apelido,
    DateOnly DataAquisicao,
    decimal ValorAquisicao,
    decimal ValorMercadoAtual,
    EnderecoDto Endereco,
    TipoImovel TipoImovel,
    decimal AreaM2,
    string Matricula,
    decimal ValorIptuMensal,
    decimal ValorCondominioMensal,
    DadosFinanciamentoDto? Financiamento);

/// <summary>Corpo de cadastro (POST) e edição (PUT) de um Carro — os mesmos campos são exigidos nos dois casos.</summary>
public sealed record CarroRequest(
    string Apelido,
    DateOnly DataAquisicao,
    decimal ValorAquisicao,
    decimal ValorMercadoAtual,
    string Placa,
    string Marca,
    string Modelo,
    int AnoFabricacao,
    int AnoModelo,
    decimal ValorFipeAtual,
    decimal Km,
    decimal ConsumoMedio,
    DadosFinanciamentoDto? Financiamento);

public sealed record MarcarStatusAtivoRequest(StatusAtivo Status);

/// <summary>Visão resumida para a listagem de Ativos (ver AC "lista seus Ativos com visão resumida").</summary>
public sealed record AtivoResumoDto(
    Guid Id,
    string Apelido,
    TipoAtivo Tipo,
    StatusAtivo Status,
    decimal ValorMercadoAtual,
    decimal LucroDoMes);

public sealed record ImovelDetalheDto(
    EnderecoDto Endereco,
    TipoImovel TipoImovel,
    decimal AreaM2,
    string Matricula,
    decimal ValorIptuMensal,
    decimal ValorCondominioMensal);

public sealed record CarroDetalheDto(
    string Placa,
    string Marca,
    string Modelo,
    int AnoFabricacao,
    int AnoModelo,
    decimal ValorFipeAtual,
    decimal Km,
    decimal ConsumoMedio);

/// <summary>
/// Detalhe completo de um Ativo. Exatamente um entre <see cref="Imovel"/> e <see cref="Carro"/>
/// vem preenchido, conforme <see cref="Tipo"/>.
/// </summary>
public sealed record AtivoDetalheDto(
    Guid Id,
    string Apelido,
    TipoAtivo Tipo,
    StatusAtivo Status,
    DateOnly DataAquisicao,
    decimal ValorAquisicao,
    decimal ValorMercadoAtual,
    bool Financiado,
    DadosFinanciamentoDto? Financiamento,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm,
    ImovelDetalheDto? Imovel,
    CarroDetalheDto? Carro);
