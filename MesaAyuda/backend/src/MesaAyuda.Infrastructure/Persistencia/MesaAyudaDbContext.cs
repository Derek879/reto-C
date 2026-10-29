using MesaAyuda.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MesaAyuda.Infrastructure.Persistencia;

public class MesaAyudaDbContext(DbContextOptions<MesaAyudaDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    /// <summary>Todas las fechas se guardan en UTC y se leen marcadas como UTC, para que el frontend las convierta a hora local.</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        b.Properties<DateTime>().HaveConversion<FechaUtcConverter>();
        b.Properties<DateTime?>().HaveConversion<FechaUtcNullableConverter>();
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Ticket>(e =>
        {
            e.Property(x => x.Titulo).HasMaxLength(150);
            e.Property(x => x.Descripcion).HasMaxLength(2000);
            e.Property(x => x.Solicitante).HasMaxLength(100);
            e.Property(x => x.AsignadoA).HasMaxLength(100);
            e.Property(x => x.Solucion).HasMaxLength(2000);
            e.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Prioridad).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Ignore(x => x.Codigo);
            e.Ignore(x => x.EstaActivo);

            // Índice para la bandeja de trabajo y para el monitor de SLA
            e.HasIndex(x => new { x.Estado, x.FechaLimite });
            e.HasIndex(x => x.AsignadoA);

            e.HasMany(x => x.Comentarios).WithOne().HasForeignKey("TicketId").OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.Comentarios).HasField("_comentarios").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<Comentario>(e =>
        {
            e.Property(x => x.Autor).HasMaxLength(100);
            e.Property(x => x.Texto).HasMaxLength(1000);
        });
    }
}

internal class FechaUtcConverter() : ValueConverter<DateTime, DateTime>(
    v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

internal class FechaUtcNullableConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
