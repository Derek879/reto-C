namespace MesaAyuda.Domain;

/// <summary>
/// Incidencia reportada por un usuario. Las reglas del flujo de atención viven aquí (modelo de dominio rico),
/// así los controladores y servicios no pueden dejar un ticket en un estado inválido.
/// </summary>
public class Ticket
{
    private readonly List<Comentario> _comentarios = new();

    private Ticket() { } // EF Core

    public Ticket(string titulo, string descripcion, Categoria categoria, Prioridad prioridad, string solicitante, DateTime ahora)
    {
        Titulo = titulo.Trim();
        Descripcion = descripcion.Trim();
        Categoria = categoria;
        Prioridad = prioridad;
        Solicitante = solicitante.Trim();
        FechaCreacion = ahora;
        FechaLimite = ahora + PoliticaSla.TiempoDeResolucion(prioridad);
        Estado = EstadoTicket.Abierto;
    }

    public int Id { get; private set; }
    public string Codigo => $"INC-{Id:D6}";
    public string Titulo { get; private set; } = "";
    public string Descripcion { get; private set; } = "";
    public Categoria Categoria { get; private set; }
    public Prioridad Prioridad { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public string Solicitante { get; private set; } = "";
    public string? AsignadoA { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaLimite { get; private set; }
    public DateTime? FechaResolucion { get; private set; }
    public string? Solucion { get; private set; }

    /// <summary>Se marcó porque pasó la fecha límite sin resolverse. Queda registrado aunque luego se resuelva.</summary>
    public bool SlaIncumplido { get; private set; }

    public IReadOnlyList<Comentario> Comentarios => _comentarios;

    public bool EstaActivo => Estado is not (EstadoTicket.Resuelto or EstadoTicket.Cerrado);

    public void Asignar(string tecnico, DateTime ahora)
    {
        ExigirActivo();
        if (string.IsNullOrWhiteSpace(tecnico))
            throw new DomainException("Indica el técnico asignado.");

        AsignadoA = tecnico.Trim();
        if (Estado == EstadoTicket.Abierto)
            Estado = EstadoTicket.EnProgreso;
        Registrar("Sistema", $"Asignado a {AsignadoA}.", ahora);
    }

    /// <summary>Al cambiar la prioridad se recalcula la fecha límite desde la creación.</summary>
    public void CambiarPrioridad(Prioridad nueva, DateTime ahora)
    {
        ExigirActivo();
        if (nueva == Prioridad) return;

        Registrar("Sistema", $"Prioridad cambiada de {Prioridad} a {nueva}.", ahora);
        Prioridad = nueva;
        FechaLimite = FechaCreacion + PoliticaSla.TiempoDeResolucion(nueva);
    }

    public void PonerEnEspera(string motivo, DateTime ahora)
    {
        if (Estado != EstadoTicket.EnProgreso)
            throw new DomainException("Solo un ticket en progreso puede pasar a espera.");
        Estado = EstadoTicket.EnEspera;
        Registrar("Sistema", $"En espera: {motivo}", ahora);
    }

    public void Retomar(DateTime ahora)
    {
        if (Estado != EstadoTicket.EnEspera)
            throw new DomainException("Solo se puede retomar un ticket en espera.");
        Estado = EstadoTicket.EnProgreso;
        Registrar("Sistema", "Atención retomada.", ahora);
    }

    public void Resolver(string solucion, DateTime ahora)
    {
        if (Estado is not (EstadoTicket.EnProgreso or EstadoTicket.EnEspera))
            throw new DomainException("Para resolver, el ticket debe estar asignado y en atención.");
        if (string.IsNullOrWhiteSpace(solucion) || solucion.Trim().Length < 10)
            throw new DomainException("Describe la solución aplicada (mínimo 10 caracteres).");

        Solucion = solucion.Trim();
        FechaResolucion = ahora;
        if (ahora > FechaLimite)
            SlaIncumplido = true;
        Estado = EstadoTicket.Resuelto;
        Registrar("Sistema", "Ticket resuelto.", ahora);
    }

    /// <summary>El solicitante no está conforme: vuelve a atención y conserva el historial.</summary>
    public void Reabrir(string motivo, DateTime ahora)
    {
        if (Estado != EstadoTicket.Resuelto)
            throw new DomainException("Solo se puede reabrir un ticket resuelto.");
        Estado = EstadoTicket.EnProgreso;
        FechaResolucion = null;
        Solucion = null;
        Registrar("Sistema", $"Reabierto: {motivo}", ahora);
    }

    public void Cerrar(DateTime ahora)
    {
        if (Estado != EstadoTicket.Resuelto)
            throw new DomainException("Solo se puede cerrar un ticket resuelto.");
        Estado = EstadoTicket.Cerrado;
        Registrar("Sistema", "Ticket cerrado.", ahora);
    }

    public Comentario Comentar(string autor, string texto, DateTime ahora)
    {
        if (Estado == EstadoTicket.Cerrado)
            throw new DomainException("No se puede comentar un ticket cerrado.");
        if (string.IsNullOrWhiteSpace(texto))
            throw new DomainException("El comentario está vacío.");
        return Registrar(autor.Trim(), texto.Trim(), ahora);
    }

    /// <summary>Devuelve true si en esta llamada el ticket pasó a estar vencido.</summary>
    public bool VerificarSla(DateTime ahora)
    {
        if (SlaIncumplido || !EstaActivo || ahora <= FechaLimite)
            return false;
        SlaIncumplido = true;
        Registrar("Sistema", "Se venció el tiempo de atención (SLA).", ahora);
        return true;
    }

    private void ExigirActivo()
    {
        if (!EstaActivo)
            throw new DomainException($"El ticket está {Estado} y ya no se puede modificar.");
    }

    private Comentario Registrar(string autor, string texto, DateTime ahora)
    {
        var comentario = new Comentario(autor, texto, ahora);
        _comentarios.Add(comentario);
        return comentario;
    }
}

public class Comentario
{
    private Comentario() { } // EF Core

    public Comentario(string autor, string texto, DateTime fecha)
    {
        Autor = autor;
        Texto = texto;
        Fecha = fecha;
    }

    public int Id { get; private set; }
    public string Autor { get; private set; } = "";
    public string Texto { get; private set; } = "";
    public DateTime Fecha { get; private set; }
}
