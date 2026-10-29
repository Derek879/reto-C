using ClosedXML.Excel;
using InventarioMvc.Web.Data;
using InventarioMvc.Web.Models;

namespace InventarioMvc.Tests;

public class ExportadorExcelTests
{
    [Fact]
    public void Genera_una_fila_por_producto_con_el_valorizado()
    {
        var archivo = ExportadorExcel.Inventario([
            new Producto { Codigo = "A-1", Nombre = "Funda", Categoria = "Accesorios", PrecioVenta = 25m, Stock = 4, StockMinimo = 2 },
            new Producto { Codigo = "B-2", Nombre = "Cable", Categoria = "Cargadores", PrecioVenta = 10m, Stock = 1, StockMinimo = 5 },
        ]);

        using var libro = new XLWorkbook(new MemoryStream(archivo));
        var hoja = libro.Worksheet("Inventario");

        Assert.Equal("Código", hoja.Cell(1, 1).GetString());
        Assert.Equal("Funda", hoja.Cell(2, 2).GetString());
        Assert.Equal(100d, hoja.Cell(2, 7).GetDouble());
        Assert.Equal(10d, hoja.Cell(3, 7).GetDouble());
    }
}
