using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InventarioMvc.Web.Controllers;

public class ProductosController(ProductoRepositorio productos) : Controller
{
    public async Task<IActionResult> Index(string? buscar, int? categoriaId)
    {
        ViewBag.Buscar = buscar;
        ViewBag.Categorias = new SelectList(await productos.CategoriasAsync(), "Id", "Nombre", categoriaId);
        return View(await productos.ListarAsync(buscar, categoriaId));
    }

    public async Task<IActionResult> Crear()
    {
        await CargarCategoriasAsync();
        return View("Formulario", new Producto());
    }

    public async Task<IActionResult> Editar(int id)
    {
        var producto = await productos.ObtenerAsync(id);
        if (producto is null) return NotFound();
        await CargarCategoriasAsync();
        return View("Formulario", producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(Producto producto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await productos.GuardarAsync(producto);
                TempData["Mensaje"] = $"Producto {producto.Codigo} guardado.";
                return RedirectToAction(nameof(Index));
            }
            catch (ErrorDeNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        await CargarCategoriasAsync();
        return View("Formulario", producto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        await productos.DesactivarAsync(id);
        TempData["Mensaje"] = "Producto desactivado. Su historial de ventas se conserva.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Exportar()
    {
        var archivo = ExportadorExcel.Inventario(await productos.ListarAsync());
        return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"inventario-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private async Task CargarCategoriasAsync() =>
        ViewBag.Categorias = new SelectList(await productos.CategoriasAsync(), "Id", "Nombre");
}
