using MesaAyuda.Domain;

namespace MesaAyuda.Infrastructure.Persistencia;

/// <summary>Carga tickets de ejemplo en una base vacía para poder probar la aplicación.</summary>
public static class DatosDeEjemplo
{
    public static async Task CargarAsync(MesaAyudaDbContext db, DateTime ahora)
    {
        if (db.Tickets.Any()) return;

        Ticket Nuevo(string titulo, string desc, Categoria c, Prioridad p, string quien, double horasAtras) =>
            new(titulo, desc, c, p, quien, ahora.AddHours(-horasAtras));

        var vpn = Nuevo("No conecta la VPN", "Desde esta mañana la VPN muestra error 809 al conectar desde casa.", Categoria.Red, Prioridad.Alta, "Rosa Medina", 10);
        vpn.Asignar("Luis Paredes", ahora.AddHours(-9));
        vpn.VerificarSla(ahora);

        var impresora = Nuevo("Impresora del 2do piso atasca papel", "La impresora HP del segundo piso atasca papel en cada impresión.", Categoria.Hardware, Prioridad.Media, "Jorge Salas", 30);
        impresora.Asignar("Carla Núñez", ahora.AddHours(-28));
        impresora.Resolver("Se limpió el rodillo de alimentación y se cambió la bandeja dañada.", ahora.AddHours(-20));

        var sap = Nuevo("Sin acceso al módulo de facturación", "Usuario nuevo de contabilidad necesita acceso al módulo de facturación del ERP.", Categoria.Accesos, Prioridad.Media, "Ana Ruiz", 3);
        sap.Asignar("Luis Paredes", ahora.AddHours(-2));

        var caida = Nuevo("Caída del sistema de ventas", "El sistema de ventas no carga en ninguna tienda, aparece error 500.", Categoria.Software, Prioridad.Critica, "Gerencia Comercial", 1);
        caida.Asignar("Carla Núñez", ahora.AddMinutes(-50));
        caida.Comentar("Carla Núñez", "Se identificó que el certificado SSL del servidor venció. Renovando.", ahora.AddMinutes(-30));

        var excel = Nuevo("Excel se cierra solo", "Al abrir archivos grandes Excel se cierra sin mostrar mensaje.", Categoria.Software, Prioridad.Baja, "Pedro Gómez", 5);
        var monitor = Nuevo("Solicitud de segundo monitor", "Se solicita un segundo monitor para el área de diseño.", Categoria.Hardware, Prioridad.Baja, "Lucía Torres", 50);

        db.Tickets.AddRange(vpn, impresora, sap, caida, excel, monitor);
        await db.SaveChangesAsync();
    }
}
