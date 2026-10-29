using MesaAyuda.Domain;

namespace MesaAyuda.Tests;

public class TicketTests
{
    private static readonly DateTime Inicio = new(2026, 10, 29, 8, 0, 0, DateTimeKind.Utc);

    private static Ticket Nuevo(Prioridad prioridad = Prioridad.Media) =>
        new("No imprime", "La impresora no responde desde ayer.", Categoria.Hardware, prioridad, "Ana", Inicio);

    [Theory]
    [InlineData(Prioridad.Critica, 4)]
    [InlineData(Prioridad.Alta, 8)]
    [InlineData(Prioridad.Media, 24)]
    [InlineData(Prioridad.Baja, 72)]
    public void La_fecha_limite_depende_de_la_prioridad(Prioridad prioridad, int horas)
    {
        Assert.Equal(Inicio.AddHours(horas), Nuevo(prioridad).FechaLimite);
    }

    [Fact]
    public void Asignar_pasa_el_ticket_a_en_progreso()
    {
        var t = Nuevo();
        t.Asignar("Luis", Inicio.AddMinutes(10));

        Assert.Equal(EstadoTicket.EnProgreso, t.Estado);
        Assert.Equal("Luis", t.AsignadoA);
        Assert.Contains(t.Comentarios, c => c.Texto.Contains("Asignado a Luis"));
    }

    [Fact]
    public void No_se_puede_resolver_sin_asignar()
    {
        var ex = Assert.Throws<DomainException>(() => Nuevo().Resolver("Se reinició el equipo.", Inicio));
        Assert.Contains("asignado", ex.Message);
    }

    [Fact]
    public void Resolver_despues_de_la_fecha_limite_marca_sla_incumplido()
    {
        var t = Nuevo(Prioridad.Critica);
        t.Asignar("Luis", Inicio);
        t.Resolver("Se cambió el cable de red del equipo.", Inicio.AddHours(5));

        Assert.True(t.SlaIncumplido);
        Assert.Equal(EstadoTicket.Resuelto, t.Estado);
    }

    [Fact]
    public void Resolver_a_tiempo_cumple_el_sla()
    {
        var t = Nuevo(Prioridad.Critica);
        t.Asignar("Luis", Inicio);
        t.Resolver("Se cambió el cable de red del equipo.", Inicio.AddHours(3));

        Assert.False(t.SlaIncumplido);
    }

    [Fact]
    public void Subir_la_prioridad_recalcula_la_fecha_limite()
    {
        var t = Nuevo(Prioridad.Baja);
        t.CambiarPrioridad(Prioridad.Alta, Inicio.AddHours(1));

        Assert.Equal(Inicio.AddHours(8), t.FechaLimite);
    }

    [Fact]
    public void Verificar_sla_marca_una_sola_vez()
    {
        var t = Nuevo(Prioridad.Alta);

        Assert.False(t.VerificarSla(Inicio.AddHours(7)));
        Assert.True(t.VerificarSla(Inicio.AddHours(9)));
        Assert.False(t.VerificarSla(Inicio.AddHours(10)));
        Assert.Single(t.Comentarios, c => c.Texto.Contains("SLA"));
    }

    [Fact]
    public void Flujo_completo_con_espera_reapertura_y_cierre()
    {
        var t = Nuevo();
        t.Asignar("Luis", Inicio);
        t.PonerEnEspera("Esperando repuesto", Inicio.AddHours(1));
        t.Retomar(Inicio.AddHours(2));
        t.Resolver("Se instaló el repuesto nuevo.", Inicio.AddHours(3));
        t.Reabrir("Volvió a fallar", Inicio.AddHours(4));

        Assert.Equal(EstadoTicket.EnProgreso, t.Estado);
        Assert.Null(t.Solucion);

        t.Resolver("Se reemplazó la fuente de poder.", Inicio.AddHours(5));
        t.Cerrar(Inicio.AddHours(6));

        Assert.Equal(EstadoTicket.Cerrado, t.Estado);
        Assert.Throws<DomainException>(() => t.Comentar("Ana", "Gracias", Inicio.AddHours(7)));
        Assert.Throws<DomainException>(() => t.Asignar("Otro", Inicio.AddHours(7)));
    }

    [Fact]
    public void Solo_se_cierra_un_ticket_resuelto()
    {
        Assert.Throws<DomainException>(() => Nuevo().Cerrar(Inicio));
    }
}
