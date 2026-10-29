namespace MesaAyuda.Domain;

/// <summary>Una operación viola una regla del negocio de la mesa de ayuda.</summary>
public class DomainException(string mensaje) : Exception(mensaje);
