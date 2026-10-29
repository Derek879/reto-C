using MesaAyuda.Application.Tickets;

namespace MesaAyuda.Api.Infraestructura;

/// <summary>Proceso en segundo plano que cada minuto marca los tickets que vencieron su SLA.</summary>
public class MonitorSla(IServiceScopeFactory scopes, ILogger<MonitorSla> logger, IConfiguration config) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromSeconds(config.GetValue("Sla:IntervaloSegundos", 60));
        using var timer = new PeriodicTimer(intervalo);
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var marcados = await scope.ServiceProvider.GetRequiredService<TicketService>().VerificarSlaAsync(stoppingToken);
                if (marcados > 0)
                    logger.LogWarning("{Cantidad} ticket(s) vencieron su SLA", marcados);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falló la verificación de SLA");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
