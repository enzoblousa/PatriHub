using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Autenticacao;
using PatriHub.Infrastructure.Jwt;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    [HttpPost("registrar")]
    [AllowAnonymous]
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var resultado = await autenticacaoService.LoginAsync(request);
        if (!resultado.Sucesso)
        {
            return Unauthorized(new { erro = resultado.Erro });
        }

        return Ok(resultado);
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
}
