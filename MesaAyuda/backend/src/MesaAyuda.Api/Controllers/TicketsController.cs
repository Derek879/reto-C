using MesaAyuda.Application.Tickets;
using Microsoft.AspNetCore.Mvc;

namespace MesaAyuda.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController(TicketService tickets) : ControllerBase
{
    /// <summary>Bandeja de tickets con filtros. Los activos más urgentes aparecen primero.</summary>
    [HttpGet]
    public Task<Pagina<TicketResumen>> Buscar([FromQuery] FiltroTickets filtro, CancellationToken ct) =>
        tickets.BuscarAsync(filtro, ct);

    [HttpGet("{id:int}")]
    public Task<TicketDetalle> Obtener(int id, CancellationToken ct) => tickets.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<TicketDetalle>> Crear(CrearTicketDto dto, CancellationToken ct)
    {
        var ticket = await tickets.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = ticket.Id }, ticket);
    }

    [HttpPost("{id:int}/asignar")]
    public Task<TicketDetalle> Asignar(int id, AsignarDto dto, CancellationToken ct) => tickets.AsignarAsync(id, dto, ct);

    [HttpPost("{id:int}/prioridad")]
    public Task<TicketDetalle> CambiarPrioridad(int id, PrioridadDto dto, CancellationToken ct) => tickets.CambiarPrioridadAsync(id, dto, ct);

    [HttpPost("{id:int}/en-espera")]
    public Task<TicketDetalle> PonerEnEspera(int id, TextoDto dto, CancellationToken ct) => tickets.PonerEnEsperaAsync(id, dto, ct);

    [HttpPost("{id:int}/retomar")]
    public Task<TicketDetalle> Retomar(int id, CancellationToken ct) => tickets.RetomarAsync(id, ct);

    [HttpPost("{id:int}/resolver")]
    public Task<TicketDetalle> Resolver(int id, TextoDto dto, CancellationToken ct) => tickets.ResolverAsync(id, dto, ct);

    [HttpPost("{id:int}/reabrir")]
    public Task<TicketDetalle> Reabrir(int id, TextoDto dto, CancellationToken ct) => tickets.ReabrirAsync(id, dto, ct);

    [HttpPost("{id:int}/cerrar")]
    public Task<TicketDetalle> Cerrar(int id, CancellationToken ct) => tickets.CerrarAsync(id, ct);

    [HttpPost("{id:int}/comentarios")]
    public Task<TicketDetalle> Comentar(int id, ComentarioDto dto, CancellationToken ct) => tickets.ComentarAsync(id, dto, ct);
}
