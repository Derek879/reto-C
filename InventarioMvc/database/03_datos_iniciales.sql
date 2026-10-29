-- Datos de ejemplo para probar la aplicación (solo si la base está vacía)
IF NOT EXISTS (SELECT 1 FROM dbo.Categoria)
BEGIN
    INSERT dbo.Categoria (Nombre) VALUES (N'Accesorios'), (N'Cargadores'), (N'Audio'), (N'Protectores');

    INSERT dbo.Producto (Codigo, Nombre, CategoriaId, PrecioVenta, Stock, StockMinimo) VALUES
        ('ACC-001', N'Funda silicona iPhone 13',       1, 25.00, 30, 5),
        ('ACC-002', N'Soporte de auto magnético',      1, 35.00,  8, 3),
        ('CAR-001', N'Cargador rápido USB-C 20W',      2, 49.90, 15, 5),
        ('CAR-002', N'Cable USB-C a Lightning 1m',     2, 29.90,  4, 5),
        ('AUD-001', N'Audífonos inalámbricos TWS',     3, 89.00, 10, 3),
        ('PRO-001', N'Mica de vidrio templado A54',    4, 15.00, 50, 10),
        ('PRO-002', N'Mica hidrogel universal',        4, 20.00,  2, 5);
END
GO
