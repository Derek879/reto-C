-- Esquema de la base de datos de inventario y ventas (SQL Server)

IF OBJECT_ID('dbo.VentaDetalle') IS NULL
BEGIN
    CREATE TABLE dbo.Categoria (
        Id      INT IDENTITY PRIMARY KEY,
        Nombre  NVARCHAR(80) NOT NULL CONSTRAINT UQ_Categoria_Nombre UNIQUE
    );

    CREATE TABLE dbo.Producto (
        Id           INT IDENTITY PRIMARY KEY,
        Codigo       VARCHAR(20)   NOT NULL CONSTRAINT UQ_Producto_Codigo UNIQUE,
        Nombre       NVARCHAR(120) NOT NULL,
        CategoriaId  INT           NOT NULL CONSTRAINT FK_Producto_Categoria REFERENCES dbo.Categoria(Id),
        PrecioVenta  DECIMAL(10,2) NOT NULL CONSTRAINT CK_Producto_Precio CHECK (PrecioVenta >= 0),
        Stock        INT           NOT NULL CONSTRAINT CK_Producto_Stock CHECK (Stock >= 0),
        StockMinimo  INT           NOT NULL CONSTRAINT DF_Producto_StockMinimo DEFAULT 5,
        Activo       BIT           NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT 1
    );

    CREATE TABLE dbo.Venta (
        Id       INT IDENTITY PRIMARY KEY,
        Fecha    DATETIME2(0)  NOT NULL CONSTRAINT DF_Venta_Fecha DEFAULT SYSDATETIME(),
        Cliente  NVARCHAR(120) NOT NULL,
        Total    DECIMAL(12,2) NOT NULL,
        Anulada  BIT           NOT NULL CONSTRAINT DF_Venta_Anulada DEFAULT 0
    );

    CREATE TABLE dbo.VentaDetalle (
        Id              INT IDENTITY PRIMARY KEY,
        VentaId         INT           NOT NULL CONSTRAINT FK_Detalle_Venta REFERENCES dbo.Venta(Id),
        ProductoId      INT           NOT NULL CONSTRAINT FK_Detalle_Producto REFERENCES dbo.Producto(Id),
        Cantidad        INT           NOT NULL CONSTRAINT CK_Detalle_Cantidad CHECK (Cantidad > 0),
        PrecioUnitario  DECIMAL(10,2) NOT NULL,
        Subtotal        AS (Cantidad * PrecioUnitario) PERSISTED
    );

    CREATE INDEX IX_Venta_Fecha ON dbo.Venta(Fecha) INCLUDE (Total, Anulada);
    CREATE INDEX IX_VentaDetalle_VentaId ON dbo.VentaDetalle(VentaId);
END
GO

-- Tipo tabla para enviar todo el detalle de una venta en una sola llamada (table-valued parameter)
IF TYPE_ID('dbo.TipoDetalleVenta') IS NULL
    CREATE TYPE dbo.TipoDetalleVenta AS TABLE (
        ProductoId INT NOT NULL,
        Cantidad   INT NOT NULL
    );
GO
