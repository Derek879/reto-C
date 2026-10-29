using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventarioMvc.Web.Controllers;

public class VentasController(VentaRepositorio ventas, ProductoRepositorio productos, TimeProvider reloj) : Controller
{
    public async Task<IActionResult> Index(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
        var d = desde ?? hoy.AddDays(-30);
        var h = hasta ?? hoy;
        ViewBag.Desde = d.ToString("yyyy-MM-dd");
        ViewBag.Hasta = h.ToString("yyyy-MM-dd");
        return View(await ventas.ListarAsync(d, h));
    }

    public async Task<IActionResult> Nueva() =>
        View(new NuevaVentaViewModel { Productos = (await productos.ListarAsync()).ToList() });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Nueva(NuevaVentaViewModel modelo)
    {
        var lineas = modelo.Lineas.Where(l => l.ProductoId > 0 && l.Cantidad > 0).ToList();
        if (lineas.Count == 0)
            ModelState.AddModelError(string.Empty, "Agrega al menos un producto con cantidad mayor a cero.");

        if (ModelState.IsValid)
        {
            try
            {
                var id = await ventas.RegistrarAsync(modelo.Cliente, lineas);
                TempData["Mensaje"] = $"Venta N° {id} registrada.";
                return RedirectToAction(nameof(Detalle), new { id });
            }
            catch (ErrorDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        modelo.Productos = (await productos.ListarAsync()).ToList();
        return View(modelo);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var venta = await ventas.ObtenerAsync(id);
        return venta is null ? NotFound() : View(venta);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        try
        {
            await ventas.AnularAsync(id);
            TempData["Mensaje"] = $"Venta N° {id} anulada. El stock fue devuelto.";
        }
        catch (ErrorDeNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
