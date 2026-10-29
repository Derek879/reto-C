using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;
using RecepcionCelulares.Api.Services;

namespace RecepcionCelulares.Api.Controllers;

[ApiController]
[Authorize(Roles = nameof(Rol.Admin))]
[Route("api/reportes")]
public class ReportesController(ReporteService reportes) : ControllerBase
{
    /// <summary>Ganancias de las órdenes entregadas entre dos fechas (formato yyyy-MM-dd).</summary>
    [HttpGet("ganancias")]
    public Task<ReporteGanancias> Ganancias([FromQuery] DateOnly desde, [FromQuery] DateOnly hasta) =>
        reportes.GananciasAsync(desde, hasta);
}
