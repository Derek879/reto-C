using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace InventarioMvc.Web.Data;

/// <summary>Crea la base de datos si no existe y ejecuta los scripts de la carpeta database/ en orden.</summary>
public static partial class InicializadorBd
{
    public static async Task EjecutarAsync(string cadena, string carpetaScripts)
    {
        var builder = new SqlConnectionStringBuilder(cadena);
        var nombreBd = builder.InitialCatalog;

        builder.InitialCatalog = "master";
        await using (var master = new SqlConnection(builder.ConnectionString))
        {
            await master.OpenAsync();
            await using var crear = new SqlCommand(
                $"IF DB_ID(@nombre) IS NULL CREATE DATABASE [{nombreBd.Replace("]", "]]")}]", master);
            crear.Parameters.AddWithValue("@nombre", nombreBd);
            await crear.ExecuteNonQueryAsync();
        }

        await using var db = new SqlConnection(cadena);
        await db.OpenAsync();
        foreach (var archivo in Directory.GetFiles(carpetaScripts, "*.sql").Order())
        {
            // "GO" no es T-SQL: separa lotes, así que cada lote se ejecuta por separado.
            foreach (var lote in SeparadorGo().Split(await File.ReadAllTextAsync(archivo)).Where(l => !string.IsNullOrWhiteSpace(l)))
            {
                await using var cmd = new SqlCommand(lote, db);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex SeparadorGo();
}
