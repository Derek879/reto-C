using MesaAyuda.Application.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace MesaAyuda.Api.Controllers;

[ApiController]
[Route("api/indicadores")]
public class IndicadoresController(TicketService tickets) : ControllerBase
{
    /// <summary>Tickets activos, vencidos, cumplimiento de SLA y tiempo promedio de resolución.</summary>
    [HttpGet]
    public Indicadores Obtener() => tickets.Indicadores();
}
