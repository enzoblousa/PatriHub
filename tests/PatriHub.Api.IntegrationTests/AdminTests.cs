using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatriHub.Application.Admin;
using PatriHub.Application.Ativos;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Api.IntegrationTests;

/// <summary>
/// Cobre a issue #8 (Admin: leitura auditada + gestão de contas — ver ADR-0002). Toda
/// asserção de auditoria lê `AuditLogsAdmin` direto do DbContext (resolvido do container de
/// DI), pois não há endpoint de leitura de log — só a persistência importa aqui.
/// </summary>
public sealed class AdminTests(PatriHubApiFactory factory) : IClassFixture<PatriHubApiFactory>
{
    private static Task<Guid> CriarAtivoAsync(HttpClient client) => CenarioTestHelper.CriarAtivoAsync(client);

    private static Task CriarLancamentoAsync(HttpClient client, Guid ativoId) =>
        client.PostAsJsonAsync("/api/lancamentos", new LancamentoRequest(
            ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m, new DateOnly(2026, 3, 10), "Aluguel de março", null));

    private async Task<List<AuditLogAdmin>> LogsDoAlvoAsync(Guid usuarioAlvoId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PatriHubDbContext>();
        return await db.AuditLogsAdmin.Where(a => a.UsuarioAlvoId == usuarioAlvoId).ToListAsync();
    }

    [Fact]
    public async Task Rota_admin_sem_token_retorna_401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/admin/usuarios");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rota_admin_com_usuario_comum_retorna_403()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var response = await client.GetAsync("/api/admin/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ListarUsuarios_inclui_conta_recem_criada_como_ativa()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var (_, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync("Usuário Alvo");

        var usuarios = await admin.GetFromJsonAsync<List<UsuarioAdminDto>>("/api/admin/usuarios");

        var alvo = usuarios!.Single(u => u.Id == usuarioAlvoId);
        Assert.True(alvo.Ativo);
        Assert.Equal("User", alvo.Papel);
    }

    [Fact]
    public async Task ListarUsuarios_grava_uma_auditoria_por_usuario_retornado_mas_nao_para_o_proprio_admin()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        var (_, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();

        await admin.GetFromJsonAsync<List<UsuarioAdminDto>>("/api/admin/usuarios");

        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(adminId, log.AdminUsuarioId);
        Assert.Equal(RecursoAuditoria.Usuario, log.Recurso);
        Assert.Equal(usuarioAlvoId, log.RecursoId);
        Assert.Empty(await LogsDoAlvoAsync(adminId));
    }

    [Fact]
    public async Task AtualizarStatus_desativa_conta_e_bloqueia_login_subsequente()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var email = AutenticacaoTestHelper.EmailUnico();
        var clienteAlvo = factory.CreateClient();
        var registro = await clienteAlvo.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Usuário Alvo", email, AutenticacaoTestHelper.SenhaPadrao, ConsentimentoLgpd: true));
        var usuarioAlvoId = (await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>())!.Usuario!.Id;

