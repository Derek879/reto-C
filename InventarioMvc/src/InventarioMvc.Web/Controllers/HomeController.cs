using System.Diagnostics;
using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventarioMvc.Web.Controllers;

public class HomeController(VentaRepositorio ventas, ProductoRepositorio productos, ReporteRepositorio reportes, TimeProvider reloj) : Controller
{
    public async Task<IActionResult> Index()
    {
        var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
        var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);
        var ventasMes = (await ventas.ListarAsync(inicioMes, hoy)).Where(v => !v.Anulada).ToList();

        return View(new InicioViewModel
        {
            VentasHoy = ventasMes.Where(v => DateOnly.FromDateTime(v.Fecha) == hoy).Sum(v => v.Total),
            VentasMes = ventasMes.Sum(v => v.Total),
            CantidadVentasMes = ventasMes.Count,
            StockBajo = (await productos.StockBajoAsync()).ToList(),
            TopMes = (await reportes.TopProductosAsync(inicioMes, hoy, 5)).ToList(),
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
