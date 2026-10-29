namespace MesaAyuda.Tests;

/// <summary>Reloj controlable para probar el SLA sin esperar horas reales.</summary>
public class RelojFalso(DateTimeOffset inicio) : TimeProvider
{
    public DateTimeOffset Ahora { get; private set; } = inicio;
    public override DateTimeOffset GetUtcNow() => Ahora;
    public void Avanzar(TimeSpan tiempo) => Ahora += tiempo;
}
