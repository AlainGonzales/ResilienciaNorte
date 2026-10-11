using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository
{
    public class ResilienciaDbContext : IdentityDbContext<UsuarioAplicacion>
    {
        public ResilienciaDbContext(DbContextOptions<ResilienciaDbContext> options) : base(options)
        {
        }

        // ── Tablas Principales ───────────────────────────────────────────────
        public DbSet<Distrito> Distritos { get; set; }
        public DbSet<IncidenteEmergencia> IncidentesEmergencia { get; set; }
        public DbSet<VerificacionOtpCiudadano> VerificacionesOtpCiudadano { get; set; }
        public DbSet<RecursoAlmacen> RecursosAlmacen { get; set; }
        public DbSet<Almacen> Almacenes { get; set; }
        public DbSet<ExistenciaAlmacen> ExistenciasAlmacen { get; set; }
        public DbSet<MovimientoAlmacen> MovimientosAlmacen { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Incidencia> Incidencias { get; set; }
        public DbSet<FichaEdan> FichasEdan { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<AuditoriaSistema> AuditoriaSistema { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // CRUCIAL: base.OnModelCreating configura las llaves y tablas de AspNetUsers/AspNetRoles
            base.OnModelCreating(modelBuilder);

            // Configuración de la relación Usuario -> Distrito (Jurisdicción)
            modelBuilder.Entity<UsuarioAplicacion>(entity =>
            {
                entity.HasOne(u => u.Distrito)
                      .WithMany()
                      .HasForeignKey(u => u.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

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

                // 1. Relación con IncidenteEmergencia
                entity.HasOne(e => e.Distrito)
                      .WithMany(d => d.Incidentes) // Enlaza con la colección de Distrito.cs
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Incidencia>(entity =>
            {
                entity.HasKey(e => e.IncidenciaId);
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Codigo).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Sector).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TipoEvento).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue(EstadoIncidencia.Abierta);

                entity.HasOne(e => e.Distrito)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Reportes)
                      .WithOne(r => r.Incidencia)
                      .HasForeignKey(r => r.IncidenciaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FichaEdan)
                      .WithOne(f => f.Incidencia)
                      .HasForeignKey<FichaEdan>(f => f.IncidenciaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RecursoAlmacen>(entity =>
            {
                entity.HasKey(e => e.RecursoId);
                entity.HasIndex(e => e.CodigoBAH).IsUnique();
                entity.Property(e => e.CodigoBAH).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Categoria).HasMaxLength(50).IsRequired();
                entity.Property(e => e.UnidadMedida).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.HasKey(e => e.AlmacenId);
                entity.Property(e => e.Nombre).HasMaxLength(80).IsRequired();
                entity.Property(e => e.Nivel).HasMaxLength(20).IsRequired();

                entity.HasOne(e => e.Distrito)
                      .WithMany()
                      .HasForeignKey(e => e.DistritoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Provincia)
                      .WithMany()
                      .HasForeignKey(e => e.ProvinciaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AlmacenPadre)
                      .WithMany()
                      .HasForeignKey(e => e.AlmacenPadreId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ExistenciaAlmacen>(entity =>
            {
                entity.HasKey(e => new { e.AlmacenId, e.RecursoId });

                entity.HasOne(e => e.Almacen)
                      .WithMany(a => a.Existencias)
                      .HasForeignKey(e => e.AlmacenId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Recurso)
                      .WithMany()
                      .HasForeignKey(e => e.RecursoId)
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

            modelBuilder.Entity<MovimientoAlmacen>(entity =>
            {
                entity.HasKey(e => e.MovimientoId);
                entity.Property(e => e.TipoMovimiento).HasMaxLength(20).IsRequired();
                entity.Property(e => e.ConceptoMovimiento).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.Recurso)
                      .WithMany()
                      .HasForeignKey(e => e.RecursoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Almacen)
                      .WithMany()
                      .HasForeignKey(e => e.AlmacenId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Solicitud>(entity =>
            {
                entity.HasKey(e => e.SolicitudId);
                entity.HasIndex(e => e.CodigoSolicitud).IsUnique();
                entity.Property(e => e.CodigoSolicitud).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Tipo).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(20).HasDefaultValue("Pendiente");

                entity.HasOne(e => e.AlmacenSolicitante)
                      .WithMany()
                      .HasForeignKey(e => e.AlmacenSolicitanteId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AlmacenProveedor)
                      .WithMany()
                      .HasForeignKey(e => e.AlmacenProveedorId)
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