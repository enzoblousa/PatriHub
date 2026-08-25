using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Ativos;
using PatriHub.Application.Common;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/ativos")]
[Authorize]
public sealed class AtivosController(IAtivoService ativoService) : ControllerBase
{
    [HttpPost("imoveis")]
    public async Task<IActionResult> CriarImovel([FromBody] CriarImovelRequest request)
    {
        var resultado = await ativoService.CriarImovelAsync(User.ObterUsuarioId(), request);
        if (!resultado.Sucesso)
        {
            return BadRequest(new { erro = resultado.Erro });
        }

        return CreatedAtAction(nameof(ObterDetalhe), new { id = resultado.Dado!.Id }, resultado.Dado);
    }

    [HttpPost("carros")]
    public async Task<IActionResult> CriarCarro([FromBody] CriarCarroRequest request)
    {
        var resultado = await ativoService.CriarCarroAsync(User.ObterUsuarioId(), request);
        if (!resultado.Sucesso)
        {
            return BadRequest(new { erro = resultado.Erro });
        }

        return CreatedAtAction(nameof(ObterDetalhe), new { id = resultado.Dado!.Id }, resultado.Dado);
    }

    [HttpPut("imoveis/{id:guid}")]
    public async Task<IActionResult> AtualizarImovel(Guid id, [FromBody] AtualizarImovelRequest request)
    {
        var resultado = await ativoService.AtualizarImovelAsync(User.ObterUsuarioId(), id, request);
        return ResultadoParaResposta(resultado);
    }

    [HttpPut("carros/{id:guid}")]
    public async Task<IActionResult> AtualizarCarro(Guid id, [FromBody] AtualizarCarroRequest request)
    {
        var resultado = await ativoService.AtualizarCarroAsync(User.ObterUsuarioId(), id, request);
        return ResultadoParaResposta(resultado);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> MarcarStatus(Guid id, [FromBody] MarcarStatusAtivoRequest request)
    {
        var resultado = await ativoService.MarcarStatusAsync(User.ObterUsuarioId(), id, request);
        return ResultadoParaResposta(resultado);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var resultado = await ativoService.ExcluirAsync(User.ObterUsuarioId(), id);
        if (!resultado.Sucesso)
        {
            return resultado.TipoErro == TipoErroOperacao.NaoEncontrado
                ? NotFound(new { erro = resultado.Erro })
                : BadRequest(new { erro = resultado.Erro });
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var ativos = await ativoService.ListarAsync(User.ObterUsuarioId());
        return Ok(ativos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterDetalhe(Guid id)
    {
        var resultado = await ativoService.ObterDetalheAsync(User.ObterUsuarioId(), id);
        return ResultadoParaResposta(resultado);
    }

    private IActionResult ResultadoParaResposta(ResultadoOperacao<AtivoDetalheDto> resultado)
    {
        if (resultado.Sucesso)
        {
            return Ok(resultado.Dado);
        }

        return resultado.TipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro = resultado.Erro })
            : BadRequest(new { erro = resultado.Erro });
    }
}
