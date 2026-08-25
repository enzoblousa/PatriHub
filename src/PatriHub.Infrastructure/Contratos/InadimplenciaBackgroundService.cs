using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PatriHub.Application.Contratos;

namespace PatriHub.Infrastructure.Contratos;

/// <summary>
/// Dispara <see cref="IVerificacaoInadimplenciaService.VerificarAsync"/> uma vez por dia,
/// in-process, dentro do próprio processo da API (ADR-0003) — roda a primeira checagem
/// imediatamente ao subir a API, depois a cada 24h. Cria um escopo de DI por execução porque o
/// serviço injetado depende do <c>DbContext</c> (escopado) e este `BackgroundService` é singleton.
/// </summary>
public sealed class InadimplenciaBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<InadimplenciaBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);
        do
        {
            await ChecarInadimplenciaAsync(stoppingToken);
        }
        while (await AguardarProximoTickAsync(timer, stoppingToken));
    }

    private async Task ChecarInadimplenciaAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var verificacao = scope.ServiceProvider.GetRequiredService<IVerificacaoInadimplenciaService>();
            await verificacao.VerificarAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Uma falha nesta execução não deve derrubar o BackgroundService — a próxima
            // execução, no dia seguinte, tenta de novo.
            logger.LogError(ex, "Falha ao executar a checagem diária de inadimplência.");
        }
    }

    private static async Task<bool> AguardarProximoTickAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
