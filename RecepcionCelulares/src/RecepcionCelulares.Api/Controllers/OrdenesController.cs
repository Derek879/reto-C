using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;
using RecepcionCelulares.Api.Services;

namespace RecepcionCelulares.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ordenes")]
public class OrdenesController(OrdenService ordenes) : ControllerBase
{
    [HttpGet]
    public Task<PaginaResponse<OrdenResponse>> Listar(
        [FromQuery] EstadoOrden? estado, [FromQuery] string? buscar, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20) =>
        ordenes.ListarAsync(estado, buscar, pagina, tamano);

    [HttpGet("{id:int}")]
    public Task<OrdenResponse> Obtener(int id) => ordenes.ObtenerAsync(id);

    /// <summary>Recepciona un equipo y genera la orden con su número correlativo.</summary>
    [HttpPost]
    public async Task<ActionResult<OrdenResponse>> Crear(CrearOrdenRequest req)
    {
        var orden = await ordenes.CrearAsync(req);
        return CreatedAtAction(nameof(Obtener), new { id = orden.Id }, orden);
    }

    [HttpPatch("{id:int}/estado")]
    public Task<OrdenResponse> CambiarEstado(int id, CambiarEstadoRequest req) => ordenes.CambiarEstadoAsync(id, req.Estado);

    [HttpPost("{id:int}/repuestos")]
    public Task<OrdenResponse> AgregarRepuesto(int id, AgregarRepuestoRequest req) => ordenes.AgregarRepuestoAsync(id, req);

    /// <summary>Anula la orden (no se borra) y devuelve al stock los repuestos usados. Solo administradores.</summary>
    [HttpPost("{id:int}/anular")]
    [Authorize(Roles = nameof(Rol.Admin))]
    public Task<OrdenResponse> Anular(int id, AnularOrdenRequest req) => ordenes.AnularAsync(id, req.Motivo);
}
