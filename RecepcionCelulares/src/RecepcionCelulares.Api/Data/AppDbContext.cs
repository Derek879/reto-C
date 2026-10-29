using Microsoft.EntityFrameworkCore;
using RecepcionCelulares.Api.Domain;
using RecepcionCelulares.Api.Infrastructure;

namespace RecepcionCelulares.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, INegocioActual negocioActual) : DbContext(options)
{
    private int? NegocioId => negocioActual.NegocioId;

    public DbSet<Negocio> Negocios => Set<Negocio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Repuesto> Repuestos => Set<Repuesto>();
    public DbSet<OrdenServicio> Ordenes => Set<OrdenServicio>();
    public DbSet<OrdenRepuesto> OrdenRepuestos => Set<OrdenRepuesto>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Negocio>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Ruc).HasMaxLength(11);
        });

        b.Entity<Usuario>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Cliente>(e =>
        {
            e.HasIndex(x => new { x.NegocioId, x.Dni }).IsUnique();
            e.Property(x => x.Nombres).HasMaxLength(150);
            e.Property(x => x.Dni).HasMaxLength(12);
            e.Property(x => x.Telefono).HasMaxLength(15);
        });

        b.Entity<Repuesto>(e =>
        {
            e.Property(x => x.Nombre).HasMaxLength(120);
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CostoUnitario).HasPrecision(10, 2);
            e.ToTable(t => t.HasCheckConstraint("CK_Repuesto_Stock", "[Stock] >= 0"));
        });

        b.Entity<OrdenServicio>(e =>
        {
            e.HasIndex(x => new { x.NegocioId, x.Numero }).IsUnique();
            e.Property(x => x.Marca).HasMaxLength(50);
            e.Property(x => x.Modelo).HasMaxLength(80);
            e.Property(x => x.Imei).HasMaxLength(20);
            e.Property(x => x.Falla).HasMaxLength(500);
            e.Property(x => x.MotivoAnulacion).HasMaxLength(300);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PrecioServicio).HasPrecision(10, 2);
            e.Property(x => x.Adelanto).HasPrecision(10, 2);
            e.Ignore(x => x.Saldo);
            e.Ignore(x => x.CostoRepuestos);
            e.HasOne(x => x.Cliente).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Repuestos).WithOne().HasForeignKey(x => x.OrdenServicioId);
        });

        b.Entity<OrdenRepuesto>(e =>
        {
            e.Property(x => x.CostoUnitario).HasPrecision(10, 2);
            e.HasOne(x => x.Repuesto).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        // Todo lo que pertenece a un negocio pasa por este filtro: un usuario nunca ve datos de otra cuenta.
        b.Entity<Usuario>().HasQueryFilter(x => x.NegocioId == NegocioId);
        b.Entity<Cliente>().HasQueryFilter(x => x.NegocioId == NegocioId);
        b.Entity<Repuesto>().HasQueryFilter(x => x.NegocioId == NegocioId);
        b.Entity<OrdenServicio>().HasQueryFilter(x => x.NegocioId == NegocioId);
        b.Entity<OrdenRepuesto>().HasQueryFilter(x => x.Repuesto!.NegocioId == NegocioId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Asigna el negocio actual a todo registro nuevo, para no depender de que cada servicio lo recuerde.
        foreach (var entry in ChangeTracker.Entries<IPerteneceANegocio>().Where(e => e.State == EntityState.Added))
        {
            if (entry.Entity.NegocioId == 0 && NegocioId is int id)
                entry.Entity.NegocioId = id;
        }
        return base.SaveChangesAsync(ct);
    }
}
