using MesaAyuda.Domain;

namespace MesaAyuda.Application.Tickets;

public record CrearTicketDto(string Titulo, string Descripcion, Categoria Categoria, Prioridad Prioridad, string Solicitante);

public record AsignarDto(string Tecnico);

public record PrioridadDto(Prioridad Prioridad);

public record TextoDto(string Texto);

public record ComentarioDto(string Autor, string Texto);

public record FiltroTickets(
    EstadoTicket? Estado = null,
    Prioridad? Prioridad = null,
    Categoria? Categoria = null,
    string? AsignadoA = null,
    bool? SoloVencidos = null,
    string? Buscar = null,
    int Pagina = 1,
    int TamanoPagina = 20);

public record ComentarioRespuesta(string Autor, string Texto, DateTime Fecha);

public record TicketResumen(
    int Id, string Codigo, string Titulo, Categoria Categoria, Prioridad Prioridad, EstadoTicket Estado,
    string Solicitante, string? AsignadoA, DateTime FechaCreacion, DateTime FechaLimite, bool SlaIncumplido);

public record TicketDetalle(
    int Id, string Codigo, string Titulo, string Descripcion, Categoria Categoria, Prioridad Prioridad, EstadoTicket Estado,
    string Solicitante, string? AsignadoA, DateTime FechaCreacion, DateTime FechaLimite, DateTime? FechaResolucion,
    string? Solucion, bool SlaIncumplido, IReadOnlyList<ComentarioRespuesta> Comentarios);

public record Pagina<T>(IReadOnlyList<T> Items, int Total, int NumeroPagina, int TamanoPagina);

public record ConteoPor(string Clave, int Cantidad);

public record Indicadores(
    int Activos,
    int VencidosActivos,
    int ResueltosUltimos30Dias,
    double CumplimientoSla,
    double HorasPromedioResolucion,
    IReadOnlyList<ConteoPor> ActivosPorPrioridad,
    IReadOnlyList<ConteoPor> ActivosPorCategoria,
    IReadOnlyList<ConteoPor> ActivosPorTecnico);

internal static class Mapeos
{
    public static TicketResumen AResumen(this Ticket t) => new(
        t.Id, t.Codigo, t.Titulo, t.Categoria, t.Prioridad, t.Estado, t.Solicitante, t.AsignadoA,
        t.FechaCreacion, t.FechaLimite, t.SlaIncumplido);

    public static TicketDetalle ADetalle(this Ticket t) => new(
        t.Id, t.Codigo, t.Titulo, t.Descripcion, t.Categoria, t.Prioridad, t.Estado, t.Solicitante, t.AsignadoA,
        t.FechaCreacion, t.FechaLimite, t.FechaResolucion, t.Solucion, t.SlaIncumplido,
        t.Comentarios.OrderBy(c => c.Fecha).Select(c => new ComentarioRespuesta(c.Autor, c.Texto, c.Fecha)).ToList());
}
