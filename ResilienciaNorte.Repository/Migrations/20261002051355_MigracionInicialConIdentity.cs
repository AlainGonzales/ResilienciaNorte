using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResilienciaNorte.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicialConIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditoriaSistema",
                columns: table => new
                {
                    AuditoriaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RolUsuario = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Operacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IpCliente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaHoraUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaSistema", x => x.AuditoriaId);
                });

            migrationBuilder.CreateTable(
                name: "Distritos",
                columns: table => new
                {
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NivelRiesgo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UbicacionCOEL = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.DistritoId);
                });

            migrationBuilder.CreateTable(
                name: "VerificacionesOtpCiudadano",
                columns: table => new
                {
                    OtpId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DniCiudadano = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CodigoIncidente = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PinHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntentosFallidos = table.Column<int>(type: "int", nullable: false),
                    FueUtilizado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificacionesOtpCiudadano", x => x.OtpId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Dni = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NivelJurisdiccion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentesEmergencia",
                columns: table => new
                {
                    IncidenteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoIncidente = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DniCiudadano = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    NombreCiudadano = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SectorCritico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FamiliasAfectadas = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DireccionReferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Severidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Moderado"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Reportado"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentesEmergencia", x => x.IncidenteId);
                    table.ForeignKey(
                        name: "FK_IncidentesEmergencia_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecursosAlmacen",
                columns: table => new
                {
                    RecursoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoBAH = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StockDisponible = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosAlmacen", x => x.RecursoId);
                    table.ForeignKey(
                        name: "FK_RecursosAlmacen_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesAtencion",
                columns: table => new
                {
                    OrdenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoOrden = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IncidenteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAnalistaId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioCoordinadorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Aprobada"),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesAtencion", x => x.OrdenId);
                    table.ForeignKey(
                        name: "FK_OrdenesAtencion_IncidentesEmergencia_IncidenteId",
                        column: x => x.IncidenteId,
                        principalTable: "IncidentesEmergencia",
                        principalColumn: "IncidenteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosAlmacen",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ConceptoMovimiento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentoReferencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntidadOrigenDestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    StockAnterior = table.Column<int>(type: "int", nullable: false),
                    StockPosterior = table.Column<int>(type: "int", nullable: false),
                    UsuarioResponsableId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosAlmacen", x => x.MovimientoId);
                    table.ForeignKey(
                        name: "FK_MovimientosAlmacen_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosAlmacen_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesReabastecimiento",
                columns: table => new
                {
                    SolicitudId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoSolicitud = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistritoOrigenId = table.Column<int>(type: "int", nullable: false),
                    NivelDestino = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaAtencion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAprobadorId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesReabastecimiento", x => x.SolicitudId);
                    table.ForeignKey(
                        name: "FK_SolicitudesReabastecimiento_Distritos_DistritoOrigenId",
                        column: x => x.DistritoOrigenId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolicitudesReabastecimiento_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesOrden",
                columns: table => new
                {
                    DetalleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenId = table.Column<int>(type: "int", nullable: false),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    CantidadEntregada = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrden", x => x.DetalleId);
                    table.ForeignKey(
                        name: "FK_DetallesOrden_OrdenesAtencion_OrdenId",
                        column: x => x.OrdenId,
                        principalTable: "OrdenesAtencion",
                        principalColumn: "OrdenId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesOrden_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Distritos",
                columns: new[] { "DistritoId", "Activo", "NivelRiesgo", "Nombre", "UbicacionCOEL" },
                values: new object[,]
                {
                    { 1, true, "Alto", "El Porvenir", "Av. Sánchez Carrión 1200" },
                    { 2, true, "Alto", "Florencia de Mora", "Calle 20 de Setiembre" },
                    { 3, true, "Medio", "Huanchaco", "Av. Víctor Larco Herrera" },
                    { 4, true, "Medio", "La Esperanza", "Plaza de Armas La Esperanza" },
                    { 5, true, "Alto", "Laredo", "Riberas cuenca Río Moche" },
                    { 6, true, "Medio", "Moche", "Campiña de Moche" },
                    { 7, true, "Alto", "Poroto", "Sector Quebradas Altas" },
                    { 8, true, "Bajo", "Salaverry", "Zona Portuaria" },
                    { 9, true, "Alto", "Simbal", "Cuenca Alta Río Moche" },
                    { 10, true, "Bajo", "Trujillo", "COEP - Palacio Municipal" },
                    { 11, true, "Alto", "Víctor Larco Herrera", "Buenos Aires Sur" }
                });

            migrationBuilder.InsertData(
                table: "IncidentesEmergencia",
                columns: new[] { "IncidenteId", "CodigoIncidente", "Descripcion", "DireccionReferencia", "DistritoId", "DniCiudadano", "Estado", "FamiliasAfectadas", "FechaRegistro", "NombreCiudadano", "SectorCritico", "Severidad", "Telefono", "TipoEvento" },
                values: new object[,]
                {
                    { 1, "ALT-2026-0001", "Drenaje pluvial colapsado por barro acumulado tras lluvias en la parte alta. Afectación a viviendas contiguas.", "Av. Sánchez Carrión cuadra 12, El Porvenir", 1, "71234567", "Constatado", 18, new DateTime(2026, 9, 18, 14, 30, 0, 0, DateTimeKind.Utc), "Juan Carlos Pérez", "Sector Río Seco - Quebrada San Ildefonso", "Crítico", "944112233", "Desborde Quebrada" },
                    { 2, "ALT-2026-0002", "Escorrentía superficial ingresando a predios en el margen de la carretera.", "Entrada principal a El Trópico, Huanchaco", 4, "40987654", "Reportado", 5, new DateTime(2026, 9, 18, 18, 15, 0, 0, DateTimeKind.Utc), "María Elena Rojas", "Entrada principal a El Trópico", "Moderado", "988776655", "Inundación Pluvial" }
                });

            migrationBuilder.InsertData(
                table: "RecursosAlmacen",
                columns: new[] { "RecursoId", "Categoria", "CodigoBAH", "DistritoId", "Nombre", "StockDisponible", "StockMinimo", "UnidadMedida" },
                values: new object[,]
                {
                    { 1, "Defensa Ribereña", "BAH-001", 1, "Sacos terreros de polipropileno", 15000, 500, "Unidades" },
                    { 2, "Techo", "BAH-002", 1, "Bobinas de plástico impermeable 6x50m", 300, 20, "Rollos" },
                    { 3, "Equipamiento", "BAH-003", 1, "Motobomba de achique autocebante 4\"", 15, 3, "Equipos" },
                    { 4, "Techo", "BAH-004", 1, "Calaminas galvanizadas 1.83x0.83m", 5000, 200, "Planchas" },
                    { 5, "Alimentos", "BAH-005", 1, "Kit de víveres no perecibles (ración 3 días)", 2000, 100, "Kits" },
                    { 6, "Abrigo", "BAH-006", 5, "Camas plegables de lona para albergues", 120, 25, "Unidades" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DistritoId",
                table: "AspNetUsers",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrden_OrdenId",
                table: "DetallesOrden",
                column: "OrdenId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrden_RecursoId",
                table: "DetallesOrden",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentesEmergencia_CodigoIncidente",
                table: "IncidentesEmergencia",
                column: "CodigoIncidente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentesEmergencia_DistritoId",
                table: "IncidentesEmergencia",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosAlmacen_DistritoId",
                table: "MovimientosAlmacen",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosAlmacen_RecursoId",
                table: "MovimientosAlmacen",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesAtencion_CodigoOrden",
                table: "OrdenesAtencion",
                column: "CodigoOrden",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesAtencion_IncidenteId",
                table: "OrdenesAtencion",
                column: "IncidenteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecursosAlmacen_CodigoBAH",
                table: "RecursosAlmacen",
                column: "CodigoBAH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecursosAlmacen_DistritoId",
                table: "RecursosAlmacen",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesReabastecimiento_CodigoSolicitud",
                table: "SolicitudesReabastecimiento",
                column: "CodigoSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesReabastecimiento_DistritoOrigenId",
                table: "SolicitudesReabastecimiento",
                column: "DistritoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesReabastecimiento_RecursoId",
                table: "SolicitudesReabastecimiento",
                column: "RecursoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditoriaSistema");

            migrationBuilder.DropTable(
                name: "DetallesOrden");

            migrationBuilder.DropTable(
                name: "MovimientosAlmacen");

            migrationBuilder.DropTable(
                name: "SolicitudesReabastecimiento");

            migrationBuilder.DropTable(
                name: "VerificacionesOtpCiudadano");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OrdenesAtencion");

            migrationBuilder.DropTable(
                name: "RecursosAlmacen");

            migrationBuilder.DropTable(
                name: "IncidentesEmergencia");

            migrationBuilder.DropTable(
                name: "Distritos");
        }
    }
}
