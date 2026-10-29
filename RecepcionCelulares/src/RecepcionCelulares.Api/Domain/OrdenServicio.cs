namespace RecepcionCelulares.Api.Domain;

public enum EstadoOrden { Recibido, EnReparacion, Listo, Entregado, Anulado }

public class OrdenServicio : IPerteneceANegocio
{
    public int Id { get; set; }
    public int NegocioId { get; set; }

    /// <summary>Número correlativo por negocio (001, 002...). Se asigna al crear y no se reutiliza.</summary>
    public int Numero { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string? Imei { get; set; }
    public string Falla { get; set; } = "";

    /// <summary>El cliente acepta el riesgo (p. ej. equipo mojado o con golpes) antes de la reparación.</summary>
    public bool DeclaracionRiesgoAceptada { get; set; }

    public decimal PrecioServicio { get; set; }
    public decimal Adelanto { get; set; }

    public EstadoOrden Estado { get; set; } = EstadoOrden.Recibido;
    public DateTime FechaRecepcion { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string? MotivoAnulacion { get; set; }

    public List<OrdenRepuesto> Repuestos { get; set; } = new();

    public decimal Saldo => PrecioServicio - Adelanto;
    public decimal CostoRepuestos => Repuestos.Sum(r => r.CostoUnitario * r.Cantidad);

    private static readonly Dictionary<EstadoOrden, EstadoOrden[]> Transiciones = new()
    {
        [EstadoOrden.Recibido] = new[] { EstadoOrden.EnReparacion, EstadoOrden.Anulado },
        [EstadoOrden.EnReparacion] = new[] { EstadoOrden.Listo, EstadoOrden.Anulado },
        [EstadoOrden.Listo] = new[] { EstadoOrden.Entregado, EstadoOrden.EnReparacion },
        [EstadoOrden.Entregado] = Array.Empty<EstadoOrden>(),
        [EstadoOrden.Anulado] = Array.Empty<EstadoOrden>(),
    };

    public bool PuedeCambiarA(EstadoOrden nuevo) => Transiciones[Estado].Contains(nuevo);

    public bool AdmiteRepuestos => Estado is EstadoOrden.Recibido or EstadoOrden.EnReparacion;
}

/// <summary>Repuesto usado en una orden. Guarda el costo del momento para que los reportes no cambien si sube el precio.</summary>
public class OrdenRepuesto
{
    public int Id { get; set; }
    public int OrdenServicioId { get; set; }
    public int RepuestoId { get; set; }
    public Repuesto? Repuesto { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
}
