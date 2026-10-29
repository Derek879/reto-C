using System.Data;
using Microsoft.Data.SqlClient;

namespace InventarioMvc.Web.Data;

public interface IConexionSql
{
    IDbConnection Abrir();
}

public class ConexionSql(string cadena) : IConexionSql
{
    public IDbConnection Abrir()
    {
        var conexion = new SqlConnection(cadena);
        conexion.Open();
        return conexion;
    }
}

/// <summary>Error de negocio lanzado por un procedimiento almacenado con THROW 50001-50099.</summary>
public class ErrorDeNegocioException(string mensaje, Exception interna) : Exception(mensaje, interna)
{
    public static bool EsDeNegocio(SqlException ex) => ex.Number is >= 50000 and < 50100;
}
