using InventarioMvc.Web.Data;
using Microsoft.Data.SqlClient;

namespace InventarioMvc.Tests;

/// <summary>
/// Crea una base de datos SQL Server nueva para las pruebas, ejecuta los mismos scripts que usa la app
/// (tablas, procedimientos y datos iniciales) y la elimina al terminar.
/// Usa la variable de entorno INVENTARIO_SQL o, si no existe, el SQL Server del docker-compose.
/// </summary>
public class BaseDeDatosFixture : IAsyncLifetime
{
    private readonly string _servidor = Environment.GetEnvironmentVariable("INVENTARIO_SQL")
        ?? "Server=localhost,1433;User Id=sa;Password=Taller#2026Local;TrustServerCertificate=True";

    public string Cadena { get; private set; } = "";
    public IConexionSql Conexion { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Cadena = new SqlConnectionStringBuilder(_servidor) { InitialCatalog = $"InventarioPruebas_{Guid.NewGuid():N}" }.ConnectionString;
        await InicializadorBd.EjecutarAsync(Cadena, Path.Combine(AppContext.BaseDirectory, "database"));
        Conexion = new ConexionSql(Cadena);
    }

    public async Task DisposeAsync()
    {
        var nombre = new SqlConnectionStringBuilder(Cadena).InitialCatalog;
        SqlConnection.ClearAllPools();
        await using var master = new SqlConnection(new SqlConnectionStringBuilder(Cadena) { InitialCatalog = "master" }.ConnectionString);
        await master.OpenAsync();
        await using var cmd = new SqlCommand($"ALTER DATABASE [{nombre}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{nombre}];", master);
        await cmd.ExecuteNonQueryAsync();
    }
}
