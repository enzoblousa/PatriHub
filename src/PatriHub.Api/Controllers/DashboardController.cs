using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatriHub.Api.Autenticacao;
using PatriHub.Application.Dashboard;

namespace PatriHub.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>
    /// `taxaReferenciaAnual` é opcional: quando informada (ex.: 0.12 para 12% a.a.), cada Ativo
    /// retorna o custo de oportunidade calculado; omitida, o campo vem null (ver 01-SPEC-FUNCIONAL.md §5).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Obter([FromQuery] decimal? taxaReferenciaAnual)
    {
        var dashboard = await dashboardService.ObterDashboardAsync(User.ObterUsuarioId(), taxaReferenciaAnual);
        return Ok(dashboard);
    }
}
