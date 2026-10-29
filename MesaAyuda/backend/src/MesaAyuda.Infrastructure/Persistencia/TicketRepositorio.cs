using MesaAyuda.Application.Tickets;
using MesaAyuda.Domain;
using Microsoft.EntityFrameworkCore;

namespace MesaAyuda.Infrastructure.Persistencia;

public class TicketRepositorio(MesaAyudaDbContext db) : ITicketRepositorio
{
    public Task<Ticket?> ObtenerAsync(int id, CancellationToken ct = default) =>
        db.Tickets.Include(t => t.Comentarios).SingleOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Pagina<Ticket>> BuscarAsync(FiltroTickets f, CancellationToken ct = default)
    {
        var numero = Math.Max(1, f.Pagina);
        var tamano = Math.Clamp(f.TamanoPagina, 1, 100);

        var q = db.Tickets.AsNoTracking();
        if (f.Estado is not null) q = q.Where(t => t.Estado == f.Estado);
        if (f.Prioridad is not null) q = q.Where(t => t.Prioridad == f.Prioridad);
        if (f.Categoria is not null) q = q.Where(t => t.Categoria == f.Categoria);
        if (!string.IsNullOrWhiteSpace(f.AsignadoA)) q = q.Where(t => t.AsignadoA == f.AsignadoA);
        if (f.SoloVencidos == true) q = q.Where(t => t.SlaIncumplido);
        if (!string.IsNullOrWhiteSpace(f.Buscar))
            q = q.Where(t => t.Titulo.Contains(f.Buscar) || t.Solicitante.Contains(f.Buscar));

        var total = await q.CountAsync(ct);
        // Primero lo urgente: activos, por prioridad y por fecha límite más cercana
        var items = await q
            .OrderBy(t => t.Estado == EstadoTicket.Resuelto || t.Estado == EstadoTicket.Cerrado)
            // La prioridad se guarda como texto, así que se ordena por su nivel y no alfabéticamente
            .ThenBy(t => t.Prioridad == Prioridad.Critica ? 0 : t.Prioridad == Prioridad.Alta ? 1 : t.Prioridad == Prioridad.Media ? 2 : 3)
            .ThenBy(t => t.FechaLimite)
            .Skip((numero - 1) * tamano).Take(tamano)
            .ToListAsync(ct);

        return new Pagina<Ticket>(items, total, numero, tamano);
    }

    public async Task<IReadOnlyList<Ticket>> ActivosConSlaVencidoAsync(DateTime ahora, CancellationToken ct = default) =>
        await db.Tickets
            .Where(t => !t.SlaIncumplido && t.FechaLimite < ahora
                        && t.Estado != EstadoTicket.Resuelto && t.Estado != EstadoTicket.Cerrado)
            .ToListAsync(ct);

    public IQueryable<Ticket> Consulta() => db.Tickets.AsNoTracking();

    public void Agregar(Ticket ticket) => db.Tickets.Add(ticket);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
