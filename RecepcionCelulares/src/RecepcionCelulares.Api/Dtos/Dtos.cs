using System.ComponentModel.DataAnnotations;
using RecepcionCelulares.Api.Domain;

namespace RecepcionCelulares.Api.Dtos;

public record RegistroRequest(
    [Required, StringLength(120)] string NombreNegocio,
    [StringLength(11, MinimumLength = 11)] string? Ruc,
    [Required, StringLength(120)] string NombreAdmin,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);

public record TokenResponse(string Token, DateTime ExpiraEn, string Nombre, string Rol);

public record ClienteRequest(
    [Required, StringLength(150)] string Nombres,
    [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos.")] string Dni,
    [Required, RegularExpression(@"^9\d{8}$", ErrorMessage = "El celular debe tener 9 dígitos y empezar con 9.")] string Telefono);

public record ClienteResponse(int Id, string Nombres, string Dni, string Telefono);

public record RepuestoRequest(
    [Required, StringLength(120)] string Nombre,
    TipoRepuesto Tipo,
    [Range(0, 10_000)] int Stock,
    [Range(0, 100_000)] decimal CostoUnitario);

public record RepuestoResponse(int Id, string Nombre, TipoRepuesto Tipo, int Stock, decimal CostoUnitario);

public record AjusteStockRequest([Range(-10_000, 10_000)] int Cantidad, [Required] string Motivo);

public record CrearOrdenRequest(
    int ClienteId,
    [Required, StringLength(50)] string Marca,
    [Required, StringLength(80)] string Modelo,
    [RegularExpression(@"^\d{15}$", ErrorMessage = "El IMEI debe tener 15 dígitos.")] string? Imei,
    [Required, StringLength(500)] string Falla,
    bool DeclaracionRiesgoAceptada,
    [Range(0, 100_000)] decimal PrecioServicio,
    [Range(0, 100_000)] decimal Adelanto);

public record CambiarEstadoRequest(EstadoOrden Estado);

public record AnularOrdenRequest([Required, StringLength(300, MinimumLength = 5)] string Motivo);

public record AgregarRepuestoRequest(int RepuestoId, [Range(1, 100)] int Cantidad);

public record OrdenRepuestoResponse(int RepuestoId, string Nombre, int Cantidad, decimal CostoUnitario);

public record OrdenResponse(
    int Id,
    string Numero,
    ClienteResponse Cliente,
    string Marca,
    string Modelo,
    string? Imei,
    string Falla,
    bool DeclaracionRiesgoAceptada,
    EstadoOrden Estado,
    decimal PrecioServicio,
    decimal Adelanto,
    decimal Saldo,
    DateTime FechaRecepcion,
    DateTime? FechaEntrega,
    string? MotivoAnulacion,
    IReadOnlyList<OrdenRepuestoResponse> Repuestos,
    string Aviso);

public record GananciaDia(DateOnly Fecha, int OrdenesEntregadas, decimal Ingresos, decimal CostoRepuestos, decimal Ganancia);

public record ReporteGanancias(
    DateOnly Desde,
    DateOnly Hasta,
    int OrdenesEntregadas,
    decimal Ingresos,
    decimal CostoRepuestos,
    decimal Ganancia,
    IReadOnlyList<GananciaDia> PorDia);

public record PaginaResponse<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina);
