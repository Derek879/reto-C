using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Dtos;
using RecepcionCelulares.Api.Infrastructure;

namespace RecepcionCelulares.Api.Services;

public class JwtOptions
{
    public string Clave { get; set; } = "";
    public string Emisor { get; set; } = "RecepcionCelulares";
    public int HorasDeValidez { get; set; } = 8;
}

public class AuthService(AppDbContext db, IOptions<JwtOptions> jwt, TimeProvider reloj)
{
    /// <summary>Crea un negocio nuevo con su usuario administrador.</summary>
    public async Task<TokenResponse> RegistrarAsync(RegistroRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == email))
            throw new ReglaNegocioException("Ya existe una cuenta con ese correo.");

        var negocio = new Negocio { Nombre = req.NombreNegocio.Trim(), Ruc = req.Ruc };
        var admin = new Usuario
        {
            Negocio = negocio,
            Nombre = req.NombreAdmin.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Rol = Rol.Admin,
        };
        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();
        return GenerarToken(admin);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.IgnoreQueryFilters().SingleOrDefaultAsync(u => u.Email == email);

        // Mismo mensaje si no existe el correo o si la contraseña es incorrecta: no revelamos qué cuentas existen.
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
            throw new CredencialesInvalidasException();

        return GenerarToken(usuario);
    }

    private TokenResponse GenerarToken(Usuario usuario)
    {
        var expira = reloj.GetUtcNow().UtcDateTime.AddHours(jwt.Value.HorasDeValidez);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim(NegocioActualDesdeToken.Claim, usuario.NegocioId.ToString()),
        };
        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Clave)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Value.Emisor, jwt.Value.Emisor, claims, expires: expira, signingCredentials: credenciales);

        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expira, usuario.Nombre, usuario.Rol.ToString());
    }
}

public class CredencialesInvalidasException() : Exception("Correo o contraseña incorrectos.");
