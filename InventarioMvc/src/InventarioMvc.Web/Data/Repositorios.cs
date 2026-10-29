using System.Data;
using Dapper;
using InventarioMvc.Web.Models;
using Microsoft.Data.SqlClient;

namespace InventarioMvc.Web.Data;

/// <summary>Todo el acceso a datos pasa por procedimientos almacenados usando Dapper.</summary>
public abstract class RepositorioBase(IConexionSql conexion)
{
    protected async Task<T> EjecutarAsync<T>(Func<IDbConnection, Task<T>> accion)
    {
        using var db = conexion.Abrir();
        try
        {
            return await accion(db);
        }
        catch (SqlException ex) when (ErrorDeNegocioException.EsDeNegocio(ex))
        {
            throw new ErrorDeNegocioException(ex.Message, ex);
        }
    }
}

public class ProductoRepositorio(IConexionSql conexion) : RepositorioBase(conexion)
{
    public Task<IEnumerable<Categoria>> CategoriasAsync() =>
        EjecutarAsync(db => db.QueryAsync<Categoria>("dbo.usp_Categoria_Listar", commandType: CommandType.StoredProcedure));

    public Task<IEnumerable<Producto>> ListarAsync(string? buscar = null, int? categoriaId = null, bool soloActivos = true) =>
        EjecutarAsync(db => db.QueryAsync<Producto>("dbo.usp_Producto_Listar",
            new { Buscar = string.IsNullOrWhiteSpace(buscar) ? null : buscar.Trim(), CategoriaId = categoriaId, SoloActivos = soloActivos },
            commandType: CommandType.StoredProcedure));

    public Task<Producto?> ObtenerAsync(int id) =>
        EjecutarAsync(db => db.QuerySingleOrDefaultAsync<Producto>("dbo.usp_Producto_Obtener", new { Id = id },
            commandType: CommandType.StoredProcedure));

    public Task<int> GuardarAsync(Producto p) =>
        EjecutarAsync(db => db.ExecuteScalarAsync<int>("dbo.usp_Producto_Guardar",
            new { Id = p.Id == 0 ? (int?)null : p.Id, p.Codigo, p.Nombre, p.CategoriaId, p.PrecioVenta, p.Stock, p.StockMinimo },
            commandType: CommandType.StoredProcedure));

    public Task<int> DesactivarAsync(int id) =>
        EjecutarAsync(db => db.ExecuteAsync("dbo.usp_Producto_Desactivar", new { Id = id }, commandType: CommandType.StoredProcedure));

    public Task<IEnumerable<Producto>> StockBajoAsync() =>
        EjecutarAsync(db => db.QueryAsync<Producto>("dbo.usp_Producto_StockBajo", commandType: CommandType.StoredProcedure));
}

public class VentaRepositorio(IConexionSql conexion) : RepositorioBase(conexion)
{
    /// <summary>Envía todo el detalle en un table-valued parameter para registrar la venta en una sola transacción.</summary>
    public Task<int> RegistrarAsync(string cliente, IEnumerable<LineaVenta> lineas)
    {
        var detalle = new DataTable();
        detalle.Columns.Add("ProductoId", typeof(int));
        detalle.Columns.Add("Cantidad", typeof(int));
        foreach (var l in lineas)
            detalle.Rows.Add(l.ProductoId, l.Cantidad);

        return EjecutarAsync(db => db.ExecuteScalarAsync<int>("dbo.usp_Venta_Registrar",
            new { Cliente = cliente.Trim(), Detalle = detalle.AsTableValuedParameter("dbo.TipoDetalleVenta") },
            commandType: CommandType.StoredProcedure));
    }

    public Task<IEnumerable<Venta>> ListarAsync(DateOnly desde, DateOnly hasta) =>
        EjecutarAsync(db => db.QueryAsync<Venta>("dbo.usp_Venta_Listar",
            new { Desde = desde.ToDateTime(TimeOnly.MinValue), Hasta = hasta.ToDateTime(TimeOnly.MinValue) },
            commandType: CommandType.StoredProcedure));

    /// <summary>El procedimiento devuelve dos resultados (cabecera y detalle) que se leen con QueryMultiple.</summary>
    public Task<Venta?> ObtenerAsync(int id) =>
        EjecutarAsync(async db =>
        {
            using var resultados = await db.QueryMultipleAsync("dbo.usp_Venta_Obtener", new { Id = id },
                commandType: CommandType.StoredProcedure);
            var venta = await resultados.ReadSingleOrDefaultAsync<Venta>();
            if (venta is not null)
                venta.Detalle = (await resultados.ReadAsync<VentaDetalle>()).ToList();
            return venta;
        });

    public Task<int> AnularAsync(int id) =>
        EjecutarAsync(db => db.ExecuteAsync("dbo.usp_Venta_Anular", new { Id = id }, commandType: CommandType.StoredProcedure));
}

public class ReporteRepositorio(IConexionSql conexion) : RepositorioBase(conexion)
{
    public Task<IEnumerable<VentasMes>> VentasPorMesAsync(int anio) =>
        EjecutarAsync(db => db.QueryAsync<VentasMes>("dbo.usp_Reporte_VentasPorMes", new { Anio = anio },
            commandType: CommandType.StoredProcedure));

    public Task<IEnumerable<TopProducto>> TopProductosAsync(DateOnly desde, DateOnly hasta, int top = 10) =>
        EjecutarAsync(db => db.QueryAsync<TopProducto>("dbo.usp_Reporte_TopProductos",
            new { Desde = desde.ToDateTime(TimeOnly.MinValue), Hasta = hasta.ToDateTime(TimeOnly.MinValue), Top = top },
            commandType: CommandType.StoredProcedure));
}
