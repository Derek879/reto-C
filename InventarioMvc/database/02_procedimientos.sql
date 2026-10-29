-- Procedimientos almacenados. Los errores de negocio usan THROW con códigos 50001-50099,
-- que la aplicación muestra al usuario como mensajes de validación.

CREATE OR ALTER PROCEDURE dbo.usp_Categoria_Listar
AS
    SELECT Id, Nombre FROM dbo.Categoria ORDER BY Nombre;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Listar
    @Buscar      NVARCHAR(120) = NULL,
    @CategoriaId INT = NULL,
    @SoloActivos BIT = 1
AS
    SELECT p.Id, p.Codigo, p.Nombre, p.CategoriaId, c.Nombre AS Categoria,
           p.PrecioVenta, p.Stock, p.StockMinimo, p.Activo
    FROM dbo.Producto p
    JOIN dbo.Categoria c ON c.Id = p.CategoriaId
    WHERE (@Buscar IS NULL OR p.Nombre LIKE '%' + @Buscar + '%' OR p.Codigo = @Buscar)
      AND (@CategoriaId IS NULL OR p.CategoriaId = @CategoriaId)
      AND (@SoloActivos = 0 OR p.Activo = 1)
    ORDER BY p.Nombre;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_Obtener
    @Id INT
AS
    SELECT p.Id, p.Codigo, p.Nombre, p.CategoriaId, c.Nombre AS Categoria,
           p.PrecioVenta, p.Stock, p.StockMinimo, p.Activo
    FROM dbo.Producto p
    JOIN dbo.Categoria c ON c.Id = p.CategoriaId
    WHERE p.Id = @Id;
GO

-- Inserta si @Id es NULL, actualiza si no. Devuelve el Id del producto.
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Guardar
    @Id          INT = NULL,
    @Codigo      VARCHAR(20),
    @Nombre      NVARCHAR(120),
    @CategoriaId INT,
    @PrecioVenta DECIMAL(10,2),
    @Stock       INT,
    @StockMinimo INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Producto WHERE Codigo = @Codigo AND (@Id IS NULL OR Id <> @Id))
        THROW 50001, 'Ya existe otro producto con ese código.', 1;

    IF @Id IS NULL
    BEGIN
        INSERT dbo.Producto (Codigo, Nombre, CategoriaId, PrecioVenta, Stock, StockMinimo)
        VALUES (@Codigo, @Nombre, @CategoriaId, @PrecioVenta, @Stock, @StockMinimo);
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        UPDATE dbo.Producto
        SET Codigo = @Codigo, Nombre = @Nombre, CategoriaId = @CategoriaId,
            PrecioVenta = @PrecioVenta, Stock = @Stock, StockMinimo = @StockMinimo
        WHERE Id = @Id;

        IF @@ROWCOUNT = 0
            THROW 50004, 'El producto no existe.', 1;
        SELECT @Id;
    END
END
GO

-- Los productos con ventas no se borran: se desactivan para conservar el historial.
CREATE OR ALTER PROCEDURE dbo.usp_Producto_Desactivar
    @Id INT
AS
    UPDATE dbo.Producto SET Activo = 0 WHERE Id = @Id;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Producto_StockBajo
AS
    SELECT p.Id, p.Codigo, p.Nombre, p.CategoriaId, c.Nombre AS Categoria,
           p.PrecioVenta, p.Stock, p.StockMinimo, p.Activo
    FROM dbo.Producto p
    JOIN dbo.Categoria c ON c.Id = p.CategoriaId
    WHERE p.Activo = 1 AND p.Stock <= p.StockMinimo
    ORDER BY p.Stock;
GO

