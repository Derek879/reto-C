using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Services;

namespace RecepcionCelulares.Api.Infrastructure;

/// <summary>Convierte las excepciones del negocio en respuestas ProblemDetails (RFC 7807) con el código HTTP correcto.</summary>
public class ManejadorErrores(ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            ReglaNegocioException => (StatusCodes.Status400BadRequest, "Regla de negocio"),
            NoEncontradoException => (StatusCodes.Status404NotFound, "No encontrado"),
            CredencialesInvalidasException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno"),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado");

        http.Response.StatusCode = status;
        await http.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = status == StatusCodes.Status500InternalServerError ? "Ocurrió un error inesperado." : ex.Message,
        }, ct);
        return true;
    }
}
