using Microsoft.AspNetCore.Mvc;
using RecepcionCelulares.Api.Dtos;
using RecepcionCelulares.Api.Services;

namespace RecepcionCelulares.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>Registra un negocio nuevo y su usuario administrador.</summary>
    [HttpPost("registro")]
    public async Task<ActionResult<TokenResponse>> Registrar(RegistroRequest req) =>
        StatusCode(StatusCodes.Status201Created, await auth.RegistrarAsync(req));

    [HttpPost("login")]
    public Task<TokenResponse> Login(LoginRequest req) => auth.LoginAsync(req);
}
