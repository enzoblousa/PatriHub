using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Application.Locatarios;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/locatarios")]
[Authorize]
public sealed class LocatariosController(ILocatarioService locatarioService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] LocatarioRequest request)
    {
        var resultado = await locatarioService.CriarAsync(User.ObterUsuarioId(), request);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(ObterDetalhe), new { id = resultado.Dado!.Id }, resultado.Dado)
            : ErroParaResposta(resultado.Erro, resultado.TipoErro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LocatarioRequest request)
    {
        var resultado = await locatarioService.AtualizarAsync(User.ObterUsuarioId(), id, request);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterDetalhe(Guid id)
    {
        var resultado = await locatarioService.ObterDetalheAsync(User.ObterUsuarioId(), id);
        return ResultadoParaResposta(resultado);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var locatarios = await locatarioService.ListarAsync(User.ObterUsuarioId());
        return Ok(locatarios);
    }

    private IActionResult ResultadoParaResposta(ResultadoOperacao<LocatarioDto> resultado) =>
        resultado.Sucesso ? Ok(resultado.Dado) : ErroParaResposta(resultado.Erro, resultado.TipoErro);

    /// <summary>Mapeamento único de erro de domínio para status HTTP, reaproveitado por toda ação deste controller.</summary>
    private IActionResult ErroParaResposta(string? erro, TipoErroOperacao? tipoErro) =>
        tipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro })
            : BadRequest(new { erro });
}
