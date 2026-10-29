using Microsoft.EntityFrameworkCore;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;

namespace RecepcionCelulares.Api.Services;

public class ReporteService(AppDbContext db)
{
    /// <summary>Ganancia = precio cobrado de las órdenes entregadas − costo de los repuestos que usaron.</summary>
    public async Task<ReporteGanancias> GananciasAsync(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
            throw new ReglaNegocioException("La fecha 'hasta' debe ser mayor o igual a 'desde'.");

        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var fin = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var entregadas = await db.Ordenes
            .Where(o => o.Estado == EstadoOrden.Entregado && o.FechaEntrega >= inicio && o.FechaEntrega < fin)
            .Select(o => new
            {
                Fecha = o.FechaEntrega!.Value.Date,
                o.PrecioServicio,
                Repuestos = o.Repuestos.Select(r => new { r.CostoUnitario, r.Cantidad }).ToList(),
            })
            .ToListAsync();

        // El costo se suma en memoria para que el cálculo con decimales sea exacto en cualquier motor de BD.
        var conCosto = entregadas.Select(o => new
        {
            o.Fecha,
            o.PrecioServicio,
            Costo = o.Repuestos.Sum(r => r.CostoUnitario * r.Cantidad),
        });

        var porDia = conCosto
            .GroupBy(o => DateOnly.FromDateTime(o.Fecha))
            .OrderBy(g => g.Key)
            .Select(g => new GananciaDia(g.Key, g.Count(), g.Sum(x => x.PrecioServicio), g.Sum(x => x.Costo),
                                         g.Sum(x => x.PrecioServicio - x.Costo)))
            .ToList();

        return new ReporteGanancias(desde, hasta,
            porDia.Sum(d => d.OrdenesEntregadas), porDia.Sum(d => d.Ingresos),
            porDia.Sum(d => d.CostoRepuestos), porDia.Sum(d => d.Ganancia), porDia);
    }
}
