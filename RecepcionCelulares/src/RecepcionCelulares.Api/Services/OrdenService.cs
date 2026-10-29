using Microsoft.EntityFrameworkCore;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;
using RecepcionCelulares.Api.Infrastructure;

namespace RecepcionCelulares.Api.Services;

public class OrdenService(AppDbContext db, INegocioActual negocioActual, TimeProvider reloj)
{
    public async Task<OrdenResponse> CrearAsync(CrearOrdenRequest req)
    {
        if (req.Adelanto > req.PrecioServicio)
            throw new ReglaNegocioException("El adelanto no puede ser mayor que el precio del servicio.");

        var cliente = await db.Clientes.FindAsync(req.ClienteId)
            ?? throw new NoEncontradoException($"No existe el cliente {req.ClienteId}.");

        await using var tx = await db.Database.BeginTransactionAsync();

        // Incremento atómico en la base de datos: la fila del negocio queda bloqueada hasta el commit,
        // así dos recepciones simultáneas nunca obtienen el mismo número.
        var negocioId = negocioActual.NegocioId!.Value;
        await db.Negocios.Where(n => n.Id == negocioId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.UltimoCorrelativo, n => n.UltimoCorrelativo + 1));
        var numero = await db.Negocios.Where(n => n.Id == negocioId).Select(n => n.UltimoCorrelativo).SingleAsync();

        var orden = new OrdenServicio
        {
            Numero = numero,
            ClienteId = cliente.Id,
            Marca = req.Marca.Trim(),
            Modelo = req.Modelo.Trim(),
            Imei = req.Imei,
            Falla = req.Falla.Trim(),
            DeclaracionRiesgoAceptada = req.DeclaracionRiesgoAceptada,
            PrecioServicio = req.PrecioServicio,
            Adelanto = req.Adelanto,
            FechaRecepcion = reloj.GetUtcNow().UtcDateTime,
        };
        db.Ordenes.Add(orden);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return await ObtenerAsync(orden.Id);
    }

    public async Task<PaginaResponse<OrdenResponse>> ListarAsync(EstadoOrden? estado, string? buscar, int pagina, int tamano)
    {
        pagina = Math.Max(1, pagina);
        tamano = Math.Clamp(tamano, 1, 100);

        var query = ConDetalle();
        if (estado is not null)
            query = query.Where(o => o.Estado == estado);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var texto = buscar.Trim();
            query = query.Where(o => o.Cliente!.Nombres.Contains(texto) || o.Cliente.Dni == texto
                                     || o.Modelo.Contains(texto) || o.Imei == texto);
        }

        var total = await query.CountAsync();
        var ordenes = await query.OrderByDescending(o => o.Numero)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

        return new PaginaResponse<OrdenResponse>(ordenes.Select(Mapear).ToList(), total, pagina, tamano);
    }

    public async Task<OrdenResponse> ObtenerAsync(int id) => Mapear(await BuscarAsync(id));

    public async Task<OrdenResponse> CambiarEstadoAsync(int id, EstadoOrden nuevo)
    {
        if (nuevo == EstadoOrden.Anulado)
            throw new ReglaNegocioException("Para anular una orden usa POST /api/ordenes/{id}/anular indicando el motivo.");

        var orden = await BuscarAsync(id);
        if (!orden.PuedeCambiarA(nuevo))
            throw new ReglaNegocioException($"No se puede pasar de {orden.Estado} a {nuevo}.");

        orden.Estado = nuevo;
        if (nuevo == EstadoOrden.Entregado)
            orden.FechaEntrega = reloj.GetUtcNow().UtcDateTime;

        await db.SaveChangesAsync();
        return Mapear(orden);
    }

    public async Task<OrdenResponse> AgregarRepuestoAsync(int id, AgregarRepuestoRequest req)
    {
        var orden = await BuscarAsync(id);
        if (!orden.AdmiteRepuestos)
            throw new ReglaNegocioException($"No se pueden agregar repuestos a una orden en estado {orden.Estado}.");

        var repuesto = await db.Repuestos.FindAsync(req.RepuestoId)
            ?? throw new NoEncontradoException($"No existe el repuesto {req.RepuestoId}.");
        if (repuesto.Stock < req.Cantidad)
            throw new ReglaNegocioException($"Stock insuficiente de {repuesto.Nombre}: quedan {repuesto.Stock}.");

        repuesto.Stock -= req.Cantidad;
        orden.Repuestos.Add(new OrdenRepuesto
        {
            RepuestoId = repuesto.Id,
            Repuesto = repuesto,
            Cantidad = req.Cantidad,
            CostoUnitario = repuesto.CostoUnitario,
        });
        if (orden.Estado == EstadoOrden.Recibido)
            orden.Estado = EstadoOrden.EnReparacion;

        await db.SaveChangesAsync();
        return Mapear(orden);
    }

    /// <summary>Las órdenes no se borran: se anulan con motivo y se devuelve el stock usado. El número queda reservado.</summary>
    public async Task<OrdenResponse> AnularAsync(int id, string motivo)
    {
        var orden = await BuscarAsync(id);
        if (!orden.PuedeCambiarA(EstadoOrden.Anulado))
            throw new ReglaNegocioException($"No se puede anular una orden en estado {orden.Estado}.");

        foreach (var usado in orden.Repuestos)
            usado.Repuesto!.Stock += usado.Cantidad;

        orden.Estado = EstadoOrden.Anulado;
        orden.MotivoAnulacion = motivo.Trim();
        await db.SaveChangesAsync();
        return Mapear(orden);
    }

    private IQueryable<OrdenServicio> ConDetalle() =>
        db.Ordenes.Include(o => o.Cliente).Include(o => o.Repuestos).ThenInclude(r => r.Repuesto);

    private async Task<OrdenServicio> BuscarAsync(int id) =>
        await ConDetalle().SingleOrDefaultAsync(o => o.Id == id)
        ?? throw new NoEncontradoException($"No existe la orden {id}.");

    private static OrdenResponse Mapear(OrdenServicio o) => new(
        o.Id,
        o.Numero.ToString("D6"),
        new ClienteResponse(o.Cliente!.Id, o.Cliente.Nombres, o.Cliente.Dni, o.Cliente.Telefono),
        o.Marca, o.Modelo, o.Imei, o.Falla, o.DeclaracionRiesgoAceptada, o.Estado,
        o.PrecioServicio, o.Adelanto, o.Saldo, o.FechaRecepcion, o.FechaEntrega, o.MotivoAnulacion,
        o.Repuestos.Select(r => new OrdenRepuestoResponse(r.RepuestoId, r.Repuesto?.Nombre ?? "", r.Cantidad, r.CostoUnitario)).ToList(),
        "Esta orden de servicio no es un comprobante de pago SUNAT.");
}
