using MesaAyuda.Domain;

namespace MesaAyuda.Application.Tickets;

/// <summary>Puerto de persistencia. La capa de aplicación no sabe si detrás hay SQL Server, SQLite o memoria.</summary>
public interface ITicketRepositorio
{
    Task<Ticket?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<Pagina<Ticket>> BuscarAsync(FiltroTickets filtro, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> ActivosConSlaVencidoAsync(DateTime ahora, CancellationToken ct = default);
    IQueryable<Ticket> Consulta();
    void Agregar(Ticket ticket);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
