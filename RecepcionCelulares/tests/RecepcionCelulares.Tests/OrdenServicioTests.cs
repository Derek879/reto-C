using RecepcionCelulares.Api.Domain;

namespace RecepcionCelulares.Tests;

public class OrdenServicioTests
{
    [Theory]
    [InlineData(EstadoOrden.Recibido, EstadoOrden.EnReparacion, true)]
    [InlineData(EstadoOrden.Recibido, EstadoOrden.Entregado, false)]
    [InlineData(EstadoOrden.EnReparacion, EstadoOrden.Listo, true)]
    [InlineData(EstadoOrden.Listo, EstadoOrden.Entregado, true)]
    [InlineData(EstadoOrden.Listo, EstadoOrden.EnReparacion, true)]
    [InlineData(EstadoOrden.Entregado, EstadoOrden.Anulado, false)]
    [InlineData(EstadoOrden.Anulado, EstadoOrden.Recibido, false)]
    public void Valida_las_transiciones_de_estado(EstadoOrden actual, EstadoOrden nuevo, bool permitido)
    {
        var orden = new OrdenServicio { Estado = actual };
        Assert.Equal(permitido, orden.PuedeCambiarA(nuevo));
    }

    [Fact]
    public void Calcula_saldo_y_costo_de_repuestos()
    {
        var orden = new OrdenServicio
        {
            PrecioServicio = 300m,
            Adelanto = 100m,
            Repuestos = { new OrdenRepuesto { Cantidad = 2, CostoUnitario = 45.50m } },
        };

        Assert.Equal(200m, orden.Saldo);
        Assert.Equal(91m, orden.CostoRepuestos);
    }
}
