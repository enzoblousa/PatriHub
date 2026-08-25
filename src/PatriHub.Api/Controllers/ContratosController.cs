using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Application.Contratos;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/contratos")]
[Authorize]
public sealed class ContratosController(IContratoService contratoService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ContratoRequest request)
    {
        var resultado = await contratoService.CriarAsync(User.ObterUsuarioId(), request);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(ObterDetalhe), new { id = resultado.Dado!.Id }, resultado.Dado)
            : ErroParaResposta(resultado.Erro, resultado.TipoErro);
    }

    [HttpPost("{id:guid}/encerrar")]
    public async Task<IActionResult> Encerrar(Guid id)
    {
        var resultado = await contratoService.EncerrarAsync(User.ObterUsuarioId(), id);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterDetalhe(Guid id)
    {
        var resultado = await contratoService.ObterDetalheAsync(User.ObterUsuarioId(), id);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var contratos = await contratoService.ListarAsync(User.ObterUsuarioId());
        return Ok(contratos);
    }

    private IActionResult ResultadoParaResposta(ResultadoOperacao<ContratoDto> resultado) =>
        resultado.Sucesso ? Ok(resultado.Dado) : ErroParaResposta(resultado.Erro, resultado.TipoErro);

    /// <summary>Mapeamento único de erro de domínio para status HTTP, reaproveitado por toda ação deste controller.</summary>
    private IActionResult ErroParaResposta(string? erro, TipoErroOperacao? tipoErro) =>
        tipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro })
            : BadRequest(new { erro });
}
