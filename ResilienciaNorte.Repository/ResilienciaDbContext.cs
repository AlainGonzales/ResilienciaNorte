using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository;

public class ResilienciaDbContext : DbContext
{
    public ResilienciaDbContext(DbContextOptions<ResilienciaDbContext> options) : base(options)
    {
    }

    public DbSet<Distrito> Distritos => Set<Distrito>();
    public DbSet<IncidenteEmergencia> Incidentes => Set<IncidenteEmergencia>();
    public DbSet<RecursoAlmacen> Recursos => Set<RecursoAlmacen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación Distrito -> Incidentes
        modelBuilder.Entity<IncidenteEmergencia>()
            .HasOne(i => i.Distrito)
            .WithMany(d => d.Incidentes)
            .HasForeignKey(i => i.DistritoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Distrito -> Recursos
        modelBuilder.Entity<RecursoAlmacen>()
            .HasOne(r => r.Distrito)
            .WithMany(d => d.Recursos)
            .HasForeignKey(r => r.DistritoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Aplicar Seeding inicial de Trujillo
        modelBuilder.SeedData();
    }
}