namespace RecepcionCelulares.Api.Domain;

/// <summary>Se viola una regla del negocio (stock insuficiente, cambio de estado no permitido...). Responde 400.</summary>
public class ReglaNegocioException(string mensaje) : Exception(mensaje);

/// <summary>El recurso no existe o pertenece a otro negocio. Responde 404.</summary>
public class NoEncontradoException(string mensaje) : Exception(mensaje);
