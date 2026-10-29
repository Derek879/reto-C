using System.Text.Json.Serialization;
using MesaAyuda.Api.Infraestructura;
using MesaAyuda.Application;
using MesaAyuda.Infrastructure;
using MesaAyuda.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();

if (builder.Configuration.GetValue("Sla:MonitorActivo", true))
    builder.Services.AddHostedService<MonitorSla>();

var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Configuration.GetValue<bool>("Database:MigrarAlIniciar"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MesaAyudaDbContext>();
    db.Database.Migrate();
    if (app.Configuration.GetValue<bool>("Database:DatosDeEjemplo"))
        await DatosDeEjemplo.CargarAsync(db, DateTime.UtcNow);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { estado = "ok" }));

app.Run();

public partial class Program;
