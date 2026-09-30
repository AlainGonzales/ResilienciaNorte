using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository
{
    public class ResilienciaDbContext : DbContext
    {
        public ResilienciaDbContext(DbContextOptions<ResilienciaDbContext> options) : base(options)
        {
        }

        // ── Tablas Principales ───────────────────────────────────────────────
        public DbSet<Distrito> Distritos { get; set; }
        public DbSet<IncidenteEmergencia> IncidentesEmergencia { get; set; }
        public DbSet<VerificacionOtpCiudadano> VerificacionesOtpCiudadano { get; set; }
        public DbSet<RecursoAlmacen> RecursosAlmacen { get; set; }
        public DbSet<OrdenAtencion> OrdenesAtencion { get; set; }
        public DbSet<DetalleOrden> DetallesOrden { get; set; }
        public DbSet<MovimientoAlmacen> MovimientosAlmacen { get; set; }
        public DbSet<SolicitudReabastecimiento> SolicitudesReabastecimiento { get; set; }
        public DbSet<AuditoriaSistema> AuditoriaSistema { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Restricciones e Índices Únicos ────────────────────────────────
            modelBuilder.Entity<IncidenteEmergencia>(entity =>
            {
                entity.HasKey(e => e.IncidenteId);
                entity.HasIndex(e => e.CodigoIncidente).IsUnique();
                entity.Property(e => e.CodigoIncidente).HasMaxLength(30).IsRequired();
                entity.Property(e => e.DniCiudadano).HasMaxLength(8).IsRequired();
                entity.Property(e => e.NombreCiudadano).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(15);
                entity.Property(e => e.SectorCritico).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TipoEvento).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Severidad).HasMaxLength(20).HasDefaultValue("Moderado");
                entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Reportado");

                // Relación con Distrito
                entity.HasOne(e => e.Distrito)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RecursoAlmacen>(entity =>
            {
                entity.HasKey(e => e.RecursoId);
                entity.HasIndex(e => e.CodigoBAH).IsUnique();
                entity.Property(e => e.CodigoBAH).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Categoria).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UnidadMedida).HasMaxLength(20).IsRequired();

                // Relación con Distrito custodio
                entity.HasOne(e => e.Distrito)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<VerificacionOtpCiudadano>(entity =>
            {
                entity.HasKey(e => e.OtpId);
                entity.Property(e => e.DniCiudadano).HasMaxLength(8).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(15).IsRequired();
                entity.Property(e => e.CodigoIncidente).HasMaxLength(30).IsRequired();
                entity.Property(e => e.PinHash).HasMaxLength(128).IsRequired();
            });

            // ── Maestro-Detalle (OrdenAtencion -> DetalleOrden) ───────────────
            modelBuilder.Entity<OrdenAtencion>(entity =>
            {
                entity.HasKey(e => e.OrdenId);
                entity.HasIndex(e => e.CodigoOrden).IsUnique();
                entity.Property(e => e.CodigoOrden).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Aprobada");

                // Relación 1 a 1 con Incidente
                entity.HasOne(e => e.Incidente)
                      .WithOne()
                      .HasForeignKey<OrdenAtencion>(e => e.IncidenteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación 1 a N con DetalleOrden (Borrado en Cascada)
                entity.HasMany(e => e.Detalles)
                      .WithOne(d => d.OrdenAtencion)
                      .HasForeignKey(d => d.OrdenId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DetalleOrden>(entity =>
            {
                entity.HasKey(e => e.DetalleId);

                entity.HasOne(e => e.Recurso)
                      .WithMany()
                      .HasForeignKey(e => e.RecursoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MovimientoAlmacen>(entity =>
            {
                entity.HasKey(e => e.MovimientoId);
                entity.Property(e => e.TipoMovimiento).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ConceptoMovimiento).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.Recurso)
                      .WithMany()
                      .HasForeignKey(e => e.RecursoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Distrito)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SolicitudReabastecimiento>(entity =>
            {
                entity.HasKey(e => e.SolicitudId);
                entity.HasIndex(e => e.CodigoSolicitud).IsUnique();
                entity.Property(e => e.CodigoSolicitud).HasMaxLength(30).IsRequired();
                entity.Property(e => e.NivelDestino).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");

                entity.HasOne(e => e.DistritoOrigen)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoOrigenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Recurso)
                      .WithMany()
                      .HasForeignKey(e => e.RecursoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuditoriaSistema>(entity =>
            {
                entity.HasKey(e => e.AuditoriaId);
                entity.Property(e => e.UsuarioId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RolUsuario).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Operacion).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Modulo).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Detalle).HasMaxLength(500).IsRequired();
            });

            // ── Invocación al Sembrado Centralizado desde DataSeeder.cs ─────────
            modelBuilder.SeedData();
        }
    }
}