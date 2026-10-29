namespace RecepcionCelulares.Api.Domain;

/// <summary>Entidades que pertenecen a un negocio (multi-negocio): cada cuenta solo ve sus datos.</summary>
public interface IPerteneceANegocio
{
    int NegocioId { get; set; }
}

public class Negocio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Ruc { get; set; }

    /// <summary>Último número de orden emitido. Solo crece: un número nunca se repite.</summary>
    public int UltimoCorrelativo { get; set; }
}

public enum Rol { Admin, Tecnico }

public class Usuario : IPerteneceANegocio
{
    public int Id { get; set; }
    public int NegocioId { get; set; }
    public Negocio? Negocio { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Rol Rol { get; set; }
}

public class Cliente : IPerteneceANegocio
{
    public int Id { get; set; }
    public int NegocioId { get; set; }
    public string Nombres { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Telefono { get; set; } = "";
}

public enum TipoRepuesto { Pantalla, Bateria, Placa, Otro }

public class Repuesto : IPerteneceANegocio
{
    public int Id { get; set; }
    public int NegocioId { get; set; }
    public string Nombre { get; set; } = "";
    public TipoRepuesto Tipo { get; set; }
    public int Stock { get; set; }
    public decimal CostoUnitario { get; set; }
}
