using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Autenticacao;
using PatriHub.Application.Common;
using PatriHub.Infrastructure.Jwt;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    [HttpPost("registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request)
    {
        var resultado = await autenticacaoService.RegistrarAsync(request);
        if (!resultado.Sucesso)
        {
            return Conflict(new { erro = resultado.Erro });
        }

        return Created(string.Empty, resultado);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await autenticacaoService.LoginAsync(request);
        if (!resultado.Sucesso)
        {
            return Unauthorized(new { erro = resultado.Erro });
        }

        return Ok(resultado);
    }

    /// <summary>"Esqueci minha senha" — ver ADR-0009. Sempre 200 quando o email existe, 404 quando não (Q3: decisão consciente de revelar isso, ver ADR-0009).</summary>
    [HttpPost("esqueci-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    public async Task<IActionResult> EsqueciSenha([FromBody] SolicitarRecuperacaoSenhaRequest request)
    {
        var resultado = await autenticacaoService.SolicitarRecuperacaoSenhaAsync(request);
        if (resultado.Sucesso)
        {
            return Ok();
        }

        return resultado.TipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro = resultado.Erro })
            : BadRequest(new { erro = resultado.Erro });
    }

    /// <summary>Conclui a recuperação de senha a partir do link do email — ver ADR-0009.</summary>
    [HttpPost("redefinir-senha")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
    {
        var resultado = await autenticacaoService.RedefinirSenhaAsync(request);
        if (resultado.Sucesso)
        {
            return Ok();
        }

        return resultado.TipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro = resultado.Erro })
            : BadRequest(new { erro = resultado.Erro });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var nome = User.FindFirstValue(PatriHubClaimTypes.Nome);
        var papel = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new UsuarioDto(User.ObterUsuarioId(), nome ?? string.Empty, email ?? string.Empty, papel ?? string.Empty));
    }

    /// <summary>
    /// Exclusão definitiva da própria conta e dados (LGPD — ver ADR-0005). `usuarioId` vem só
    /// do JWT (nunca de um parâmetro na rota), mesmo padrão de <see cref="Me"/> — ninguém
    /// exclui a conta de outra pessoa por aqui.
    /// </summary>
    [HttpDelete("conta")]
    [Authorize]
    public async Task<IActionResult> ExcluirConta()
    {
        var resultado = await autenticacaoService.ExcluirContaAsync(User.ObterUsuarioId());
        if (resultado.Sucesso)
        {
            return NoContent();
        }

        return resultado.TipoErro == TipoErroOperacao.NaoEncontrado
            ? NotFound(new { erro = resultado.Erro })
            : BadRequest(new { erro = resultado.Erro });
    }
}
