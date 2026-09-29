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

            // ── Data Seeding Oficial (Distritos e Inventario Base) ─────────────
            modelBuilder.Entity<Distrito>().HasData(
                new Distrito { DistritoId = 1, Nombre = "El Porvenir", NivelRiesgo = "Alto", UbicacionCOEL = "Av. Sánchez Carrión 1200" },
                new Distrito { DistritoId = 2, Nombre = "Florencia de Mora", NivelRiesgo = "Alto", UbicacionCOEL = "Calle 20 de Setiembre" },
                new Distrito { DistritoId = 3, Nombre = "La Esperanza", NivelRiesgo = "Medio", UbicacionCOEL = "Plaza de Armas La Esperanza" },
                new Distrito { DistritoId = 4, Nombre = "Huanchaco", NivelRiesgo = "Medio", UbicacionCOEL = "Av. Víctor Larco Herrera" },
                new Distrito { DistritoId = 5, Nombre = "Víctor Larco Herrera", NivelRiesgo = "Alto", UbicacionCOEL = "Buenos Aires" },
                new Distrito { DistritoId = 6, Nombre = "Trujillo Centro", NivelRiesgo = "Bajo", UbicacionCOEL = "COEP - Palacio Municipal" }
            );

            modelBuilder.Entity<RecursoAlmacen>().HasData(
                new RecursoAlmacen { RecursoId = 1, CodigoBAH = "BAH-001", Nombre = "Sacos terreros de polipropileno", Categoria = "Defensa Ribereña", UnidadMedida = "Unidades", StockDisponible = 15000, StockMinimo = 500, DistritoId = 1 },
                new RecursoAlmacen { RecursoId = 2, CodigoBAH = "BAH-002", Nombre = "Bobinas de plástico impermeable 6x50m", Categoria = "Techo", UnidadMedida = "Rollos", StockDisponible = 300, StockMinimo = 20, DistritoId = 1 },
                new RecursoAlmacen { RecursoId = 3, CodigoBAH = "BAH-003", Nombre = "Motobomba de achique autocebante 4\"", Categoria = "Equipamiento", UnidadMedida = "Equipos", StockDisponible = 15, StockMinimo = 3, DistritoId = 1 },
                new RecursoAlmacen { RecursoId = 4, CodigoBAH = "BAH-004", Nombre = "Calaminas galvanizadas 1.83x0.83m", Categoria = "Techo", UnidadMedida = "Planchas", StockDisponible = 5000, StockMinimo = 200, DistritoId = 1 },
                new RecursoAlmacen { RecursoId = 5, CodigoBAH = "BAH-005", Nombre = "Kit de víveres no perecibles (ración 3 días)", Categoria = "Alimentos", UnidadMedida = "Kits", StockDisponible = 2000, StockMinimo = 100, DistritoId = 1 }
            );
        }
    }
}