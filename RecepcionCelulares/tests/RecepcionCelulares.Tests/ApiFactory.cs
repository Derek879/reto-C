using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecepcionCelulares.Api.Data;

namespace RecepcionCelulares.Tests;

/// <summary>Levanta la API completa en memoria con SQLite en lugar de SQL Server.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Clave", "clave-de-pruebas-con-mas-de-32-caracteres");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            _conexion.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexion));

            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexion.Dispose();
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<T>(this IServiceCollection services)
    {
        foreach (var d in services.Where(d => d.ServiceType == typeof(T)).ToList())
            services.Remove(d);
    }
}
