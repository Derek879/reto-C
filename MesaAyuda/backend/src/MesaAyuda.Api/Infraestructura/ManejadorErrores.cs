using FluentValidation;
using MesaAyuda.Application.Tickets;
using MesaAyuda.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MesaAyuda.Api.Infraestructura;

/// <summary>Traduce las excepciones de las capas internas a respuestas HTTP con ProblemDetails.</summary>
public class ManejadorErrores(ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        ProblemDetails problema = ex switch
        {
            ValidationException v => new ValidationProblemDetails(
                v.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Datos inválidos",
            },
            DomainException => new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Operación no permitida", Detail = ex.Message },
            NoEncontradoException => new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "No encontrado", Detail = ex.Message },
            _ => new ProblemDetails { Status = StatusCodes.Status500InternalServerError, Title = "Error interno" },
        };

        if (problema.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado");

        http.Response.StatusCode = problema.Status!.Value;
        await http.Response.WriteAsJsonAsync(problema, problema.GetType(), cancellationToken: ct);
        return true;
    }
}
