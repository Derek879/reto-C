using System.ComponentModel.DataAnnotations;

namespace InventarioMvc.Web.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Producto
{
    public int Id { get; set; }

    [Required, StringLength(20), RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Usa mayúsculas, números o guiones.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = "";

    [Required, StringLength(120)]
    public string Nombre { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Elige una categoría.")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    public string? Categoria { get; set; }

    [Range(0, 100_000)]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, 100_000)]
    public int Stock { get; set; }

    [Range(0, 1_000)]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; } = 5;

    public bool Activo { get; set; } = true;

    public bool StockBajo => Stock <= StockMinimo;
}

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = "";
    public decimal Total { get; set; }
    public bool Anulada { get; set; }
    public int Unidades { get; set; }
    public List<VentaDetalle> Detalle { get; set; } = new();
}

public class VentaDetalle
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class NuevaVentaViewModel
{
    [Required(ErrorMessage = "Ingresa el nombre del cliente."), StringLength(120)]
    public string Cliente { get; set; } = "";

    public List<LineaVenta> Lineas { get; set; } = new();

    public IReadOnlyList<Producto> Productos { get; set; } = Array.Empty<Producto>();
}

public class LineaVenta
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public record VentasMes(int Mes, int Ventas, decimal Total);

public record TopProducto(string Codigo, string Nombre, int Unidades, decimal Total);

public class ReportesViewModel
{
    public int Anio { get; set; }
    public DateOnly Desde { get; set; }
    public DateOnly Hasta { get; set; }
    public IReadOnlyList<VentasMes> PorMes { get; set; } = Array.Empty<VentasMes>();
    public IReadOnlyList<TopProducto> Top { get; set; } = Array.Empty<TopProducto>();
}

public class InicioViewModel
{
    public decimal VentasHoy { get; set; }
    public decimal VentasMes { get; set; }
    public int CantidadVentasMes { get; set; }
    public IReadOnlyList<Producto> StockBajo { get; set; } = Array.Empty<Producto>();
    public IReadOnlyList<TopProducto> TopMes { get; set; } = Array.Empty<TopProducto>();
}
