using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;

namespace InventarioMvc.Tests;

public class ProcedimientosTests(BaseDeDatosFixture bd) : IClassFixture<BaseDeDatosFixture>
{
    private readonly ProductoRepositorio _productos = new(bd.Conexion);
    private readonly VentaRepositorio _ventas = new(bd.Conexion);
    private readonly ReporteRepositorio _reportes = new(bd.Conexion);

    private async Task<Producto> NuevoProductoAsync(int stock = 10, decimal precio = 20m)
    {
        var p = new Producto
        {
            Codigo = $"T-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            Nombre = "Producto de prueba",
            CategoriaId = 1,
            PrecioVenta = precio,
            Stock = stock,
            StockMinimo = 2,
        };
        p.Id = await _productos.GuardarAsync(p);
        return p;
    }

    [Fact]
    public async Task Guardar_crea_y_luego_actualiza_el_producto()
    {
        var p = await NuevoProductoAsync();
        p.Nombre = "Nombre editado";
        p.PrecioVenta = 35.5m;

        Assert.Equal(p.Id, await _productos.GuardarAsync(p));
        var guardado = await _productos.ObtenerAsync(p.Id);

        Assert.Equal("Nombre editado", guardado!.Nombre);
        Assert.Equal(35.5m, guardado.PrecioVenta);
        Assert.Equal("Accesorios", guardado.Categoria);
    }

    [Fact]
    public async Task No_permite_dos_productos_con_el_mismo_codigo()
    {
        var existente = await NuevoProductoAsync();
        var duplicado = new Producto { Codigo = existente.Codigo, Nombre = "Otro", CategoriaId = 1, PrecioVenta = 1, Stock = 1 };

        var ex = await Assert.ThrowsAsync<ErrorDeNegocioException>(() => _productos.GuardarAsync(duplicado));
        Assert.Contains("código", ex.Message);
    }

    [Fact]
    public async Task Venta_descuenta_stock_y_calcula_total_con_el_precio_actual()
    {
        var a = await NuevoProductoAsync(stock: 10, precio: 12.50m);
        var b = await NuevoProductoAsync(stock: 5, precio: 30m);

        // El mismo producto en dos líneas se agrupa en una sola
        var id = await _ventas.RegistrarAsync("Cliente prueba",
            [new LineaVenta { ProductoId = a.Id, Cantidad = 2 }, new LineaVenta { ProductoId = b.Id, Cantidad = 1 },
             new LineaVenta { ProductoId = a.Id, Cantidad = 1 }]);
        var venta = await _ventas.ObtenerAsync(id);

        Assert.Equal(67.50m, venta!.Total);
        Assert.Equal(2, venta.Detalle.Count);
        Assert.Equal(4, venta.Unidades);
        Assert.Equal(7, (await _productos.ObtenerAsync(a.Id))!.Stock);
        Assert.Equal(4, (await _productos.ObtenerAsync(b.Id))!.Stock);
    }

    [Fact]
    public async Task Venta_sin_stock_suficiente_no_guarda_nada()
    {
        var conStock = await NuevoProductoAsync(stock: 10);
        var sinStock = await NuevoProductoAsync(stock: 1);

        var ex = await Assert.ThrowsAsync<ErrorDeNegocioException>(() => _ventas.RegistrarAsync("Cliente",
            [new LineaVenta { ProductoId = conStock.Id, Cantidad = 3 }, new LineaVenta { ProductoId = sinStock.Id, Cantidad = 2 }]));

        Assert.StartsWith("Stock insuficiente", ex.Message);
        // La transacción se revirtió completa: el otro producto tampoco perdió stock
        Assert.Equal(10, (await _productos.ObtenerAsync(conStock.Id))!.Stock);
    }

    [Fact]
    public async Task No_vende_productos_desactivados()
    {
        var p = await NuevoProductoAsync();
        await _productos.DesactivarAsync(p.Id);

        await Assert.ThrowsAsync<ErrorDeNegocioException>(() =>
            _ventas.RegistrarAsync("Cliente", [new LineaVenta { ProductoId = p.Id, Cantidad = 1 }]));
    }

    [Fact]
    public async Task Anular_devuelve_el_stock_y_no_se_puede_anular_dos_veces()
    {
        var p = await NuevoProductoAsync(stock: 4);
        var id = await _ventas.RegistrarAsync("Cliente", [new LineaVenta { ProductoId = p.Id, Cantidad = 3 }]);

        await _ventas.AnularAsync(id);

        Assert.Equal(4, (await _productos.ObtenerAsync(p.Id))!.Stock);
        Assert.True((await _ventas.ObtenerAsync(id))!.Anulada);
        await Assert.ThrowsAsync<ErrorDeNegocioException>(() => _ventas.AnularAsync(id));
    }

    [Fact]
    public async Task Ventas_simultaneas_no_venden_mas_stock_del_que_hay()
    {
        var p = await NuevoProductoAsync(stock: 5);

        var intentos = Enumerable.Range(0, 10).Select(async _ =>
        {
            try
            {
                await _ventas.RegistrarAsync("Cliente", [new LineaVenta { ProductoId = p.Id, Cantidad = 1 }]);
                return true;
            }
            catch (ErrorDeNegocioException)
            {
                return false;
            }
        });
        var resultados = await Task.WhenAll(intentos);

        Assert.Equal(5, resultados.Count(ok => ok));
        Assert.Equal(0, (await _productos.ObtenerAsync(p.Id))!.Stock);
    }

    [Fact]
    public async Task Reportes_excluyen_ventas_anuladas()
    {
        var p = await NuevoProductoAsync(stock: 20, precio: 10m);
        await _ventas.RegistrarAsync("Cliente", [new LineaVenta { ProductoId = p.Id, Cantidad = 4 }]);
        var anulada = await _ventas.RegistrarAsync("Cliente", [new LineaVenta { ProductoId = p.Id, Cantidad = 6 }]);
        await _ventas.AnularAsync(anulada);

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var top = (await _reportes.TopProductosAsync(hoy.AddDays(-1), hoy.AddDays(1), 100)).Single(t => t.Codigo == p.Codigo);

        Assert.Equal(4, top.Unidades);
        Assert.Equal(40m, top.Total);
        Assert.Contains(await _reportes.VentasPorMesAsync(hoy.Year), m => m.Mes == hoy.Month && m.Total >= 40m);
    }
}
