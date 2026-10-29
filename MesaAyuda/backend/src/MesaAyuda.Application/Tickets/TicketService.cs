using FluentValidation;
using MesaAyuda.Domain;

namespace MesaAyuda.Application.Tickets;

public class NoEncontradoException(string mensaje) : Exception(mensaje);

/// <summary>Casos de uso de la mesa de ayuda. Orquesta: valida, carga el ticket, aplica la regla de dominio y guarda.</summary>
public class TicketService(
    ITicketRepositorio repositorio,
    IValidator<CrearTicketDto> validadorCrear,
    IValidator<ComentarioDto> validadorComentario,
    TimeProvider reloj)
{
    private DateTime Ahora => reloj.GetUtcNow().UtcDateTime;

    public async Task<TicketDetalle> CrearAsync(CrearTicketDto dto, CancellationToken ct = default)
    {
        await validadorCrear.ValidateAndThrowAsync(dto, ct);
        var ticket = new Ticket(dto.Titulo, dto.Descripcion, dto.Categoria, dto.Prioridad, dto.Solicitante, Ahora);
        repositorio.Agregar(ticket);
        await repositorio.GuardarCambiosAsync(ct);
        return ticket.ADetalle();
    }

    public async Task<Pagina<TicketResumen>> BuscarAsync(FiltroTickets filtro, CancellationToken ct = default)
    {
        var pagina = await repositorio.BuscarAsync(filtro, ct);
        return new Pagina<TicketResumen>(pagina.Items.Select(t => t.AResumen()).ToList(), pagina.Total, pagina.NumeroPagina, pagina.TamanoPagina);
    }

    public async Task<TicketDetalle> ObtenerAsync(int id, CancellationToken ct = default) =>
        (await CargarAsync(id, ct)).ADetalle();

    public Task<TicketDetalle> AsignarAsync(int id, AsignarDto dto, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.Asignar(dto.Tecnico, Ahora), ct);

    public Task<TicketDetalle> CambiarPrioridadAsync(int id, PrioridadDto dto, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.CambiarPrioridad(dto.Prioridad, Ahora), ct);

    public Task<TicketDetalle> PonerEnEsperaAsync(int id, TextoDto dto, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.PonerEnEspera(dto.Texto, Ahora), ct);

    public Task<TicketDetalle> RetomarAsync(int id, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.Retomar(Ahora), ct);

    public Task<TicketDetalle> ResolverAsync(int id, TextoDto dto, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.Resolver(dto.Texto, Ahora), ct);

    public Task<TicketDetalle> ReabrirAsync(int id, TextoDto dto, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.Reabrir(dto.Texto, Ahora), ct);

    public Task<TicketDetalle> CerrarAsync(int id, CancellationToken ct = default) =>
        AplicarAsync(id, t => t.Cerrar(Ahora), ct);

    public async Task<TicketDetalle> ComentarAsync(int id, ComentarioDto dto, CancellationToken ct = default)
    {
        await validadorComentario.ValidateAndThrowAsync(dto, ct);
        return await AplicarAsync(id, t => t.Comentar(dto.Autor, dto.Texto, Ahora), ct);
    }

    /// <summary>Marca como incumplidos los tickets activos que pasaron su fecha límite. Lo ejecuta un proceso en segundo plano.</summary>
    public async Task<int> VerificarSlaAsync(CancellationToken ct = default)
    {
        var ahora = Ahora;
        var vencidos = await repositorio.ActivosConSlaVencidoAsync(ahora, ct);
        var marcados = vencidos.Count(t => t.VerificarSla(ahora));
        if (marcados > 0)
            await repositorio.GuardarCambiosAsync(ct);
        return marcados;
    }

    public Indicadores Indicadores()
    {
        var ahora = Ahora;
        var activos = repositorio.Consulta()
            .Where(t => t.Estado != EstadoTicket.Resuelto && t.Estado != EstadoTicket.Cerrado)
            .Select(t => new { t.Prioridad, t.Categoria, t.AsignadoA, t.FechaLimite })
            .ToList();

        var desde = ahora.AddDays(-30);
        var resueltos = repositorio.Consulta()
            .Where(t => t.FechaResolucion != null && t.FechaResolucion >= desde)
            .Select(t => new { t.FechaCreacion, FechaResolucion = t.FechaResolucion!.Value, t.SlaIncumplido })
            .ToList();

        static List<ConteoPor> Contar<T>(IEnumerable<T> items) =>
            items.GroupBy(x => x?.ToString() ?? "Sin asignar")
                 .Select(g => new ConteoPor(g.Key, g.Count()))
                 .OrderByDescending(c => c.Cantidad).ToList();

        return new Indicadores(
            Activos: activos.Count,
            VencidosActivos: activos.Count(t => t.FechaLimite < ahora),
            ResueltosUltimos30Dias: resueltos.Count,
            CumplimientoSla: resueltos.Count == 0 ? 100 : Math.Round(100.0 * resueltos.Count(r => !r.SlaIncumplido) / resueltos.Count, 1),
            HorasPromedioResolucion: resueltos.Count == 0 ? 0 : Math.Round(resueltos.Average(r => (r.FechaResolucion - r.FechaCreacion).TotalHours), 1),
            ActivosPorPrioridad: Contar(activos.Select(t => t.Prioridad)),
            ActivosPorCategoria: Contar(activos.Select(t => t.Categoria)),
            ActivosPorTecnico: Contar(activos.Select(t => t.AsignadoA)));
    }

    private async Task<Ticket> CargarAsync(int id, CancellationToken ct) =>
        await repositorio.ObtenerAsync(id, ct) ?? throw new NoEncontradoException($"No existe el ticket {id}.");

    private async Task<TicketDetalle> AplicarAsync(int id, Action<Ticket> accion, CancellationToken ct)
    {
        var ticket = await CargarAsync(id, ct);
        accion(ticket);
        await repositorio.GuardarCambiosAsync(ct);
        return ticket.ADetalle();
    }
}
