namespace MesaAyuda.Domain;

/// <summary>Tiempo máximo de resolución según la prioridad del ticket.</summary>
public static class PoliticaSla
{
    public static TimeSpan TiempoDeResolucion(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Critica => TimeSpan.FromHours(4),
        Prioridad.Alta => TimeSpan.FromHours(8),
        Prioridad.Media => TimeSpan.FromHours(24),
        Prioridad.Baja => TimeSpan.FromHours(72),
        _ => throw new ArgumentOutOfRangeException(nameof(prioridad)),
    };
}
