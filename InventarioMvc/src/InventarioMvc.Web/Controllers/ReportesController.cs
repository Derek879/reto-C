using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventarioMvc.Web.Controllers;

public class ReportesController(ReporteRepositorio reportes, TimeProvider reloj) : Controller
{
    public async Task<IActionResult> Index(int? anio, DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
        var modelo = new ReportesViewModel
        {
            Anio = anio ?? hoy.Year,
            Desde = desde ?? new DateOnly(hoy.Year, hoy.Month, 1),
            Hasta = hasta ?? hoy,
        };
        modelo.PorMes = (await reportes.VentasPorMesAsync(modelo.Anio)).ToList();
        modelo.Top = (await reportes.TopProductosAsync(modelo.Desde, modelo.Hasta)).ToList();
        return View(modelo);
    }
}
