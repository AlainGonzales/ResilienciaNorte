using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResilienciaNorte.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InicialConsolidada : Migration
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
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.DistritoId);
                });

            migrationBuilder.CreateTable(
                name: "Provincias",
                columns: table => new
                {
                    ProvinciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provincias", x => x.ProvinciaId);
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
                    UnidadMedida = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosAlmacen", x => x.RecursoId);
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
                name: "Incidencias",
                columns: table => new
                {
                    IncidenciaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoEvento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Abierta"),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UsuarioEvaluadorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ObservacionResolucion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencias", x => x.IncidenciaId);
                    table.ForeignKey(
                        name: "FK_Incidencias_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Almacenes",
                columns: table => new
                {
                    AlmacenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: true),
                    ProvinciaId = table.Column<int>(type: "int", nullable: true),
                    AlmacenPadreId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almacenes", x => x.AlmacenId);
                    table.ForeignKey(
                        name: "FK_Almacenes_Almacenes_AlmacenPadreId",
                        column: x => x.AlmacenPadreId,
                        principalTable: "Almacenes",
                        principalColumn: "AlmacenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Almacenes_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "DistritoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Almacenes_Provincias_ProvinciaId",
                        column: x => x.ProvinciaId,
                        principalTable: "Provincias",
                        principalColumn: "ProvinciaId",
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
                name: "FichasEdan",
                columns: table => new
                {
                    FichaEdanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidenciaId = table.Column<int>(type: "int", nullable: false),
                    FamiliasAfectadas = table.Column<int>(type: "int", nullable: false),
                    ViviendasAfectadas = table.Column<int>(type: "int", nullable: false),
                    PersonasAfectadas = table.Column<int>(type: "int", nullable: false),
                    NecesidadesUrgentes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaVisita = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioEvaluadorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasEdan", x => x.FichaEdanId);
                    table.ForeignKey(
                        name: "FK_FichasEdan_Incidencias_IncidenciaId",
                        column: x => x.IncidenciaId,
                        principalTable: "Incidencias",
                        principalColumn: "IncidenciaId",
                        onDelete: ReferentialAction.Cascade);
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
                    IncidenciaId = table.Column<int>(type: "int", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_IncidentesEmergencia_Incidencias_IncidenciaId",
                        column: x => x.IncidenciaId,
                        principalTable: "Incidencias",
                        principalColumn: "IncidenciaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExistenciasAlmacen",
                columns: table => new
                {
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    StockDisponible = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExistenciasAlmacen", x => new { x.AlmacenId, x.RecursoId });
                    table.ForeignKey(
                        name: "FK_ExistenciasAlmacen_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "AlmacenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExistenciasAlmacen_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosAlmacen",
                columns: table => new
                {
                    MovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_MovimientosAlmacen_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "AlmacenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosAlmacen_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    SolicitudId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoSolicitud = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlmacenSolicitanteId = table.Column<int>(type: "int", nullable: false),
                    AlmacenProveedorId = table.Column<int>(type: "int", nullable: true),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    CantidadSolicitada = table.Column<int>(type: "int", nullable: false),
                    Justificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaAtencion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioAprobadorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncidenciaId = table.Column<int>(type: "int", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.SolicitudId);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Almacenes_AlmacenProveedorId",
                        column: x => x.AlmacenProveedorId,
                        principalTable: "Almacenes",
                        principalColumn: "AlmacenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Almacenes_AlmacenSolicitanteId",
                        column: x => x.AlmacenSolicitanteId,
                        principalTable: "Almacenes",
                        principalColumn: "AlmacenId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Incidencias_IncidenciaId",
                        column: x => x.IncidenciaId,
                        principalTable: "Incidencias",
                        principalColumn: "IncidenciaId");
                    table.ForeignKey(
                        name: "FK_Solicitudes_RecursosAlmacen_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "RecursosAlmacen",
                        principalColumn: "RecursoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Almacenes",
                columns: new[] { "AlmacenId", "AlmacenPadreId", "DistritoId", "Nivel", "Nombre", "ProvinciaId" },
                values: new object[] { 1, null, null, "Regional", "Almacén Regional La Libertad", null });

            migrationBuilder.InsertData(
                table: "Distritos",
                columns: new[] { "DistritoId", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "El Porvenir" },
                    { 2, true, "Florencia de Mora" },
                    { 3, true, "Huanchaco" },
                    { 4, true, "La Esperanza" },
                    { 5, true, "Laredo" },
                    { 6, true, "Moche" },
                    { 7, true, "Poroto" },
                    { 8, true, "Salaverry" },
                    { 9, true, "Simbal" },
                    { 10, true, "Trujillo" },
                    { 11, true, "Víctor Larco Herrera" }
                });

            migrationBuilder.InsertData(
                table: "Provincias",
                columns: new[] { "ProvinciaId", "Nombre" },
                values: new object[,]
                {
                    { 1, "Trujillo" },
                    { 2, "Ascope" },
                    { 3, "Bolívar" },
                    { 4, "Chepén" },
                    { 5, "Julcán" },
                    { 6, "Otuzco" },
                    { 7, "Pacasmayo" },
                    { 8, "Pataz" },
                    { 9, "Sánchez Carrión" },
                    { 10, "Santiago de Chuco" },
                    { 11, "Gran Chimú" },
                    { 12, "Virú" }
                });

            migrationBuilder.InsertData(
                table: "RecursosAlmacen",
                columns: new[] { "RecursoId", "Categoria", "CodigoBAH", "Nombre", "UnidadMedida" },
                values: new object[,]
                {
                    { 1, "Defensa Ribereña", "BAH-001", "Sacos terreros de polipropileno", "Unidades" },
                    { 2, "Techo", "BAH-002", "Bobinas de plástico impermeable 6x50m", "Rollos" },
                    { 3, "Equipamiento", "BAH-003", "Motobomba de achique autocebante 4\"", "Equipos" },
                    { 4, "Techo", "BAH-004", "Calaminas galvanizadas 1.83x0.83m", "Planchas" },
                    { 5, "Alimentos", "BAH-005", "Kit de víveres no perecibles (ración 3 días)", "Kits" },
                    { 6, "Abrigo", "BAH-006", "Camas plegables de lona para albergues", "Unidades" }
                });

            migrationBuilder.InsertData(
                table: "Almacenes",
                columns: new[] { "AlmacenId", "AlmacenPadreId", "DistritoId", "Nivel", "Nombre", "ProvinciaId" },
                values: new object[,]
                {
                    { 2, 1, null, "Provincial", "Almacén Provincial Trujillo", 1 },
                    { 14, 1, null, "Provincial", "Almacén Provincial Ascope", 2 },
                    { 15, 1, null, "Provincial", "Almacén Provincial Bolívar", 3 },
                    { 16, 1, null, "Provincial", "Almacén Provincial Chepén", 4 },
                    { 17, 1, null, "Provincial", "Almacén Provincial Julcán", 5 },
                    { 18, 1, null, "Provincial", "Almacén Provincial Otuzco", 6 },
                    { 19, 1, null, "Provincial", "Almacén Provincial Pacasmayo", 7 },
                    { 20, 1, null, "Provincial", "Almacén Provincial Pataz", 8 },
                    { 21, 1, null, "Provincial", "Almacén Provincial Sánchez Carrión", 9 },
                    { 22, 1, null, "Provincial", "Almacén Provincial Santiago de Chuco", 10 },
                    { 23, 1, null, "Provincial", "Almacén Provincial Gran Chimú", 11 },
                    { 24, 1, null, "Provincial", "Almacén Provincial Virú", 12 }
                });

            migrationBuilder.InsertData(
                table: "ExistenciasAlmacen",
                columns: new[] { "AlmacenId", "RecursoId", "StockDisponible", "StockMinimo" },
                values: new object[,]
                {
                    { 1, 1, 200000, 500 },
                    { 1, 2, 10000, 20 },
                    { 1, 3, 300, 3 },
                    { 1, 4, 100000, 200 },
                    { 1, 5, 50000, 100 },
                    { 1, 6, 3000, 25 }
                });

            migrationBuilder.InsertData(
                table: "IncidentesEmergencia",
                columns: new[] { "IncidenteId", "CodigoIncidente", "Descripcion", "DireccionReferencia", "DistritoId", "DniCiudadano", "Estado", "FamiliasAfectadas", "FechaRegistro", "IncidenciaId", "NombreCiudadano", "SectorCritico", "Severidad", "Telefono", "TipoEvento" },
                values: new object[,]
                {
                    { 1, "ALT-2026-0001", "Drenaje pluvial colapsado por barro acumulado tras lluvias en la parte alta. Afectación a viviendas contiguas.", "Av. Sánchez Carrión cuadra 12, El Porvenir", 1, "71234567", "Constatado", 18, new DateTime(2026, 9, 18, 14, 30, 0, 0, DateTimeKind.Utc), null, "Juan Carlos Pérez", "Sector Río Seco - Quebrada San Ildefonso", "Crítico", "944112233", "Desborde Quebrada" },
                    { 2, "ALT-2026-0002", "Escorrentía superficial ingresando a predios en el margen de la carretera.", "Entrada principal a El Trópico, Huanchaco", 4, "40987654", "Reportado", 5, new DateTime(2026, 9, 18, 18, 15, 0, 0, DateTimeKind.Utc), null, "María Elena Rojas", "Entrada principal a El Trópico", "Moderado", "988776655", "Inundación Pluvial" },
                    { 3, "ALT-2026-0003", "Reporte de prueba para el monitor de reportes.", "Quebrada San Ildefonso - tramo alto", 1, "70000000", "Reportado", 14, new DateTime(2026, 10, 6, 8, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 1", "Quebrada San Ildefonso - tramo alto", "Grave", "900000000", "Desborde Quebrada" },
                    { 4, "ALT-2026-0004", "Reporte de prueba para el monitor de reportes.", "Av. Sánchez Carrión cuadra 15", 1, "70000001", "Reportado", 6, new DateTime(2026, 10, 6, 9, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 2", "Av. Sánchez Carrión cuadra 15", "Moderado", "900000001", "Inundación Pluvial" },
                    { 5, "ALT-2026-0005", "Reporte de prueba para el monitor de reportes.", "Ribera del río Moche - Laredo", 5, "70000002", "Reportado", 30, new DateTime(2026, 10, 6, 10, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 3", "Ribera del río Moche - Laredo", "Crítico", "900000002", "Desborde Quebrada" },
                    { 6, "ALT-2026-0006", "Reporte de prueba para el monitor de reportes.", "Canal de riego Laredo - sector norte", 5, "70000003", "Reportado", 12, new DateTime(2026, 10, 6, 11, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 4", "Canal de riego Laredo - sector norte", "Grave", "900000003", "Desborde Quebrada" },
                    { 7, "ALT-2026-0007", "Reporte de prueba para el monitor de reportes.", "Calle Los Álamos - Laredo", 5, "70000004", "Reportado", 5, new DateTime(2026, 10, 6, 12, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 5", "Calle Los Álamos - Laredo", "Moderado", "900000004", "Inundación Pluvial" },
                    { 8, "ALT-2026-0008", "Reporte de prueba para el monitor de reportes.", "Calle 20 de Setiembre - Florencia de Mora", 2, "70000005", "Reportado", 9, new DateTime(2026, 10, 6, 13, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 6", "Calle 20 de Setiembre - Florencia de Mora", "Grave", "900000005", "Inundación Pluvial" },
                    { 9, "ALT-2026-0009", "Reporte de prueba para el monitor de reportes.", "Canal Florencia de Mora - acceso", 2, "70000006", "Reportado", 4, new DateTime(2026, 10, 6, 14, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 7", "Canal Florencia de Mora - acceso", "Moderado", "900000006", "Desborde Quebrada" },
                    { 10, "ALT-2026-0010", "Reporte de prueba para el monitor de reportes.", "Cuenca alta Simbal - puente", 9, "70000007", "Constatado", 25, new DateTime(2026, 10, 6, 15, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 8", "Cuenca alta Simbal - puente", "Crítico", "900000007", "Desborde Quebrada" },
                    { 11, "ALT-2026-0011", "Reporte de prueba para el monitor de reportes.", "Quebrada El Tablón - Simbal", 9, "70000008", "Reportado", 10, new DateTime(2026, 10, 6, 16, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 9", "Quebrada El Tablón - Simbal", "Grave", "900000008", "Desborde Quebrada" },
                    { 12, "ALT-2026-0012", "Reporte de prueba para el monitor de reportes.", "Camino rural Simbal - sector Huaca", 9, "70000009", "Reportado", 3, new DateTime(2026, 10, 6, 17, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 10", "Camino rural Simbal - sector Huaca", "Moderado", "900000009", "Inundación Pluvial" },
                    { 13, "ALT-2026-0013", "Reporte de prueba para el monitor de reportes.", "Quebrada Altas Poroto", 7, "70000010", "Reportado", 11, new DateTime(2026, 10, 6, 18, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 11", "Quebrada Altas Poroto", "Grave", "900000010", "Desborde Quebrada" },
                    { 14, "ALT-2026-0014", "Reporte de prueba para el monitor de reportes.", "Carretera Poroto - Quebrada Honda", 7, "70000011", "Reportado", 4, new DateTime(2026, 10, 6, 19, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 12", "Carretera Poroto - Quebrada Honda", "Moderado", "900000011", "Desborde Quebrada" },
                    { 15, "ALT-2026-0015", "Reporte de prueba para el monitor de reportes.", "Puente Poroto - sector agrícola", 7, "70000012", "Desestimado", 6, new DateTime(2026, 10, 6, 20, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 13", "Puente Poroto - sector agrícola", "Moderado", "900000012", "Inundación Pluvial" },
                    { 16, "ALT-2026-0016", "Reporte de prueba para el monitor de reportes.", "Centro histórico Trujillo - Jr. Pizarro", 10, "70000013", "Reportado", 7, new DateTime(2026, 10, 6, 21, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 14", "Centro histórico Trujillo - Jr. Pizarro", "Grave", "900000013", "Colapso de Desagüe" },
                    { 17, "ALT-2026-0017", "Reporte de prueba para el monitor de reportes.", "Av. Mansiche - Trujillo", 10, "70000014", "Reportado", 5, new DateTime(2026, 10, 6, 22, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 15", "Av. Mansiche - Trujillo", "Moderado", "900000014", "Colapso de Desagüe" },
                    { 18, "ALT-2026-0018", "Reporte de prueba para el monitor de reportes.", "Jr. Bolívar - colector principal", 10, "70000015", "Constatado", 8, new DateTime(2026, 10, 6, 23, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 16", "Jr. Bolívar - colector principal", "Grave", "900000015", "Colapso de Desagüe" },
                    { 19, "ALT-2026-0019", "Reporte de prueba para el monitor de reportes.", "Sector El Milagro - quebrada homónima", 4, "70000016", "Constatado", 12, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 17", "Sector El Milagro - quebrada homónima", "Grave", "900000016", "Desborde Quebrada" },
                    { 20, "ALT-2026-0020", "Reporte de prueba para el monitor de reportes.", "Entrada principal a El Trópico", 4, "70000017", "Reportado", 5, new DateTime(2026, 10, 7, 1, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 18", "Entrada principal a El Trópico", "Moderado", "900000017", "Inundación Pluvial" },
                    { 21, "ALT-2026-0021", "Reporte de prueba para el monitor de reportes.", "Av. Los Ángeles - La Esperanza", 4, "70000018", "Reportado", 4, new DateTime(2026, 10, 7, 2, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 19", "Av. Los Ángeles - La Esperanza", "Moderado", "900000018", "Colapso de Desagüe" },
                    { 22, "ALT-2026-0022", "Reporte de prueba para el monitor de reportes.", "Buenos Aires Sur - anegamiento", 11, "70000019", "Reportado", 9, new DateTime(2026, 10, 7, 3, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 20", "Buenos Aires Sur - anegamiento", "Grave", "900000019", "Inundación Pluvial" },
                    { 23, "ALT-2026-0023", "Reporte de prueba para el monitor de reportes.", "Calle Las Palmeras - Víctor Larco", 11, "70000020", "Reportado", 4, new DateTime(2026, 10, 7, 4, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 21", "Calle Las Palmeras - Víctor Larco", "Moderado", "900000020", "Colapso de Desagüe" },
                    { 24, "ALT-2026-0024", "Reporte de prueba para el monitor de reportes.", "Quebrada El León - Huanchaco", 3, "70000021", "Reportado", 3, new DateTime(2026, 10, 7, 5, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 22", "Quebrada El León - Huanchaco", "Moderado", "900000021", "Desborde Quebrada" },
                    { 25, "ALT-2026-0025", "Reporte de prueba para el monitor de reportes.", "Campiña de Moche - acequia", 6, "70000022", "Reportado", 6, new DateTime(2026, 10, 7, 6, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 23", "Campiña de Moche - acequia", "Moderado", "900000022", "Inundación Pluvial" },
                    { 26, "ALT-2026-0026", "Reporte de prueba para el monitor de reportes.", "Zona portuaria Salaverry", 8, "70000023", "Reportado", 3, new DateTime(2026, 10, 7, 7, 0, 0, 0, DateTimeKind.Utc), null, "Vecino de prueba 24", "Zona portuaria Salaverry", "Moderado", "900000023", "Inundación Pluvial" },
                    { 28, "ALT-2026-0028", "Reporte de demostración: casuística de agrupamiento.", "Quebrada San Ildefonso - tramo bajo", 1, "80000000", "Reportado", 8, new DateTime(2026, 10, 7, 14, 0, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 1", "Quebrada San Ildefonso - tramo bajo", "Grave", "800000000", "Desborde Quebrada" },
                    { 29, "ALT-2026-0029", "Reporte de demostración: casuística de agrupamiento.", "Quebrada San Ildefonso - puente peatonal", 1, "80000001", "Reportado", 11, new DateTime(2026, 10, 7, 15, 30, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 2", "Quebrada San Ildefonso - puente peatonal", "Grave", "800000001", "Desborde Quebrada" },
                    { 30, "ALT-2026-0030", "Reporte de demostración: casuística de agrupamiento.", "Quebrada San Ildefonso - cruce Av. Industrial", 1, "80000002", "Reportado", 5, new DateTime(2026, 10, 7, 17, 10, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 3", "Quebrada San Ildefonso - cruce Av. Industrial", "Moderado", "800000002", "Desborde Quebrada" },
                    { 31, "ALT-2026-0031", "Reporte de demostración: casuística de agrupamiento.", "Jr. Los Pinos - El Porvenir", 1, "80000003", "Reportado", 4, new DateTime(2026, 10, 7, 20, 0, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 4", "Jr. Los Pinos - El Porvenir", "Moderado", "800000003", "Inundación Pluvial" },
                    { 32, "ALT-2026-0032", "Reporte de demostración: casuística de agrupamiento.", "Av. Túpac Amaru - El Porvenir", 1, "80000004", "Reportado", 6, new DateTime(2026, 10, 7, 22, 45, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 5", "Av. Túpac Amaru - El Porvenir", "Moderado", "800000004", "Inundación Pluvial" },
                    { 33, "ALT-2026-0033", "Reporte de demostración: casuística de agrupamiento.", "Av. Mansiche tramo El Porvenir", 1, "80000005", "Reportado", 3, new DateTime(2026, 10, 7, 23, 10, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 6", "Av. Mansiche tramo El Porvenir", "Moderado", "800000005", "Colapso de Desagüe" },
                    { 34, "ALT-2026-0034", "Reporte de demostración: casuística de agrupamiento.", "Calle Las Flores - El Porvenir", 1, "80000006", "Reportado", 2, new DateTime(2026, 10, 7, 8, 0, 0, 0, DateTimeKind.Utc), null, "Vecino casuística 7", "Calle Las Flores - El Porvenir", "Moderado", "800000006", "Desborde Quebrada" }
                });

            migrationBuilder.InsertData(
                table: "MovimientosAlmacen",
                columns: new[] { "MovimientoId", "AlmacenId", "Cantidad", "ConceptoMovimiento", "DocumentoReferencia", "EntidadOrigenDestino", "FechaHora", "RecursoId", "StockAnterior", "StockPosterior", "TipoMovimiento", "UsuarioResponsableId" },
                values: new object[,]
                {
                    { 13, 1, 200000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 200000, "ENTRADA", "sistema" },
                    { 14, 1, 10000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 10000, "ENTRADA", "sistema" },
                    { 15, 1, 300, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 300, "ENTRADA", "sistema" },
                    { 16, 1, 100000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 100000, "ENTRADA", "sistema" },
                    { 17, 1, 50000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 50000, "ENTRADA", "sistema" },
                    { 18, 1, 3000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 3000, "ENTRADA", "sistema" }
                });

            migrationBuilder.InsertData(
                table: "Almacenes",
                columns: new[] { "AlmacenId", "AlmacenPadreId", "DistritoId", "Nivel", "Nombre", "ProvinciaId" },
                values: new object[,]
                {
                    { 3, 2, 1, "Distrital", "Almacén Distrital El Porvenir", 1 },
                    { 4, 2, 2, "Distrital", "Almacén Distrital Florencia de Mora", 1 },
                    { 5, 2, 3, "Distrital", "Almacén Distrital Huanchaco", 1 },
                    { 6, 2, 4, "Distrital", "Almacén Distrital La Esperanza", 1 },
                    { 7, 2, 5, "Distrital", "Almacén Distrital Laredo", 1 },
                    { 8, 2, 6, "Distrital", "Almacén Distrital Moche", 1 },
                    { 9, 2, 7, "Distrital", "Almacén Distrital Poroto", 1 },
                    { 10, 2, 8, "Distrital", "Almacén Distrital Salaverry", 1 },
                    { 11, 2, 9, "Distrital", "Almacén Distrital Simbal", 1 },
                    { 12, 2, 10, "Distrital", "Almacén Distrital Trujillo", 1 },
                    { 13, 2, 11, "Distrital", "Almacén Distrital Víctor Larco Herrera", 1 }
                });

            migrationBuilder.InsertData(
                table: "ExistenciasAlmacen",
                columns: new[] { "AlmacenId", "RecursoId", "StockDisponible", "StockMinimo" },
                values: new object[,]
                {
                    { 2, 1, 50000, 500 },
                    { 2, 2, 2000, 20 },
                    { 2, 3, 60, 3 },
                    { 2, 4, 20000, 200 },
                    { 2, 5, 10000, 100 },
                    { 2, 6, 500, 25 }
                });

            migrationBuilder.InsertData(
                table: "MovimientosAlmacen",
                columns: new[] { "MovimientoId", "AlmacenId", "Cantidad", "ConceptoMovimiento", "DocumentoReferencia", "EntidadOrigenDestino", "FechaHora", "RecursoId", "StockAnterior", "StockPosterior", "TipoMovimiento", "UsuarioResponsableId" },
                values: new object[,]
                {
                    { 7, 2, 50000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 50000, "ENTRADA", "sistema" },
                    { 8, 2, 2000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 2000, "ENTRADA", "sistema" },
                    { 9, 2, 60, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 60, "ENTRADA", "sistema" },
                    { 10, 2, 20000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 20000, "ENTRADA", "sistema" },
                    { 11, 2, 10000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 10000, "ENTRADA", "sistema" },
                    { 12, 2, 500, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 500, "ENTRADA", "sistema" }
                });

            migrationBuilder.InsertData(
                table: "ExistenciasAlmacen",
                columns: new[] { "AlmacenId", "RecursoId", "StockDisponible", "StockMinimo" },
                values: new object[,]
                {
                    { 3, 1, 15000, 500 },
                    { 3, 2, 300, 20 },
                    { 3, 3, 15, 3 },
                    { 3, 4, 5000, 200 },
                    { 3, 5, 2000, 100 },
                    { 4, 1, 4000, 300 },
                    { 4, 2, 120, 20 },
                    { 4, 5, 800, 100 },
                    { 5, 1, 3500, 300 },
                    { 5, 4, 1200, 200 },
                    { 5, 6, 60, 25 },
                    { 6, 2, 80, 20 },
                    { 6, 3, 4, 3 },
                    { 6, 5, 500, 100 },
                    { 7, 1, 2000, 300 },
                    { 7, 2, 200, 20 },
                    { 7, 6, 120, 25 },
                    { 8, 1, 2500, 300 },
                    { 8, 4, 900, 200 },
                    { 8, 6, 40, 25 },
                    { 9, 2, 150, 20 },
                    { 9, 3, 2, 3 },
                    { 9, 5, 300, 100 },
                    { 10, 1, 1800, 300 },
                    { 10, 4, 600, 200 },
                    { 11, 2, 200, 20 },
                    { 11, 5, 150, 100 },
                    { 11, 6, 30, 25 },
                    { 12, 1, 6000, 300 },
                    { 12, 3, 8, 3 },
                    { 12, 4, 1500, 200 },
                    { 13, 1, 900, 300 },
                    { 13, 5, 400, 100 },
                    { 13, 6, 15, 25 }
                });

            migrationBuilder.InsertData(
                table: "MovimientosAlmacen",
                columns: new[] { "MovimientoId", "AlmacenId", "Cantidad", "ConceptoMovimiento", "DocumentoReferencia", "EntidadOrigenDestino", "FechaHora", "RecursoId", "StockAnterior", "StockPosterior", "TipoMovimiento", "UsuarioResponsableId" },
                values: new object[,]
                {
                    { 1, 3, 15000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 15000, "ENTRADA", "sistema" },
                    { 2, 3, 300, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 300, "ENTRADA", "sistema" },
                    { 3, 3, 15, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 15, "ENTRADA", "sistema" },
                    { 4, 3, 5000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 5000, "ENTRADA", "sistema" },
                    { 5, 3, 2000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 2000, "ENTRADA", "sistema" },
                    { 6, 7, 120, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 120, "ENTRADA", "sistema" },
                    { 19, 7, 2000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 2000, "ENTRADA", "sistema" },
                    { 20, 7, 200, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 200, "ENTRADA", "sistema" },
                    { 21, 4, 4000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 4000, "ENTRADA", "sistema" },
                    { 22, 4, 120, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 120, "ENTRADA", "sistema" },
                    { 23, 4, 800, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 800, "ENTRADA", "sistema" },
                    { 24, 5, 3500, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 3500, "ENTRADA", "sistema" },
                    { 25, 5, 1200, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 1200, "ENTRADA", "sistema" },
                    { 26, 5, 60, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 60, "ENTRADA", "sistema" },
                    { 27, 6, 80, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 80, "ENTRADA", "sistema" },
                    { 28, 6, 4, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 4, "ENTRADA", "sistema" },
                    { 29, 6, 500, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 500, "ENTRADA", "sistema" },
                    { 30, 8, 2500, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 2500, "ENTRADA", "sistema" },
                    { 31, 8, 900, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 900, "ENTRADA", "sistema" },
                    { 32, 8, 40, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 40, "ENTRADA", "sistema" },
                    { 33, 9, 150, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 150, "ENTRADA", "sistema" },
                    { 34, 9, 2, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 2, "ENTRADA", "sistema" },
                    { 35, 9, 300, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 300, "ENTRADA", "sistema" },
                    { 36, 10, 1800, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 1800, "ENTRADA", "sistema" },
                    { 37, 10, 600, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 600, "ENTRADA", "sistema" },
                    { 38, 11, 200, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 0, 200, "ENTRADA", "sistema" },
                    { 39, 11, 150, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 150, "ENTRADA", "sistema" },
                    { 40, 11, 30, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 30, "ENTRADA", "sistema" },
                    { 41, 12, 6000, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 6000, "ENTRADA", "sistema" },
                    { 42, 12, 8, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 0, 8, "ENTRADA", "sistema" },
                    { 43, 12, 1500, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, 0, 1500, "ENTRADA", "sistema" },
                    { 44, 13, 900, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 0, 900, "ENTRADA", "sistema" },
                    { 45, 13, 400, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, 0, 400, "ENTRADA", "sistema" },
                    { 46, 13, 15, "SaldoApertura", "APERTURA-2026", "Carga inicial del sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), 6, 0, 15, "ENTRADA", "sistema" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_AlmacenPadreId",
                table: "Almacenes",
                column: "AlmacenPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_DistritoId",
                table: "Almacenes",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_ProvinciaId",
                table: "Almacenes",
                column: "ProvinciaId");

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
                name: "IX_ExistenciasAlmacen_RecursoId",
                table: "ExistenciasAlmacen",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_FichasEdan_IncidenciaId",
                table: "FichasEdan",
                column: "IncidenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_Codigo",
                table: "Incidencias",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidencias_DistritoId",
                table: "Incidencias",
                column: "DistritoId");

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
                name: "IX_IncidentesEmergencia_IncidenciaId",
                table: "IncidentesEmergencia",
                column: "IncidenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosAlmacen_AlmacenId",
                table: "MovimientosAlmacen",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosAlmacen_RecursoId",
                table: "MovimientosAlmacen",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosAlmacen_CodigoBAH",
                table: "RecursosAlmacen",
                column: "CodigoBAH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_AlmacenProveedorId",
                table: "Solicitudes",
                column: "AlmacenProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_AlmacenSolicitanteId",
                table: "Solicitudes",
                column: "AlmacenSolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_CodigoSolicitud",
                table: "Solicitudes",
                column: "CodigoSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_IncidenciaId",
                table: "Solicitudes",
                column: "IncidenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_RecursoId",
                table: "Solicitudes",
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
                name: "ExistenciasAlmacen");

            migrationBuilder.DropTable(
                name: "FichasEdan");

            migrationBuilder.DropTable(
                name: "IncidentesEmergencia");

            migrationBuilder.DropTable(
                name: "MovimientosAlmacen");

            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "VerificacionesOtpCiudadano");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Almacenes");

            migrationBuilder.DropTable(
                name: "Incidencias");

            migrationBuilder.DropTable(
                name: "RecursosAlmacen");

            migrationBuilder.DropTable(
                name: "Provincias");

            migrationBuilder.DropTable(
                name: "Distritos");
        }
    }
}
