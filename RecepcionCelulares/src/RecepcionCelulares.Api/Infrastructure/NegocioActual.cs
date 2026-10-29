using System.Security.Claims;

namespace RecepcionCelulares.Api.Infrastructure;

public interface INegocioActual
{
    int? NegocioId { get; }
}

/// <summary>Lee el negocio del usuario autenticado desde el claim "negocio_id" del JWT.</summary>
public class NegocioActualDesdeToken(IHttpContextAccessor accessor) : INegocioActual
{
    public const string Claim = "negocio_id";

    public int? NegocioId =>
        int.TryParse(accessor.HttpContext?.User.FindFirstValue(Claim), out var id) ? id : null;
}
