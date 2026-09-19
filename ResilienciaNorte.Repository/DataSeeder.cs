using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository;

public static class DataSeeder
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        // 1. Catálogo Territorial - Distritos de Trujillo
        modelBuilder.Entity<Distrito>().HasData(
            new Distrito { Id = 1, Nombre = "El Porvenir", ZonaVulnerablePrincipal = "Quebrada San Ildefonso / Río Seco", PoblacionEstimada = 195000, Activo = true },
            new Distrito { Id = 2, Nombre = "Florencia de Mora", ZonaVulnerablePrincipal = "Sectores bajos y red troncal de desagüe", PoblacionEstimada = 42000, Activo = true },
            new Distrito { Id = 3, Nombre = "La Esperanza", ZonaVulnerablePrincipal = "Parte alta / Quebrada El León", PoblacionEstimada = 189000, Activo = true },
            new Distrito { Id = 4, Nombre = "Trujillo Centro Historico", ZonaVulnerablePrincipal = "Av. España y microcuenca pluvial urbana", PoblacionEstimada = 315000, Activo = true },
            new Distrito { Id = 5, Nombre = "Huanchaco", ZonaVulnerablePrincipal = "El Trópico y desembocadura Quebrada El León", PoblacionEstimada = 68000, Activo = true },
            new Distrito { Id = 6, Nombre = "Víctor Larco Herrera", ZonaVulnerablePrincipal = "Sector Buenos Aires Sur / Inundaciones costeras", PoblacionEstimada = 65000, Activo = true },
            new Distrito { Id = 7, Nombre = "Laredo", ZonaVulnerablePrincipal = "Riberas de la cuenca del Río Moche", PoblacionEstimada = 37000, Activo = true },
            new Distrito { Id = 8, Nombre = "Moche", ZonaVulnerablePrincipal = "Campiña de Moche y zonas agrícolas ribereñas", PoblacionEstimada = 38000, Activo = true }
        );

        // 2. Catálogo de Recursos de Contingencia (Almacén Inicial de Bienes BAH)
        modelBuilder.Entity<RecursoAlmacen>().HasData(
            new RecursoAlmacen { Id = 1, CodigoBAH = "BAH-001", Nombre = "Sacos terreros de polipropileno", UnidadMedida = "Unidades", StockDisponible = 5000, StockMinimoSeguridad = 1000, DistritoId = 1 },
            new RecursoAlmacen { Id = 2, CodigoBAH = "BAH-002", Nombre = "Bobinas de plástico calibre pesado (200m)", UnidadMedida = "Rollos", StockDisponible = 150, StockMinimoSeguridad = 30, DistritoId = 1 },
            new RecursoAlmacen { Id = 3, CodigoBAH = "BAH-003", Nombre = "Motobomba de achique autocebante 4\"", UnidadMedida = "Equipos", StockDisponible = 8, StockMinimoSeguridad = 2, DistritoId = 4 },
            new RecursoAlmacen { Id = 4, CodigoBAH = "BAH-004", Nombre = "Carpas familiares de lona impermeable", UnidadMedida = "Unidades", StockDisponible = 60, StockMinimoSeguridad = 15, DistritoId = 3 },
            new RecursoAlmacen { Id = 5, CodigoBAH = "BAH-005", Nombre = "Bidones de agua potable 20L", UnidadMedida = "Unidades", StockDisponible = 400, StockMinimoSeguridad = 100, DistritoId = 5 },
            new RecursoAlmacen { Id = 6, CodigoBAH = "BAH-006", Nombre = "Camas plegables de lona para albergues", UnidadMedida = "Unidades", StockDisponible = 120, StockMinimoSeguridad = 25, DistritoId = 6 }
        );

        // 3. Incidentes Base de Prueba (Histórico inicial de contingencia)
        modelBuilder.Entity<IncidenteEmergencia>().HasData(
            new IncidenteEmergencia
            {
                Id = 1,
                Titulo = "Acumulación pluvial crítica en Sector Río Seco",
                Descripcion = "Drenaje pluvial colapsado por barro acumulado tras lluvias en la parte alta. Afectación a viviendas contiguas.",
                TipoDesastre = "Inundación Pluvial",
                NivelSeveridad = "Crítico",
                DireccionReferencia = "Av. Sánchez Carrión cuadra 12, El Porvenir",
                FamiliasAfectadas = 18,
                FechaReporte = new DateTime(2026, 9, 18, 14, 30, 0),
                Estado = "En Evaluación",
                DistritoId = 1
            },
            new IncidenteEmergencia
            {
                Id = 2,
                Titulo = "Anegamiento de calzada principal en El Trópico",
                Descripcion = "Escorrentía superficial ingresando a predios en el margen de la carretera.",
                TipoDesastre = "Inundación Pluvial",
                NivelSeveridad = "Moderado",
                DireccionReferencia = "Entrada principal a El Trópico, Huanchaco",
                FamiliasAfectadas = 5,
                FechaReporte = new DateTime(2026, 9, 18, 18, 15, 0),
                Estado = "Pendiente",
                DistritoId = 5
            }
        );
    }
}