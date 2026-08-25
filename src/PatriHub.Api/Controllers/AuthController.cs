using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Application.Autenticacao;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAutenticacaoService autenticacaoService) : ControllerBase
{
    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken ct)
    {
        var resultado = await autenticacaoService.RegistrarAsync(request, ct);
        if (!resultado.Sucesso)
        {
            return Conflict(new { erro = resultado.Erro });
        }

        return Created(string.Empty, resultado);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var resultado = await autenticacaoService.LoginAsync(request, ct);
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
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var nome = User.FindFirstValue("nome");
        var papel = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new UsuarioDto(Guid.Parse(id!), nome ?? string.Empty, email ?? string.Empty, papel ?? string.Empty));
    }
}
