using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Ativos;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Application.Contratos;
using PatriHub.Application.Lancamentos;
using PatriHub.Application.Locatarios;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Identity;

namespace PatriHub.Infrastructure.Seed;

/// <summary>
/// Massa de dados de demonstração — 3 usuários `User` com perfis de uso bem diferentes, pra dar
/// o que ver no backoffice do Admin (leitura auditada, ver ADR-0002) sem precisar cadastrar nada
/// manualmente. Só roda quando `SeedDadosDemo: true` está configurado (não em qualquer ambiente
/// por padrão — mesmo espírito opt-in do `AdminBootstrap`, ver <see cref="IdentitySeeder"/>) e é
/// idempotente por usuário: se o email já existe, pula a criação da conta e de todo o resto dos
/// dados dela (assume que já foi seedada numa execução anterior).
///
/// Passa pelos mesmos serviços de Application que a API usa (<see cref="IAtivoService"/> etc.),
/// nunca constrói entidade de domínio direto — a massa fica exatamente como se cada usuário
/// tivesse cadastrado tudo pela própria UI.
/// </summary>
public static class DadosDemoSeeder
{
    private const string SenhaPadrao = "Demo123!";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("SeedDadosDemo"))
        {
            return;
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var autenticacao = services.GetRequiredService<IAutenticacaoService>();
        var ativoService = services.GetRequiredService<IAtivoService>();
        var locatarioService = services.GetRequiredService<ILocatarioService>();
        var contratoService = services.GetRequiredService<IContratoService>();
        var lancamentoService = services.GetRequiredService<ILancamentoService>();

        var hoje = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        await SeedProprietariaDeVariosImoveisAsync(userManager, autenticacao, ativoService, locatarioService, contratoService, lancamentoService, hoje);
        await SeedMistoComInadimplenciaAsync(userManager, autenticacao, ativoService, locatarioService, contratoService, lancamentoService, hoje);
        await SeedIniciandoAsync(userManager, autenticacao, ativoService, lancamentoService, hoje);
    }

    /// <summary>
    /// Usuária 1 — dona de vários imóveis, uso "saudável": dois alugados em dia (com histórico
    /// de 3 meses de aluguel recebido) e um vago recém-anunciado. Mostra o caso comum de
    /// proprietário estabelecido.
    /// </summary>
    private static async Task SeedProprietariaDeVariosImoveisAsync(
        UserManager<ApplicationUser> userManager,
        IAutenticacaoService autenticacao,
        IAtivoService ativoService,
        ILocatarioService locatarioService,
        IContratoService contratoService,
        ILancamentoService lancamentoService,
        DateOnly hoje)
    {
        var usuarioId = await RegistrarUsuarioDemoAsync(userManager, autenticacao, "Marina Ferreira Dias", "marina.demo@patrihub.demo");
        if (usuarioId is null)
        {
            return;
        }

        var apeVilaMariana = ExigirDado(await ativoService.CriarImovelAsync(usuarioId.Value, new ImovelRequest(
            "Apê Vila Mariana",
            hoje.AddYears(-3),
            ValorAquisicao: 420_000m,
            ValorMercadoAtual: 480_000m,
            new EnderecoDto("Rua Vergueiro", "1420", "Apto 62", "Vila Mariana", "São Paulo", "SP", "04101-000"),
            TipoImovel.Apartamento,
            AreaM2: 65m,
            Matricula: "45210",
            ValorIptuMensal: 280m,
            ValorCondominioMensal: 650m,
            Financiamento: null)), "Apê Vila Mariana");

        var casaAlphaville = ExigirDado(await ativoService.CriarImovelAsync(usuarioId.Value, new ImovelRequest(
            "Casa Alphaville",
            hoje.AddYears(-5),
            ValorAquisicao: 950_000m,
            ValorMercadoAtual: 1_150_000m,
            new EnderecoDto("Alameda Rio Negro", "220", null, "Alphaville", "Barueri", "SP", "06454-000"),
            TipoImovel.Casa,
            AreaM2: 220m,
            Matricula: "78542",
            ValorIptuMensal: 520m,
            ValorCondominioMensal: 900m,
            Financiamento: new DadosFinanciamentoDto(4_200m, 180_000m, 9.5m, 96))), "Casa Alphaville");

        ExigirDado(await ativoService.CriarImovelAsync(usuarioId.Value, new ImovelRequest(
            "Loja Térreo Centro",
            hoje.AddYears(-1),
            ValorAquisicao: 310_000m,
            ValorMercadoAtual: 300_000m,
            new EnderecoDto("Rua Direita", "88", "Loja 3", "Centro", "São Paulo", "SP", "01002-000"),
            TipoImovel.Comercial,
            AreaM2: 45m,
            Matricula: "91023",
            ValorIptuMensal: 190m,
            ValorCondominioMensal: 0m,
            Financiamento: null)), "Loja Térreo Centro");

        var rafael = ExigirDado(await locatarioService.CriarAsync(usuarioId.Value,
            new LocatarioRequest("Rafael Nogueira", "11122233344", "(11) 98811-2233", "rafael.nogueira@example.com")), "Locatário Rafael");
        var beatriz = ExigirDado(await locatarioService.CriarAsync(usuarioId.Value,
            new LocatarioRequest("Beatriz Salles", "22233344455", "(11) 97722-3344", "beatriz.salles@example.com")), "Locatária Beatriz");

        var contratoApe = ExigirDado(await contratoService.CriarAsync(usuarioId.Value,
            new ContratoRequest(apeVilaMariana.Id, rafael.Id, ValorAluguelMensal: 2_800m, DiaVencimento: 1, hoje.AddMonths(-8), null)), "Contrato Apê Vila Mariana");
        var contratoCasa = ExigirDado(await contratoService.CriarAsync(usuarioId.Value,
            new ContratoRequest(casaAlphaville.Id, beatriz.Id, ValorAluguelMensal: 5_200m, DiaVencimento: 1, hoje.AddMonths(-20), null)), "Contrato Casa Alphaville");

        // Histórico de 3 meses de aluguel em dia + despesas fixas mensais — mostra o padrão de
        // um Ativo alugado saudável. DiaVencimento=1 do contrato garante que o mês de
        // competência avaliado pelo job de inadimplência é sempre o mês corrente na hora em que
        // o job roda (ver VerificacaoInadimplenciaService.VencimentoRelevante).
        foreach (var mesesAtras in new[] { 0, 1, 2 })
        {
            var dataAluguel = hoje.AddMonths(-mesesAtras);
            ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
                apeVilaMariana.Id, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 2_800m, dataAluguel, "Aluguel recebido", contratoApe.Id)), "Aluguel Apê");
            ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
                apeVilaMariana.Id, TipoLancamento.Despesa, CategoriaLancamento.Condominio, 650m, dataAluguel, "Condomínio", null)), "Condomínio Apê");

            ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
                casaAlphaville.Id, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 5_200m, dataAluguel, "Aluguel recebido", contratoCasa.Id)), "Aluguel Casa");
            ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
                casaAlphaville.Id, TipoLancamento.Despesa, CategoriaLancamento.Financiamento, 4_200m, dataAluguel, "Parcela do financiamento", null)), "Financiamento Casa");
        }
    }

    /// <summary>
    /// Usuária 2 — uso misto (carro + imóvel) com um Contrato inadimplente de verdade: o Kitnet
    /// Pinheiros tem vencimento todo dia 5 desde 5 meses atrás e nunca teve um Lançamento de
    /// aluguel — o <see cref="PatriHub.Application.Contratos.IVerificacaoInadimplenciaService"/>,
    /// que já roda como BackgroundService desde o startup da API, marca isso sozinho como
    /// `Inadimplente` na primeira checagem (não setamos o Status manualmente aqui). O carro tem
    /// um Contrato saudável, mesmo padrão de DiaVencimento=1 usado no outro usuário demo.
    /// </summary>
    private static async Task SeedMistoComInadimplenciaAsync(
        UserManager<ApplicationUser> userManager,
        IAutenticacaoService autenticacao,
        IAtivoService ativoService,
        ILocatarioService locatarioService,
        IContratoService contratoService,
        ILancamentoService lancamentoService,
        DateOnly hoje)
    {
        var usuarioId = await RegistrarUsuarioDemoAsync(userManager, autenticacao, "Carlos Eduardo Ramos", "carlos.demo@patrihub.demo");
        if (usuarioId is null)
        {
            return;
        }

        var civic = ExigirDado(await ativoService.CriarCarroAsync(usuarioId.Value, new CarroRequest(
            "Civic Touring 2021",
            hoje.AddYears(-2),
            ValorAquisicao: 145_000m,
            ValorMercadoAtual: 118_000m,
            Placa: "RAM2E21",
            Marca: "Honda",
            Modelo: "Civic Touring",
            AnoFabricacao: 2021,
            AnoModelo: 2021,
            ValorFipeAtual: 118_000m,
            Km: 42_000m,
            Motorizacao: Motorizacao.Combustao,
            ConsumoMedio: 12.5m,
            Financiamento: new DadosFinanciamentoDto(2_100m, 38_000m, 14.2m, 18))), "Civic Touring 2021");

        var kitnet = ExigirDado(await ativoService.CriarImovelAsync(usuarioId.Value, new ImovelRequest(
            "Kitnet Pinheiros",
            hoje.AddYears(-2),
            ValorAquisicao: 260_000m,
            ValorMercadoAtual: 295_000m,
            new EnderecoDto("Rua Cardeal Arcoverde", "1900", "Apto 12", "Pinheiros", "São Paulo", "SP", "05408-002"),
            TipoImovel.Apartamento,
            AreaM2: 28m,
            Matricula: "33810",
            ValorIptuMensal: 110m,
            ValorCondominioMensal: 380m,
            Financiamento: null)), "Kitnet Pinheiros");

        var juliana = ExigirDado(await locatarioService.CriarAsync(usuarioId.Value,
            new LocatarioRequest("Juliana Prado", "33344455566", "(11) 96633-4455", "juliana.prado@example.com")), "Locatária Juliana");
        var diego = ExigirDado(await locatarioService.CriarAsync(usuarioId.Value,
            new LocatarioRequest("Diego Martins", "44455566677", "(11) 95544-5566", "diego.martins@example.com")), "Locatário Diego");

        var contratoCarro = ExigirDado(await contratoService.CriarAsync(usuarioId.Value,
            new ContratoRequest(civic.Id, juliana.Id, ValorAluguelMensal: 1_800m, DiaVencimento: 1, hoje.AddMonths(-4), null)), "Contrato Civic");
        ExigirDado(await contratoService.CriarAsync(usuarioId.Value,
            new ContratoRequest(kitnet.Id, diego.Id, ValorAluguelMensal: 1_400m, DiaVencimento: 5, hoje.AddMonths(-5), null)), "Contrato Kitnet (inadimplente)");

        // Carro: alugado em dia, mas com um mês de manutenção que derruba o lucro daquele mês —
        // mostra que "alugado" não é sinônimo de lucro sempre positivo.
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            civic.Id, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_800m, hoje, "Aluguel recebido", contratoCarro.Id)), "Aluguel Civic");
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            civic.Id, TipoLancamento.Despesa, CategoriaLancamento.Ipva, 3_100m, hoje.AddDays(-12), "IPVA anual", null)), "IPVA Civic");
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            civic.Id, TipoLancamento.Despesa, CategoriaLancamento.Manutencao, 890m, hoje.AddDays(-20), "Revisão + pastilhas de freio", null)), "Manutenção Civic");

        // Kitnet: só despesas antigas, nenhum Lançamento de aluguel — é isso que deixa o
        // Contrato em carência sem pagamento e o job de inadimplência marca sozinho.
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            kitnet.Id, TipoLancamento.Despesa, CategoriaLancamento.Condominio, 380m, hoje.AddMonths(-2), "Condomínio", null)), "Condomínio Kitnet");
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            kitnet.Id, TipoLancamento.Despesa, CategoriaLancamento.Iptu, 110m, hoje.AddMonths(-2), "IPTU", null)), "IPTU Kitnet");
    }

    /// <summary>
    /// Usuária 3 — acabou de comprar o primeiro Ativo, ainda vago, financiado quase inteiro e
    /// sem nenhuma receita ainda. Mostra o caso de quem está começando a usar o PatriHub.
    /// </summary>
    private static async Task SeedIniciandoAsync(
        UserManager<ApplicationUser> userManager,
        IAutenticacaoService autenticacao,
        IAtivoService ativoService,
        ILancamentoService lancamentoService,
        DateOnly hoje)
    {
        var usuarioId = await RegistrarUsuarioDemoAsync(userManager, autenticacao, "Fernanda Lima Costa", "fernanda.demo@patrihub.demo");
        if (usuarioId is null)
        {
            return;
        }

        var studio = ExigirDado(await ativoService.CriarImovelAsync(usuarioId.Value, new ImovelRequest(
            "Studio Recém-comprado",
            hoje.AddMonths(-2),
            ValorAquisicao: 195_000m,
            ValorMercadoAtual: 195_000m,
            new EnderecoDto("Rua Augusta", "3050", "Apto 91", "Consolação", "São Paulo", "SP", "01412-100"),
            TipoImovel.Apartamento,
            AreaM2: 32m,
            Matricula: "60215",
            ValorIptuMensal: 95m,
            ValorCondominioMensal: 320m,
            Financiamento: new DadosFinanciamentoDto(1_450m, 175_000m, 10.8m, 348))), "Studio Recém-comprado");

        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            studio.Id, TipoLancamento.Despesa, CategoriaLancamento.Condominio, 320m, hoje.AddDays(-15), "Condomínio", null)), "Condomínio Studio");
        ExigirDado(await lancamentoService.CriarAsync(usuarioId.Value, new LancamentoRequest(
            studio.Id, TipoLancamento.Despesa, CategoriaLancamento.Iptu, 95m, hoje.AddDays(-15), "IPTU", null)), "IPTU Studio");
    }

    /// <summary>
    /// Registra a conta demo (`ConsentimentoLgpd: true` — não é um aceite real, é infraestrutura
    /// de seed, mesma ressalva de <see cref="IdentitySeeder.SeedAdminAsync"/> pro AdminBootstrap).
    /// Retorna `null` (em vez de lançar) quando o email já existe, pra quem chama pular o resto
    /// da massa daquele usuário — idempotente entre reinícios da API.
    /// </summary>
    private static async Task<Guid?> RegistrarUsuarioDemoAsync(
        UserManager<ApplicationUser> userManager,
        IAutenticacaoService autenticacao,
        string nome,
        string email)
    {
        var existente = await userManager.FindByEmailAsync(email);
        if (existente is not null)
        {
            return null;
        }

        var resultado = await autenticacao.RegistrarAsync(new RegistrarUsuarioRequest(nome, email, SenhaPadrao, ConsentimentoLgpd: true));
        if (!resultado.Sucesso)
        {
            throw new InvalidOperationException($"Falha ao seedar usuário demo ({email}): {resultado.Erro}");
        }

        return resultado.Usuario!.Id;
    }

    private static T ExigirDado<T>(ResultadoOperacao<T> resultado, string contexto)
    {
        if (!resultado.Sucesso)
        {
            throw new InvalidOperationException($"Falha ao seedar dados demo ({contexto}): {resultado.Erro}");
        }

        return resultado.Dado!;
    }
}
