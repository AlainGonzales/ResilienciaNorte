using System;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository
{
    public static class DataSeeder
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // ── 1. Catálogo Territorial - Distritos de Trujillo ──────────────
            modelBuilder.Entity<Distrito>().HasData(
                new Distrito { DistritoId = 1, Nombre = "El Porvenir", NivelRiesgo = "Alto", UbicacionCOEL = "Av. Sánchez Carrión 1200", Activo = true },
                new Distrito { DistritoId = 2, Nombre = "Florencia de Mora", NivelRiesgo = "Alto", UbicacionCOEL = "Calle 20 de Setiembre", Activo = true },
                new Distrito { DistritoId = 3, Nombre = "La Esperanza", NivelRiesgo = "Medio", UbicacionCOEL = "Plaza de Armas La Esperanza", Activo = true },
                new Distrito { DistritoId = 4, Nombre = "Huanchaco", NivelRiesgo = "Medio", UbicacionCOEL = "Av. Víctor Larco Herrera", Activo = true },
                new Distrito { DistritoId = 5, Nombre = "Víctor Larco Herrera", NivelRiesgo = "Alto", UbicacionCOEL = "Buenos Aires", Activo = true },
                new Distrito { DistritoId = 6, Nombre = "Trujillo Centro", NivelRiesgo = "Bajo", UbicacionCOEL = "COEP - Palacio Municipal", Activo = true },
                new Distrito { DistritoId = 7, Nombre = "Laredo", NivelRiesgo = "Alto", UbicacionCOEL = "Riberas de la cuenca del Río Moche", Activo = true },
                new Distrito { DistritoId = 8, Nombre = "Moche", NivelRiesgo = "Medio", UbicacionCOEL = "Campaña de Moche", Activo = true }
            );

            // ── 2. Catálogo de Recursos de Contingencia (Almacén Inicial BAH) ──
            modelBuilder.Entity<RecursoAlmacen>().HasData(
                new RecursoAlmacen
                {
                    RecursoId = 1,
                    CodigoBAH = "BAH-001",
                    Nombre = "Sacos terreros de polipropileno",
                    Categoria = "Defensa Ribereña",
                    UnidadMedida = "Unidades",
                    StockDisponible = 15000,
                    StockMinimo = 500,
                    DistritoId = 1
                },
                new RecursoAlmacen
                {
                    RecursoId = 2,
                    CodigoBAH = "BAH-002",
                    Nombre = "Bobinas de plástico impermeable 6x50m",
                    Categoria = "Techo",
                    UnidadMedida = "Rollos",
                    StockDisponible = 300,
                    StockMinimo = 20,
                    DistritoId = 1
                },
                new RecursoAlmacen
                {
                    RecursoId = 3,
                    CodigoBAH = "BAH-003",
                    Nombre = "Motobomba de achique autocebante 4\"",
                    Categoria = "Equipamiento",
                    UnidadMedida = "Equipos",
                    StockDisponible = 15,
                    StockMinimo = 3,
                    DistritoId = 1
                },
                new RecursoAlmacen
                {
                    RecursoId = 4,
                    CodigoBAH = "BAH-004",
                    Nombre = "Calaminas galvanizadas 1.83x0.83m",
                    Categoria = "Techo",
                    UnidadMedida = "Planchas",
                    StockDisponible = 5000,
                    StockMinimo = 200,
                    DistritoId = 1
                },
                new RecursoAlmacen
                {
                    RecursoId = 5,
                    CodigoBAH = "BAH-005",
                    Nombre = "Kit de víveres no perecibles (ración 3 días)",
                    Categoria = "Alimentos",
                    UnidadMedida = "Kits",
                    StockDisponible = 2000,
                    StockMinimo = 100,
                    DistritoId = 1
                },
                new RecursoAlmacen
                {
                    RecursoId = 6,
                    CodigoBAH = "BAH-006",
                    Nombre = "Camas plegables de lona para albergues",
                    Categoria = "Abrigo",
                    UnidadMedida = "Unidades",
                    StockDisponible = 120,
                    StockMinimo = 25,
                    DistritoId = 5
                }
            );

            // ── 3. Incidentes Base de Prueba (Histórico Inicial) ──────────────
            modelBuilder.Entity<IncidenteEmergencia>().HasData(
                new IncidenteEmergencia
                {
                    IncidenteId = 1,
                    CodigoIncidente = "ALT-2026-0001",
                    DniCiudadano = "71234567",
                    NombreCiudadano = "Juan Carlos Pérez",
                    Telefono = "944112233",
                    DistritoId = 1,
                    SectorCritico = "Sector Río Seco - Quebrada San Ildefonso",
                    TipoEvento = "Desborde Quebrada",
                    FamiliasAfectadas = 18,
                    Descripcion = "Drenaje pluvial colapsado por barro acumulado tras lluvias en la parte alta. Afectación a viviendas contiguas.",
                    DireccionReferencia = "Av. Sánchez Carrión cuadra 12, El Porvenir",
                    Severidad = "Crítico",
                    Estado = "Constatado",
                    FechaRegistro = new DateTime(2026, 9, 18, 14, 30, 0, DateTimeKind.Utc)
                },
                new IncidenteEmergencia
                {
                    IncidenteId = 2,
                    CodigoIncidente = "ALT-2026-0002",
                    DniCiudadano = "40987654",
                    NombreCiudadano = "María Elena Rojas",
                    Telefono = "988776655",
                    DistritoId = 4,
                    SectorCritico = "Entrada principal a El Trópico",
                    TipoEvento = "Inundación Pluvial",
                    FamiliasAfectadas = 5,
                    Descripcion = "Escorrentía superficial ingresando a predios en el margen de la carretera.",
                    DireccionReferencia = "Entrada principal a El Trópico, Huanchaco",
                    Severidad = "Moderado",
                    Estado = "Reportado",
                    FechaRegistro = new DateTime(2026, 9, 18, 18, 15, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}