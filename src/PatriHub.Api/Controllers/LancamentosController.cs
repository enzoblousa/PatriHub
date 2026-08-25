using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Application.Lancamentos;
using PatriHub.Domain.Entidades;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/lancamentos")]
[Authorize]
public sealed class LancamentosController(ILancamentoService lancamentoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LancamentoRequest request)
    {
        var resultado = await lancamentoService.CriarAsync(User.ObterUsuarioId(), request);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(ObterDetalhe), new { id = resultado.Dado!.Id }, resultado.Dado)
            : ErroParaResposta(resultado.Erro, resultado.TipoErro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LancamentoRequest request)
    {
        var resultado = await lancamentoService.AtualizarAsync(User.ObterUsuarioId(), id, request);
        return ResultadoParaResposta(resultado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var resultado = await lancamentoService.ExcluirAsync(User.ObterUsuarioId(), id);
        return resultado.Sucesso ? NoContent() : ErroParaResposta(resultado.Erro, resultado.TipoErro);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterDetalhe(Guid id)
    {
        var resultado = await lancamentoService.ObterDetalheAsync(User.ObterUsuarioId(), id);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? ativoId,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] TipoLancamento? tipo)
    {
        var lancamentos = await lancamentoService.ListarAsync(
            User.ObterUsuarioId(),
            new LancamentoFiltro(ativoId, dataInicio, dataFim, tipo));
        return Ok(lancamentos);
    }

    private IActionResult ResultadoParaResposta(ResultadoOperacao<LancamentoDto> resultado) =>
        resultado.Sucesso ? Ok(resultado.Dado) : ErroParaResposta(resultado.Erro, resultado.TipoErro);

    /// <summary>Mapeamento único de erro de domínio para status HTTP, reaproveitado por toda ação deste controller.</summary>
    private IActionResult ErroParaResposta(string? erro, TipoErroOperacao? tipoErro) =>
        tipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro })
            : BadRequest(new { erro });
}
