using ClosedXML.Excel;
using InventarioMvc.Web.Models;

namespace InventarioMvc.Web.Data;

public static class ExportadorExcel
{
    public static byte[] Inventario(IEnumerable<Producto> productos)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Inventario");
        string[] cabeceras = ["Código", "Producto", "Categoría", "Precio", "Stock", "Stock mínimo", "Valorizado"];
        for (var i = 0; i < cabeceras.Length; i++)
            hoja.Cell(1, i + 1).Value = cabeceras[i];
        hoja.Row(1).Style.Font.Bold = true;

        var fila = 2;
        foreach (var p in productos)
        {
            hoja.Cell(fila, 1).Value = p.Codigo;
            hoja.Cell(fila, 2).Value = p.Nombre;
            hoja.Cell(fila, 3).Value = p.Categoria;
            hoja.Cell(fila, 4).Value = p.PrecioVenta;
            hoja.Cell(fila, 5).Value = p.Stock;
            hoja.Cell(fila, 6).Value = p.StockMinimo;
            hoja.Cell(fila, 7).FormulaA1 = $"D{fila}*E{fila}";
            if (p.StockBajo)
                hoja.Range(fila, 1, fila, 7).Style.Fill.BackgroundColor = XLColor.LightPink;
            fila++;
        }

        hoja.Range(2, 4, fila, 4).Style.NumberFormat.Format = "\"S/\" #,##0.00";
        hoja.Range(2, 7, fila, 7).Style.NumberFormat.Format = "\"S/\" #,##0.00";
        hoja.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        libro.SaveAs(ms);
        return ms.ToArray();
    }
}
