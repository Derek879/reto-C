using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RecepcionCelulares.Api.Data;
using RecepcionCelulares.Api.Infrastructure;
using RecepcionCelulares.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<INegocioActual, NegocioActualDesdeToken>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrdenService>();
builder.Services.AddScoped<ReporteService>();

builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
    throw new InvalidOperationException("Configura Jwt:Clave con al menos 32 caracteres (user-secrets o variable de entorno Jwt__Clave).");
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Emisor,
        ValidAudience = jwt.Emisor,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Recepción de Celulares API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega el token que devuelve /api/auth/login",
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>(),
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:MigrarAlIniciar"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

app.Run();

public partial class Program;
