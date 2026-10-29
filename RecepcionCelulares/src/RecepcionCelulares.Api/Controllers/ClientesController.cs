using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;

namespace RecepcionCelulares.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clientes")]
public class ClientesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ClienteResponse>> Listar([FromQuery] string? buscar)
    {
        var query = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(c => c.Nombres.Contains(buscar) || c.Dni == buscar || c.Telefono == buscar);

        return await query.OrderBy(c => c.Nombres)
            .Select(c => new ClienteResponse(c.Id, c.Nombres, c.Dni, c.Telefono))
            .Take(50).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ClienteResponse> Obtener(int id)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new NoEncontradoException($"No existe el cliente {id}.");
        return new ClienteResponse(c.Id, c.Nombres, c.Dni, c.Telefono);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Crear(ClienteRequest req)
    {
        if (await db.Clientes.AnyAsync(c => c.Dni == req.Dni))
            throw new ReglaNegocioException($"Ya existe un cliente con DNI {req.Dni}.");

        var c = new Cliente { Nombres = req.Nombres.Trim(), Dni = req.Dni, Telefono = req.Telefono };
        db.Clientes.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = c.Id }, new ClienteResponse(c.Id, c.Nombres, c.Dni, c.Telefono));
    }

    [HttpPut("{id:int}")]
    public async Task<ClienteResponse> Actualizar(int id, ClienteRequest req)
    {
        var c = await db.Clientes.FindAsync(id) ?? throw new NoEncontradoException($"No existe el cliente {id}.");
        if (await db.Clientes.AnyAsync(x => x.Dni == req.Dni && x.Id != id))
            throw new ReglaNegocioException($"Ya existe un cliente con DNI {req.Dni}.");

        c.Nombres = req.Nombres.Trim();
        c.Dni = req.Dni;
        c.Telefono = req.Telefono;
        await db.SaveChangesAsync();
        return new ClienteResponse(c.Id, c.Nombres, c.Dni, c.Telefono);
    }
}