-- Registra una venta completa en una transacción: valida stock, guarda cabecera y detalle y descuenta stock.
-- UPDLOCK evita que dos ventas simultáneas vendan el mismo stock.
CREATE OR ALTER PROCEDURE dbo.usp_Venta_Registrar
    @Cliente NVARCHAR(120),
    @Detalle dbo.TipoDetalleVenta READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM @Detalle)
        THROW 50010, 'La venta debe tener al menos un producto.', 1;

    IF EXISTS (SELECT 1 FROM @Detalle WHERE Cantidad <= 0)
        THROW 50011, 'Las cantidades deben ser mayores a cero.', 1;

    BEGIN TRANSACTION;

    -- Agrupa por si el mismo producto viene en dos líneas
    DECLARE @Items TABLE (ProductoId INT PRIMARY KEY, Cantidad INT, Precio DECIMAL(10,2));
    INSERT @Items (ProductoId, Cantidad, Precio)
    SELECT d.ProductoId, SUM(d.Cantidad), p.PrecioVenta
    FROM @Detalle d
    JOIN dbo.Producto p WITH (UPDLOCK, ROWLOCK) ON p.Id = d.ProductoId AND p.Activo = 1
    GROUP BY d.ProductoId, p.PrecioVenta;

    IF (SELECT COUNT(*) FROM @Items) <> (SELECT COUNT(DISTINCT ProductoId) FROM @Detalle)
        THROW 50012, 'Uno de los productos no existe o está desactivado.', 1;

    DECLARE @SinStock NVARCHAR(400) = (
        SELECT STRING_AGG(CONCAT(p.Nombre, ' (quedan ', p.Stock, ')'), ', ')
        FROM @Items i JOIN dbo.Producto p ON p.Id = i.ProductoId
        WHERE p.Stock < i.Cantidad);
    IF @SinStock IS NOT NULL
    BEGIN
        DECLARE @Mensaje NVARCHAR(500) = CONCAT('Stock insuficiente: ', @SinStock, '.');
        THROW 50013, @Mensaje, 1;
    END

    INSERT dbo.Venta (Cliente, Total)
    VALUES (@Cliente, (SELECT SUM(Cantidad * Precio) FROM @Items));
    DECLARE @VentaId INT = SCOPE_IDENTITY();

    INSERT dbo.VentaDetalle (VentaId, ProductoId, Cantidad, PrecioUnitario)
    SELECT @VentaId, ProductoId, Cantidad, Precio FROM @Items;

    UPDATE p SET p.Stock = p.Stock - i.Cantidad
    FROM dbo.Producto p JOIN @Items i ON i.ProductoId = p.Id;

    COMMIT;
    SELECT @VentaId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Venta_Listar
    @Desde DATE,
    @Hasta DATE
AS
    SELECT v.Id, v.Fecha, v.Cliente, v.Total, v.Anulada,
           (SELECT SUM(d.Cantidad) FROM dbo.VentaDetalle d WHERE d.VentaId = v.Id) AS Unidades
    FROM dbo.Venta v
    WHERE v.Fecha >= @Desde AND v.Fecha < DATEADD(DAY, 1, @Hasta)
    ORDER BY v.Fecha DESC;
GO

-- Devuelve dos resultados: la cabecera y el detalle de la venta.
CREATE OR ALTER PROCEDURE dbo.usp_Venta_Obtener
    @Id INT
AS
BEGIN
    SELECT v.Id, v.Fecha, v.Cliente, v.Total, v.Anulada,
           (SELECT SUM(d.Cantidad) FROM dbo.VentaDetalle d WHERE d.VentaId = v.Id) AS Unidades
    FROM dbo.Venta v WHERE v.Id = @Id;

    SELECT d.ProductoId, p.Codigo, p.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal
    FROM dbo.VentaDetalle d
    JOIN dbo.Producto p ON p.Id = d.ProductoId
    WHERE d.VentaId = @Id;
END
GO

-- Anula la venta y devuelve las unidades al stock. Una venta anulada no se puede anular de nuevo.
CREATE OR ALTER PROCEDURE dbo.usp_Venta_Anular
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRANSACTION;

    UPDATE dbo.Venta SET Anulada = 1 WHERE Id = @Id AND Anulada = 0;
    IF @@ROWCOUNT = 0
        THROW 50020, 'La venta no existe o ya está anulada.', 1;

    UPDATE p SET p.Stock = p.Stock + d.Cantidad
    FROM dbo.Producto p JOIN dbo.VentaDetalle d ON d.ProductoId = p.Id
    WHERE d.VentaId = @Id;

    COMMIT;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Reporte_VentasPorMes
    @Anio INT
AS
    SELECT MONTH(v.Fecha) AS Mes, COUNT(*) AS Ventas, SUM(v.Total) AS Total
    FROM dbo.Venta v
    WHERE YEAR(v.Fecha) = @Anio AND v.Anulada = 0
    GROUP BY MONTH(v.Fecha)
    ORDER BY Mes;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Reporte_TopProductos
    @Desde DATE,
    @Hasta DATE,
    @Top   INT = 10
AS
    SELECT TOP (@Top) p.Codigo, p.Nombre, SUM(d.Cantidad) AS Unidades, SUM(d.Subtotal) AS Total
    FROM dbo.VentaDetalle d
    JOIN dbo.Venta v    ON v.Id = d.VentaId AND v.Anulada = 0
    JOIN dbo.Producto p ON p.Id = d.ProductoId
    WHERE v.Fecha >= @Desde AND v.Fecha < DATEADD(DAY, 1, @Hasta)
    GROUP BY p.Codigo, p.Nombre
    ORDER BY Unidades DESC, Total DESC;
GO