        var response = await admin.PatchAsJsonAsync($"/api/admin/usuarios/{usuarioAlvoId}/status", new AtualizarStatusUsuarioRequest(Ativo: false));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var loginAposDesativar = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, AutenticacaoTestHelper.SenhaPadrao));
        Assert.Equal(HttpStatusCode.Unauthorized, loginAposDesativar.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_reativa_conta_e_permite_login_novamente()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var email = AutenticacaoTestHelper.EmailUnico();
        var clienteAlvo = factory.CreateClient();
        var registro = await clienteAlvo.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Usuário Alvo", email, AutenticacaoTestHelper.SenhaPadrao, ConsentimentoLgpd: true));
        var usuarioAlvoId = (await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>())!.Usuario!.Id;
        await admin.PatchAsJsonAsync($"/api/admin/usuarios/{usuarioAlvoId}/status", new AtualizarStatusUsuarioRequest(Ativo: false));

        await admin.PatchAsJsonAsync($"/api/admin/usuarios/{usuarioAlvoId}/status", new AtualizarStatusUsuarioRequest(Ativo: true));

        var loginAposReativar = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, AutenticacaoTestHelper.SenhaPadrao));
        Assert.Equal(HttpStatusCode.OK, loginAposReativar.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_da_propria_conta_retorna_400()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();

        var response = await admin.PatchAsJsonAsync($"/api/admin/usuarios/{adminId}/status", new AtualizarStatusUsuarioRequest(Ativo: false));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_de_usuario_inexistente_retorna_404()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();

        var response = await admin.PatchAsJsonAsync($"/api/admin/usuarios/{Guid.NewGuid()}/status", new AtualizarStatusUsuarioRequest(Ativo: false));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarStatus_grava_auditoria_com_recurso_Usuario()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        var (_, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();

        await admin.PatchAsJsonAsync($"/api/admin/usuarios/{usuarioAlvoId}/status", new AtualizarStatusUsuarioRequest(Ativo: false));

        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(adminId, log.AdminUsuarioId);
        Assert.Equal(RecursoAuditoria.Usuario, log.Recurso);
        Assert.Equal(usuarioAlvoId, log.RecursoId);
    }

    [Fact]
    public async Task ResetarSenha_permite_login_com_a_nova_senha_e_bloqueia_a_antiga()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var email = AutenticacaoTestHelper.EmailUnico();
        var clienteAlvo = factory.CreateClient();
        var registro = await clienteAlvo.PostAsJsonAsync("/api/auth/registrar", new RegistrarUsuarioRequest("Usuário Alvo", email, AutenticacaoTestHelper.SenhaPadrao, ConsentimentoLgpd: true));
        var usuarioAlvoId = (await registro.Content.ReadFromJsonAsync<ResultadoAutenticacao>())!.Usuario!.Id;
        const string novaSenha = "OutraSenhaForte456!";

        var response = await admin.PostAsJsonAsync($"/api/admin/usuarios/{usuarioAlvoId}/resetar-senha", new ResetarSenhaRequest(novaSenha));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var loginComSenhaAntiga = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, AutenticacaoTestHelper.SenhaPadrao));
        Assert.Equal(HttpStatusCode.Unauthorized, loginComSenhaAntiga.StatusCode);

        var loginComSenhaNova = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, novaSenha));
        Assert.Equal(HttpStatusCode.OK, loginComSenhaNova.StatusCode);
    }

    /// <summary>
    /// Diferente de AtualizarStatus (que bloqueia o Admin de mexer na própria conta), resetar a
    /// própria senha continua permitido: não existe endpoint de autoatendimento pra trocar senha
    /// nesta issue, então bloquear aqui deixaria o Admin sem nenhuma forma de trocá-la
    /// (ver AdminService.ResetarSenhaAsync).
    /// </summary>
    [Fact]
    public async Task ResetarSenha_da_propria_conta_e_permitido()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();

        var response = await admin.PostAsJsonAsync($"/api/admin/usuarios/{adminId}/resetar-senha", new ResetarSenhaRequest("OutraSenhaForte456!"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ListarAtivosDoUsuario_retorna_ativos_do_usuario_alvo_e_grava_auditoria()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        var (clienteAlvo, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(clienteAlvo);

        var ativos = await admin.GetFromJsonAsync<List<AtivoResumoDto>>($"/api/admin/usuarios/{usuarioAlvoId}/ativos");

        var resumo = Assert.Single(ativos!);
        Assert.Equal(ativoId, resumo.Id);

        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(adminId, log.AdminUsuarioId);
        Assert.Equal(RecursoAuditoria.Ativos, log.Recurso);
        Assert.Null(log.RecursoId);
    }

    [Fact]
    public async Task ObterAtivoDoUsuario_retorna_detalhe_e_grava_auditoria_com_o_id_do_ativo()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        var (clienteAlvo, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(clienteAlvo);

        var response = await admin.GetAsync($"/api/admin/usuarios/{usuarioAlvoId}/ativos/{ativoId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(RecursoAuditoria.Ativos, log.Recurso);
        Assert.Equal(ativoId, log.RecursoId);
    }

    [Fact]
    public async Task ListarAtivosDoUsuario_de_usuario_inexistente_retorna_404_sem_gravar_auditoria()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var usuarioInexistente = Guid.NewGuid();

        var response = await admin.GetAsync($"/api/admin/usuarios/{usuarioInexistente}/ativos");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await LogsDoAlvoAsync(usuarioInexistente));
    }

    [Fact]
    public async Task ListarLancamentosDoUsuario_retorna_lancamentos_do_usuario_alvo_e_grava_auditoria()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        var (clienteAlvo, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(clienteAlvo);
        await CriarLancamentoAsync(clienteAlvo, ativoId);

        var lancamentos = await admin.GetFromJsonAsync<List<LancamentoDto>>($"/api/admin/usuarios/{usuarioAlvoId}/lancamentos");

        var lancamento = Assert.Single(lancamentos!);
        Assert.Equal(ativoId, lancamento.AtivoId);

        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(adminId, log.AdminUsuarioId);
        Assert.Equal(RecursoAuditoria.Lancamentos, log.Recurso);
        Assert.Null(log.RecursoId);
    }

    [Fact]
    public async Task ObterLancamentoDoUsuario_retorna_detalhe_e_grava_auditoria_com_o_id_do_lancamento()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var (clienteAlvo, usuarioAlvoId) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(clienteAlvo);
        var criado = await clienteAlvo.PostAsJsonAsync("/api/lancamentos", new LancamentoRequest(
            ativoId, TipoLancamento.Receita, CategoriaLancamento.Aluguel, 1_500m, new DateOnly(2026, 3, 10), null, null));
        var lancamentoId = (await criado.Content.ReadFromJsonAsync<LancamentoDto>())!.Id;

        var response = await admin.GetAsync($"/api/admin/usuarios/{usuarioAlvoId}/lancamentos/{lancamentoId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var log = Assert.Single(await LogsDoAlvoAsync(usuarioAlvoId));
        Assert.Equal(RecursoAuditoria.Lancamentos, log.Recurso);
        Assert.Equal(lancamentoId, log.RecursoId);
    }

    [Fact]
    public async Task Admin_nao_consegue_editar_ativo_de_outro_usuario_via_endpoint_do_dono()
    {
        var admin = await factory.CriarClienteAdminAutenticadoAsync();
        var (clienteAlvo, _) = await factory.CriarClienteAutenticadoComIdAsync();
        var ativoId = await CriarAtivoAsync(clienteAlvo);

        // AtivosController filtra sempre pelo UsuarioId do próprio token — o Admin não tem um
        // endpoint de escrita alternativo, então essa chamada só enxerga os (zero) Ativos dele
        // mesmo, nunca o do usuário-alvo (ver AtivosController/AtivoService).
        var response = await admin.PutAsJsonAsync($"/api/ativos/imoveis/{ativoId}", CenarioTestHelper.ImovelValido());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_acessando_os_proprios_ativos_pela_rota_de_admin_nao_grava_auditoria()
    {
        var (admin, adminId) = await factory.CriarClienteAdminAutenticadoComIdAsync();
        await CriarAtivoAsync(admin);

        var response = await admin.GetAsync($"/api/admin/usuarios/{adminId}/ativos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await LogsDoAlvoAsync(adminId));
    }
}
