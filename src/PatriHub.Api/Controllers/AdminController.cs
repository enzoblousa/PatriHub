using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Admin;
using PatriHub.Application.Common;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.Controllers;

/// <summary>
/// Ferramentas de suporte do Admin: gestão de contas e leitura auditada de Ativos/Lançamentos
/// de qualquer usuário (ver ADR-0002). Sem endpoints de escrita sobre dado de outro usuário —
/// PUT/DELETE de Ativo/Lançamento continuam exclusivos do dono, via AtivosController/
/// LancamentosController (que filtram por `UsuarioId` do próprio token).
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("usuarios")]
    public async Task<IActionResult> ListarUsuarios()
    {
        var usuarios = await adminService.ListarUsuariosAsync(User.ObterUsuarioId());
        return Ok(usuarios);
    }

    [HttpPatch("usuarios/{usuarioId:guid}/status")]
    public async Task<IActionResult> AtualizarStatusUsuario(Guid usuarioId, [FromBody] AtualizarStatusUsuarioRequest request)
    {
        var resultado = await adminService.AtualizarStatusUsuarioAsync(User.ObterUsuarioId(), usuarioId, request.Ativo);
        return ResultadoParaResposta(resultado);
    }

    [HttpPost("usuarios/{usuarioId:guid}/resetar-senha")]
    public async Task<IActionResult> ResetarSenha(Guid usuarioId, [FromBody] ResetarSenhaRequest request)
    {
        var resultado = await adminService.ResetarSenhaAsync(User.ObterUsuarioId(), usuarioId, request.NovaSenha);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("usuarios/{usuarioId:guid}/ativos")]
    public async Task<IActionResult> ListarAtivosDoUsuario(Guid usuarioId)
    {
        var resultado = await adminService.ListarAtivosDoUsuarioAsync(User.ObterUsuarioId(), usuarioId);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("usuarios/{usuarioId:guid}/ativos/{ativoId:guid}")]
    public async Task<IActionResult> ObterAtivoDoUsuario(Guid usuarioId, Guid ativoId)
    {
        var resultado = await adminService.ObterAtivoDoUsuarioAsync(User.ObterUsuarioId(), usuarioId, ativoId);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("usuarios/{usuarioId:guid}/lancamentos")]
    public async Task<IActionResult> ListarLancamentosDoUsuario(
        Guid usuarioId,
        [FromQuery] Guid? ativoId,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] TipoLancamento? tipo)
    {
        var resultado = await adminService.ListarLancamentosDoUsuarioAsync(
            User.ObterUsuarioId(),
            usuarioId,
            new LancamentoFiltro(ativoId, dataInicio, dataFim, tipo));
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("usuarios/{usuarioId:guid}/lancamentos/{lancamentoId:guid}")]
    public async Task<IActionResult> ObterLancamentoDoUsuario(Guid usuarioId, Guid lancamentoId)
    {
        var resultado = await adminService.ObterLancamentoDoUsuarioAsync(User.ObterUsuarioId(), usuarioId, lancamentoId);
        return ResultadoParaResposta(resultado);
    }

    private IActionResult ResultadoParaResposta(ResultadoOperacao resultado) =>
        resultado.Sucesso ? NoContent() : ErroParaResposta(resultado.Erro, resultado.TipoErro);

    private IActionResult ResultadoParaResposta<T>(ResultadoOperacao<T> resultado) =>
        resultado.Sucesso ? Ok(resultado.Dado) : ErroParaResposta(resultado.Erro, resultado.TipoErro);

    /// <summary>Mapeamento único de erro de domínio para status HTTP, reaproveitado por toda ação deste controller.</summary>
    private IActionResult ErroParaResposta(string? erro, TipoErroOperacao? tipoErro) =>
        tipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro })
            : BadRequest(new { erro });
}
