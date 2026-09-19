using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ResilienciaNorte.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Distritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ZonaVulnerablePrincipal = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PoblacionEstimada = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distritos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentesEmergencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoDesastre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NivelSeveridad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DireccionReferencia = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FamiliasAfectadas = table.Column<int>(type: "int", nullable: false),
                    FechaReporte = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentesEmergencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentesEmergencia_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecursosAlmacen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoBAH = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StockDisponible = table.Column<int>(type: "int", nullable: false),
                    StockMinimoSeguridad = table.Column<int>(type: "int", nullable: false),
                    DistritoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosAlmacen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecursosAlmacen_Distritos_DistritoId",
                        column: x => x.DistritoId,
                        principalTable: "Distritos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Distritos",
                columns: new[] { "Id", "Activo", "Nombre", "PoblacionEstimada", "ZonaVulnerablePrincipal" },
                values: new object[,]
                {
                    { 1, true, "El Porvenir", 195000, "Quebrada San Ildefonso / Río Seco" },
                    { 2, true, "Florencia de Mora", 42000, "Sectores bajos y red troncal de desagüe" },
                    { 3, true, "La Esperanza", 189000, "Parte alta / Quebrada El León" },
                    { 4, true, "Trujillo Centro Historico", 315000, "Av. España y microcuenca pluvial urbana" },
                    { 5, true, "Huanchaco", 68000, "El Trópico y desembocadura Quebrada El León" },
                    { 6, true, "Víctor Larco Herrera", 65000, "Sector Buenos Aires Sur / Inundaciones costeras" },
                    { 7, true, "Laredo", 37000, "Riberas de la cuenca del Río Moche" },
                    { 8, true, "Moche", 38000, "Campiña de Moche y zonas agrícolas ribereñas" }
                });

            migrationBuilder.InsertData(
                table: "IncidentesEmergencia",
                columns: new[] { "Id", "Descripcion", "DireccionReferencia", "DistritoId", "Estado", "FamiliasAfectadas", "FechaReporte", "NivelSeveridad", "TipoDesastre", "Titulo" },
                values: new object[,]
                {
                    { 1, "Drenaje pluvial colapsado por barro acumulado tras lluvias en la parte alta. Afectación a viviendas contiguas.", "Av. Sánchez Carrión cuadra 12, El Porvenir", 1, "En Evaluación", 18, new DateTime(2026, 9, 18, 14, 30, 0, 0, DateTimeKind.Unspecified), "Crítico", "Inundación Pluvial", "Acumulación pluvial crítica en Sector Río Seco" },
                    { 2, "Escorrentía superficial ingresando a predios en el margen de la carretera.", "Entrada principal a El Trópico, Huanchaco", 5, "Pendiente", 5, new DateTime(2026, 9, 18, 18, 15, 0, 0, DateTimeKind.Unspecified), "Moderado", "Inundación Pluvial", "Anegamiento de calzada principal en El Trópico" }
                });

            migrationBuilder.InsertData(
                table: "RecursosAlmacen",
                columns: new[] { "Id", "CodigoBAH", "DistritoId", "Nombre", "StockDisponible", "StockMinimoSeguridad", "UnidadMedida" },
                values: new object[,]
                {
                    { 1, "BAH-001", 1, "Sacos terreros de polipropileno", 5000, 1000, "Unidades" },
                    { 2, "BAH-002", 1, "Bobinas de plástico calibre pesado (200m)", 150, 30, "Rollos" },
                    { 3, "BAH-003", 4, "Motobomba de achique autocebante 4\"", 8, 2, "Equipos" },
                    { 4, "BAH-004", 3, "Carpas familiares de lona impermeable", 60, 15, "Unidades" },
                    { 5, "BAH-005", 5, "Bidones de agua potable 20L", 400, 100, "Unidades" },
                    { 6, "BAH-006", 6, "Camas plegables de lona para albergues", 120, 25, "Unidades" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentesEmergencia_DistritoId",
                table: "IncidentesEmergencia",
                column: "DistritoId");

            migrationBuilder.CreateIndex(
                name: "IX_RecursosAlmacen_DistritoId",
                table: "RecursosAlmacen",
                column: "DistritoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentesEmergencia");

            migrationBuilder.DropTable(
                name: "RecursosAlmacen");

            migrationBuilder.DropTable(
                name: "Distritos");
        }
    }
}
