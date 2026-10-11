using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Repository
{
    public static class DataSeeder
    {
        // ── 0. Sembrado Asíncrono de Roles e Identidades ──
        public static async Task InicializarRolesYUsuariosAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<UsuarioAplicacion> userManager)
        {
            // Catálogo Oficial de Roles
            string[] roles = new[]
            {
                "Administrador",
                "Coordinador",
                "Analista",
                "LogisticoAlmacen",
                "EvaluadorCampo",
                "Ciudadano"
            };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 1. Administrador General del Sistema
            string adminEmail = "admin@coep.gob.pe";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new UsuarioAplicacion
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Dni = "00000001",
                    NombreCompleto = "Administrador Provincial COEP",
                    NivelJurisdiccion = "Provincial",
                    DistritoId = 10,
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };
                if ((await userManager.CreateAsync(adminUser, "Admin2026*")).Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                }
            }

            // 2. Analista Distrital (Monitoreo de Quebradas)
            string analistaDistEmail = "analista.huanchaco@coel.gob.pe";
            if (await userManager.FindByEmailAsync(analistaDistEmail) == null)
            {
                var user = new UsuarioAplicacion
                {
                    UserName = analistaDistEmail,
                    Email = analistaDistEmail,
                    Dni = "10000004",
                    NombreCompleto = "Analista de Riesgo Huanchaco",
                    NivelJurisdiccion = "Distrital",
                    DistritoId = 3, // Huanchaco
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };
                if ((await userManager.CreateAsync(user, "Analista2026*")).Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Analista");
                }
            }

            // 3. Coordinador Distrital (COEL El Porvenir)
            string coordDistEmail = "coordinador.elporvenir@coel.gob.pe";
            if (await userManager.FindByEmailAsync(coordDistEmail) == null)
            {
                var user = new UsuarioAplicacion
                {
                    UserName = coordDistEmail,
                    Email = coordDistEmail,
                    Dni = "10000002",
                    NombreCompleto = "Coordinador COEL El Porvenir",
                    NivelJurisdiccion = "Distrital",
                    DistritoId = 1, // El Porvenir
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };
                if ((await userManager.CreateAsync(user, "Coord2026*")).Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Coordinador");
                }
            }

            // 4. Analista Regional (COER La Libertad)
            string analistaRegEmail = "analista.regional@coer.gob.pe";
            if (await userManager.FindByEmailAsync(analistaRegEmail) == null)
            {
                var user = new UsuarioAplicacion
                {
                    UserName = analistaRegEmail,
                    Email = analistaRegEmail,
                    Dni = "10000003",
                    NombreCompleto = "Especialista EDAN COER La Libertad",
                    NivelJurisdiccion = "Regional",
                    DistritoId = 10,
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };
                if ((await userManager.CreateAsync(user, "Analista2026*")).Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Analista");
                }
            }

            // 5. Coordinador Provincial (COEP Trujillo)
            string coordProvEmail = "coordinador.provincial@coep.gob.pe";
            if (await userManager.FindByEmailAsync(coordProvEmail) == null)
            {
                var user = new UsuarioAplicacion
                {
                    UserName = coordProvEmail,
                    Email = coordProvEmail,
                    Dni = "10000001",
                    NombreCompleto = "Ing. Coordinador COEP Trujillo",
                    NivelJurisdiccion = "Provincial",
                    DistritoId = 10, // Trujillo Centro
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };
                if ((await userManager.CreateAsync(user, "Coord2026*")).Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Coordinador");
                }
            }                        

            // 6. Evaluador de Campo (Evaluador de Campo EDAN)
            string evaluadorEmail = "evaluador.campo@coep.gob.pe";
            if (await userManager.FindByEmailAsync(evaluadorEmail) == null)
            {
                var evaluadorUser = new UsuarioAplicacion
                {
                    UserName = evaluadorEmail,
                    Email = evaluadorEmail,
                    Dni = "10000005",
                    NombreCompleto = "Evaluador de Campo El Porvenir 1",
                    NivelJurisdiccion = "Distrital",
                    DistritoId = 1, // El Porvenir
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };

                var resultado = await userManager.CreateAsync(evaluadorUser, "Evaluador2026*");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(evaluadorUser, "EvaluadorCampo");
                }
            }

            // 6b. Personal por distrito: cada analista y evaluador solo ve su distrito
            await CrearUsuarioSiNoExisteAsync(userManager, "analista.elporvenir@coel.gob.pe", "10000007",
                "Analista de Riesgo El Porvenir", "Distrital", 1, "Analista", "Analista2026*");
            await CrearUsuarioSiNoExisteAsync(userManager, "evaluador.elporvenir2@coep.gob.pe", "10000008",
                "Evaluador de Campo El Porvenir 2", "Distrital", 1, "EvaluadorCampo", "Evaluador2026*");
            await CrearUsuarioSiNoExisteAsync(userManager, "evaluador.huanchaco1@coep.gob.pe", "10000009",
                "Evaluador de Campo Huanchaco 1", "Distrital", 3, "EvaluadorCampo", "Evaluador2026*");

            // Logístico del almacén distrital de El Porvenir: despacha lo que aprueba su coordinador
            await CrearUsuarioSiNoExisteAsync(userManager, "almacen.elporvenir@coep.gob.pe", "10000011",
                "Logístico Almacén El Porvenir", "Distrital", 1, "LogisticoAlmacen", "Almacen2026*");

            // 7. Almacenero / Custodio BAH (Módulo Logístico)
            string logisticoEmail = "almacen.bah@coep.gob.pe";
            if (await userManager.FindByEmailAsync(logisticoEmail) == null)
            {
                var almaceneroUser = new UsuarioAplicacion
                {
                    UserName = logisticoEmail,
                    Email = logisticoEmail,
                    Dni = "10000006",
                    NombreCompleto = "Custodio Central Almacén BAH",
                    NivelJurisdiccion = "Provincial",
                    DistritoId = 10, // Trujillo Centro (Almacén Central)
                    Activo = true,
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.UtcNow
                };

                var resultado = await userManager.CreateAsync(almaceneroUser, "Almacen2026*");
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(almaceneroUser, "LogisticoAlmacen");
                }
            }
        }

        // Crea un usuario con su rol solo si el correo aún no existe
        private static async Task CrearUsuarioSiNoExisteAsync(
            UserManager<UsuarioAplicacion> userManager,
            string email, string dni, string nombre, string nivel, int distritoId, string rol, string password)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;

            var usuario = new UsuarioAplicacion
            {
                UserName = email,
                Email = email,
                Dni = dni,
                NombreCompleto = nombre,
                NivelJurisdiccion = nivel,
                DistritoId = distritoId,
                Activo = true,
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow
            };

            var resultado = await userManager.CreateAsync(usuario, password);
            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(usuario, rol);
            }
        }

        public static void SeedData(this ModelBuilder modelBuilder)
        {
            // ── 1. Catálogo Territorial - 11 Distritos Oficiales de la Provincia de Trujillo ──
            modelBuilder.Entity<Distrito>().HasData(
                new Distrito { DistritoId = 1, Nombre = "El Porvenir", Activo = true },
                new Distrito { DistritoId = 2, Nombre = "Florencia de Mora", Activo = true },
                new Distrito { DistritoId = 3, Nombre = "Huanchaco", Activo = true },
                new Distrito { DistritoId = 4, Nombre = "La Esperanza", Activo = true },
                new Distrito { DistritoId = 5, Nombre = "Laredo", Activo = true },
                new Distrito { DistritoId = 6, Nombre = "Moche", Activo = true },
                new Distrito { DistritoId = 7, Nombre = "Poroto", Activo = true },
                new Distrito { DistritoId = 8, Nombre = "Salaverry", Activo = true },
                new Distrito { DistritoId = 9, Nombre = "Simbal", Activo = true },
                new Distrito { DistritoId = 10, Nombre = "Trujillo", Activo = true },
                new Distrito { DistritoId = 11, Nombre = "Víctor Larco Herrera", Activo = true }
            );

            // ── 2. Catálogo de Bienes BAH (sin stock: el stock vive en ExistenciaAlmacen) ──
            modelBuilder.Entity<RecursoAlmacen>().HasData(
                new RecursoAlmacen { RecursoId = 1, CodigoBAH = "BAH-001", Nombre = "Sacos terreros de polipropileno", Categoria = "Defensa Ribereña", UnidadMedida = "Unidades" },
                new RecursoAlmacen { RecursoId = 2, CodigoBAH = "BAH-002", Nombre = "Bobinas de plástico impermeable 6x50m", Categoria = "Techo", UnidadMedida = "Rollos" },
                new RecursoAlmacen { RecursoId = 3, CodigoBAH = "BAH-003", Nombre = "Motobomba de achique autocebante 4\"", Categoria = "Equipamiento", UnidadMedida = "Equipos" },
                new RecursoAlmacen { RecursoId = 4, CodigoBAH = "BAH-004", Nombre = "Calaminas galvanizadas 1.83x0.83m", Categoria = "Techo", UnidadMedida = "Planchas" },
                new RecursoAlmacen { RecursoId = 5, CodigoBAH = "BAH-005", Nombre = "Kit de víveres no perecibles (ración 3 días)", Categoria = "Alimentos", UnidadMedida = "Kits" },
                new RecursoAlmacen { RecursoId = 6, CodigoBAH = "BAH-006", Nombre = "Camas plegables de lona para albergues", Categoria = "Abrigo", UnidadMedida = "Unidades" }
            );

            // ── 3. Provincias de La Libertad (Trujillo tiene distritos cargados; las demás quedan vacías) ──
            var nombresProvincias = new[]
            {
                "Trujillo", "Ascope", "Bolívar", "Chepén", "Julcán", "Otuzco", "Pacasmayo",
                "Pataz", "Sánchez Carrión", "Santiago de Chuco", "Gran Chimú", "Virú"
            };
            modelBuilder.Entity<Provincia>().HasData(
                nombresProvincias.Select((nombre, i) => new Provincia { ProvinciaId = i + 1, Nombre = nombre }).ToArray());

            // ── 4. Almacenes: Regional → Provincial → Distrital ──────────────────
            // Provincial: id 2 para Trujillo y 12 + ProvinciaId para las demás. Distrital: DistritoId + 2 (3 a 13).
            var nombresDistritos = new[]
            {
                "El Porvenir", "Florencia de Mora", "Huanchaco", "La Esperanza", "Laredo", "Moche",
                "Poroto", "Salaverry", "Simbal", "Trujillo", "Víctor Larco Herrera"
            };

            var almacenes = new List<Almacen>
            {
                new Almacen { AlmacenId = 1, Nombre = "Almacén Regional La Libertad", Nivel = "Regional" },
                new Almacen { AlmacenId = 2, Nombre = "Almacén Provincial Trujillo", Nivel = "Provincial", ProvinciaId = 1, AlmacenPadreId = 1 }
            };
            for (int p = 2; p <= nombresProvincias.Length; p++)
            {
                almacenes.Add(new Almacen
                {
                    AlmacenId = 12 + p,
                    Nombre = $"Almacén Provincial {nombresProvincias[p - 1]}",
                    Nivel = "Provincial",
                    ProvinciaId = p,
                    AlmacenPadreId = 1
                });
            }
            for (int i = 0; i < nombresDistritos.Length; i++)
            {
                almacenes.Add(new Almacen
                {
                    AlmacenId = i + 3,
                    Nombre = $"Almacén Distrital {nombresDistritos[i]}",
                    Nivel = "Distrital",
                    DistritoId = i + 1,
                    ProvinciaId = 1,
                    AlmacenPadreId = 2
                });
            }
            modelBuilder.Entity<Almacen>().HasData(almacenes.ToArray());

            // ── 5. Existencias iniciales por almacén ─────────────────────────────
            var existencias = new List<ExistenciaAlmacen>();
            void Existencia(int almacenId, int recursoId, int stock, int minimo) =>
                existencias.Add(new ExistenciaAlmacen { AlmacenId = almacenId, RecursoId = recursoId, StockDisponible = stock, StockMinimo = minimo });

            // Almacén distrital El Porvenir (3)
            Existencia(3, 1, 15000, 500);
            Existencia(3, 2, 300, 20);
            Existencia(3, 3, 15, 3);
            Existencia(3, 4, 5000, 200);
            Existencia(3, 5, 2000, 100);
            // Almacén distrital Laredo (7)
            Existencia(7, 6, 120, 25);
            // Almacén provincial Trujillo (2)
            Existencia(2, 1, 50000, 500);
            Existencia(2, 2, 2000, 20);
            Existencia(2, 3, 60, 3);
            Existencia(2, 4, 20000, 200);
            Existencia(2, 5, 10000, 100);
            Existencia(2, 6, 500, 25);
            // Almacén regional La Libertad (1)
            Existencia(1, 1, 200000, 500);
            Existencia(1, 2, 10000, 20);
            Existencia(1, 3, 300, 3);
            Existencia(1, 4, 100000, 200);
            Existencia(1, 5, 50000, 100);
            Existencia(1, 6, 3000, 25);
            // Almacenes distritales restantes: al menos dos bienes cada uno, con alguno bajo mínimo para ver los estados
            Existencia(7, 1, 2000, 300);      // Laredo
            Existencia(7, 2, 200, 20);
            Existencia(4, 1, 4000, 300);      // Florencia de Mora
            Existencia(4, 2, 120, 20);
            Existencia(4, 5, 800, 100);
            Existencia(5, 1, 3500, 300);      // Huanchaco
            Existencia(5, 4, 1200, 200);
            Existencia(5, 6, 60, 25);
            Existencia(6, 2, 80, 20);         // La Esperanza
            Existencia(6, 3, 4, 3);
            Existencia(6, 5, 500, 100);
            Existencia(8, 1, 2500, 300);      // Moche
            Existencia(8, 4, 900, 200);
            Existencia(8, 6, 40, 25);
            Existencia(9, 2, 150, 20);        // Poroto
            Existencia(9, 3, 2, 3);
            Existencia(9, 5, 300, 100);
            Existencia(10, 1, 1800, 300);     // Salaverry
            Existencia(10, 4, 600, 200);
            Existencia(11, 2, 200, 20);       // Simbal
            Existencia(11, 5, 150, 100);
            Existencia(11, 6, 30, 25);
            Existencia(12, 1, 6000, 300);     // Trujillo
            Existencia(12, 3, 8, 3);
            Existencia(12, 4, 1500, 200);
            Existencia(13, 1, 900, 300);      // Víctor Larco Herrera
            Existencia(13, 5, 400, 100);
            Existencia(13, 6, 15, 25);

            modelBuilder.Entity<ExistenciaAlmacen>().HasData(existencias.ToArray());

            // ── 6. Kardex de apertura: cada existencia nace con un movimiento de saldo inicial ──
            var aperturas = existencias
                .Select((e, i) => new MovimientoAlmacen
                {
                    MovimientoId = i + 1,
                    AlmacenId = e.AlmacenId,
                    RecursoId = e.RecursoId,
                    TipoMovimiento = "ENTRADA",
                    ConceptoMovimiento = "SaldoApertura",
                    DocumentoReferencia = "APERTURA-2026",
                    EntidadOrigenDestino = "Carga inicial del sistema",
                    Cantidad = e.StockDisponible,
                    StockAnterior = 0,
                    StockPosterior = e.StockDisponible,
                    UsuarioResponsableId = "sistema",
                    FechaHora = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
                })
                .ToArray();
            modelBuilder.Entity<MovimientoAlmacen>().HasData(aperturas);

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

            // Reportes de prueba para ver el semáforo de distritos en el monitor
            // Distribución referencial según el comportamiento pluvial de cada zona de la provincia
            // Casuística para demostrar "Sugerir grupo" en El Porvenir (mismo tipo y dentro de 4 h entre sí)
            var casuisticaDemo = new (DateTime Fecha, string Sector, string Tipo, string Severidad, int Familias)[]
            {
                (new DateTime(2026, 10, 7, 14, 0, 0, DateTimeKind.Utc), "Quebrada San Ildefonso - tramo bajo", "Desborde Quebrada", "Grave", 8),
                (new DateTime(2026, 10, 7, 15, 30, 0, DateTimeKind.Utc), "Quebrada San Ildefonso - puente peatonal", "Desborde Quebrada", "Grave", 11),
                (new DateTime(2026, 10, 7, 17, 10, 0, DateTimeKind.Utc), "Quebrada San Ildefonso - cruce Av. Industrial", "Desborde Quebrada", "Moderado", 5),
                (new DateTime(2026, 10, 7, 20, 0, 0, DateTimeKind.Utc), "Jr. Los Pinos - El Porvenir", "Inundación Pluvial", "Moderado", 4),
                (new DateTime(2026, 10, 7, 22, 45, 0, DateTimeKind.Utc), "Av. Túpac Amaru - El Porvenir", "Inundación Pluvial", "Moderado", 6),
                (new DateTime(2026, 10, 7, 23, 10, 0, DateTimeKind.Utc), "Av. Mansiche tramo El Porvenir", "Colapso de Desagüe", "Moderado", 3),
                (new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc), "Calle Las Flores - El Porvenir", "Desborde Quebrada", "Moderado", 2)
            };
            modelBuilder.Entity<IncidenteEmergencia>().HasData(
                casuisticaDemo.Select((d, i) => new IncidenteEmergencia
                {
                    IncidenteId = 28 + i,
                    CodigoIncidente = $"ALT-2026-{28 + i:D4}",
                    DniCiudadano = $"8{i:D7}",
                    NombreCiudadano = $"Vecino casuística {i + 1}",
                    Telefono = $"8{i:D8}",
                    DistritoId = 1, // El Porvenir
                    SectorCritico = d.Sector,
                    TipoEvento = d.Tipo,
                    FamiliasAfectadas = d.Familias,
                    Descripcion = "Reporte de demostración: casuística de agrupamiento.",
                    DireccionReferencia = d.Sector,
                    Severidad = d.Severidad,
                    Estado = "Reportado",
                    FechaRegistro = d.Fecha
                }).ToArray());

            var reportesDemo = new (int DistritoId, string Sector, string Tipo, string Severidad, int Familias, string Estado)[]
            {
                (1, "Quebrada San Ildefonso - tramo alto", "Desborde Quebrada", "Grave", 14, "Reportado"),
                (1, "Av. Sánchez Carrión cuadra 15", "Inundación Pluvial", "Moderado", 6, "Reportado"),
                (5, "Ribera del río Moche - Laredo", "Desborde Quebrada", "Crítico", 30, "Reportado"),
                (5, "Canal de riego Laredo - sector norte", "Desborde Quebrada", "Grave", 12, "Reportado"),
                (5, "Calle Los Álamos - Laredo", "Inundación Pluvial", "Moderado", 5, "Reportado"),
                (2, "Calle 20 de Setiembre - Florencia de Mora", "Inundación Pluvial", "Grave", 9, "Reportado"),
                (2, "Canal Florencia de Mora - acceso", "Desborde Quebrada", "Moderado", 4, "Reportado"),
                (9, "Cuenca alta Simbal - puente", "Desborde Quebrada", "Crítico", 25, "Constatado"),
                (9, "Quebrada El Tablón - Simbal", "Desborde Quebrada", "Grave", 10, "Reportado"),
                (9, "Camino rural Simbal - sector Huaca", "Inundación Pluvial", "Moderado", 3, "Reportado"),
                (7, "Quebrada Altas Poroto", "Desborde Quebrada", "Grave", 11, "Reportado"),
                (7, "Carretera Poroto - Quebrada Honda", "Desborde Quebrada", "Moderado", 4, "Reportado"),
                (7, "Puente Poroto - sector agrícola", "Inundación Pluvial", "Moderado", 6, "Desestimado"),
                (10, "Centro histórico Trujillo - Jr. Pizarro", "Colapso de Desagüe", "Grave", 7, "Reportado"),
                (10, "Av. Mansiche - Trujillo", "Colapso de Desagüe", "Moderado", 5, "Reportado"),
                (10, "Jr. Bolívar - colector principal", "Colapso de Desagüe", "Grave", 8, "Constatado"),
                (4, "Sector El Milagro - quebrada homónima", "Desborde Quebrada", "Grave", 12, "Constatado"),
                (4, "Entrada principal a El Trópico", "Inundación Pluvial", "Moderado", 5, "Reportado"),
                (4, "Av. Los Ángeles - La Esperanza", "Colapso de Desagüe", "Moderado", 4, "Reportado"),
                (11, "Buenos Aires Sur - anegamiento", "Inundación Pluvial", "Grave", 9, "Reportado"),
                (11, "Calle Las Palmeras - Víctor Larco", "Colapso de Desagüe", "Moderado", 4, "Reportado"),
                (3, "Quebrada El León - Huanchaco", "Desborde Quebrada", "Moderado", 3, "Reportado"),
                (6, "Campiña de Moche - acequia", "Inundación Pluvial", "Moderado", 6, "Reportado"),
                (8, "Zona portuaria Salaverry", "Inundación Pluvial", "Moderado", 3, "Reportado")
            };
            modelBuilder.Entity<IncidenteEmergencia>().HasData(
                reportesDemo.Select((d, i) => new IncidenteEmergencia
                {
                    IncidenteId = i + 3,
                    CodigoIncidente = $"ALT-2026-{i + 3:D4}",
                    DniCiudadano = $"7{i:D7}",
                    NombreCiudadano = $"Vecino de prueba {i + 1}",
                    Telefono = $"9{i:D8}",
                    DistritoId = d.DistritoId,
                    SectorCritico = d.Sector,
                    TipoEvento = d.Tipo,
                    FamiliasAfectadas = d.Familias,
                    Descripcion = "Reporte de prueba para el monitor de reportes.",
                    DireccionReferencia = d.Sector,
                    Severidad = d.Severidad,
                    Estado = d.Estado,
                    FechaRegistro = new DateTime(2026, 10, 6, 8, 0, 0, DateTimeKind.Utc).AddHours(i)
                }).ToArray());
        }
    }
}