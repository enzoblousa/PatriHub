using System.Net.Http.Json;
using PatriHub.Application.Ativos;
using PatriHub.Application.Locatarios;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.IntegrationTests;

/// <summary>
/// Builders de Ativo/Locatário compartilhados por qualquer teste que precise de um Contrato
/// válido como pré-condição (ContratosTests, InadimplenciaTests) — evita duplicar o mesmo cenário
/// em cada arquivo.
/// </summary>
public static class CenarioTestHelper
{
    public static EnderecoDto EnderecoValido() =>
        new("Rua das Flores", "123", null, "Centro", "São Paulo", "SP", "01000-000");

    public static ImovelRequest ImovelValido(string apelido = "Apê Centro") => new(
        apelido,
        new DateOnly(2020, 1, 10),
        ValorAquisicao: 300_000m,
        ValorMercadoAtual: 350_000m,
        EnderecoValido(),
        TipoImovel.Apartamento,
        AreaM2: 65m,
        Matricula: "12345",
        ValorIptuMensal: 150m,
        ValorCondominioMensal: 400m,
        Financiamento: null);

    public static LocatarioRequest LocatarioValido(string nome = "João Souza") =>
        new(nome, "123.456.789-09", "(11) 99999-0000", "joao@example.com");

    public static async Task<Guid> CriarAtivoAsync(HttpClient client, string apelido = "Apê Centro")
    {
        var criado = await (await client.PostAsJsonAsync("/api/ativos/imoveis", ImovelValido(apelido))).Content.ReadFromJsonAsync<AtivoDetalheDto>();
        return criado!.Id;
    }

    public static async Task<Guid> CriarLocatarioAsync(HttpClient client, string nome = "João Souza")
    {
        var criado = await (await client.PostAsJsonAsync("/api/locatarios", LocatarioValido(nome))).Content.ReadFromJsonAsync<LocatarioDto>();
        return criado!.Id;
    }
}
