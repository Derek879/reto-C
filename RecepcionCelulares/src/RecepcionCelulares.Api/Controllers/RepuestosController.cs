using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;

namespace RecepcionCelulares.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/repuestos")]
public class RepuestosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<RepuestoResponse>> Listar([FromQuery] TipoRepuesto? tipo, [FromQuery] bool soloStockBajo = false)
    {
        var query = db.Repuestos.AsNoTracking();
        if (tipo is not null) query = query.Where(r => r.Tipo == tipo);
        if (soloStockBajo) query = query.Where(r => r.Stock <= 2);

        return await query.OrderBy(r => r.Nombre)
            .Select(r => new RepuestoResponse(r.Id, r.Nombre, r.Tipo, r.Stock, r.CostoUnitario))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = nameof(Rol.Admin))]
    public async Task<ActionResult<RepuestoResponse>> Crear(RepuestoRequest req)
    {
        var r = new Repuesto { Nombre = req.Nombre.Trim(), Tipo = req.Tipo, Stock = req.Stock, CostoUnitario = req.CostoUnitario };
        db.Repuestos.Add(r);
        await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, new RepuestoResponse(r.Id, r.Nombre, r.Tipo, r.Stock, r.CostoUnitario));
    }

    /// <summary>Entrada (compra) o salida manual (merma) de stock.</summary>
    [HttpPost("{id:int}/ajuste")]
    [Authorize(Roles = nameof(Rol.Admin))]
    public async Task<RepuestoResponse> AjustarStock(int id, AjusteStockRequest req)
    {
        var r = await db.Repuestos.FindAsync(id) ?? throw new NoEncontradoException($"No existe el repuesto {id}.");
        if (r.Stock + req.Cantidad < 0)
            throw new ReglaNegocioException($"El stock no puede quedar negativo (actual: {r.Stock}).");

        r.Stock += req.Cantidad;
        await db.SaveChangesAsync();
        return new RepuestoResponse(r.Id, r.Nombre, r.Tipo, r.Stock, r.CostoUnitario);
    }
}
