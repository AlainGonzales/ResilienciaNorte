using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;

namespace ResilienciaNorte.Web.Controllers
{
    public class IncidentesController : Controller
    {
        private readonly IIncidenteService _incidenteService;
        private readonly IHubContext<EmergenciaHub> _hubContext;
        private readonly UserManager<UsuarioAplicacion> _userManager;

        public IncidentesController(IIncidenteService incidenteService, IHubContext<EmergenciaHub> hubContext, UserManager<UsuarioAplicacion> userManager)
        {
            _incidenteService = incidenteService;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        // 1. Dashboard Provincial: Solo para funcionarios autenticados
        [Authorize]
        public async Task<IActionResult> Index(int? distritoId, string? estado)
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", distritoId);
            ViewBag.DistritoSeleccionado = distritoId;
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;

            var incidentes = await _incidenteService.ObtenerTodosAsync(distritoId, estado);
            return View(incidentes);
        }

        // 2. Formulario Público de Alerta (Acceso Ciudadano Anónimo o Logueado)
        [AllowAnonymous]
        public async Task<IActionResult> Registrar()
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre");

            var incidente = new IncidenteEmergencia();
            var usuario = await _userManager.GetUserAsync(User);

            // Si está logueado, prellenar los datos del usuario
            if (usuario != null)
            {
                incidente.DniCiudadano = usuario.Dni;
                incidente.NombreCiudadano = usuario.NombreCompleto;
                incidente.Telefono = usuario.PhoneNumber ?? string.Empty;
                ViewBag.UsuarioLogueado = true;
                ViewBag.UsuarioActual = usuario;
            }
            else
            {
                ViewBag.UsuarioLogueado = false;
            }

            return View(incidente);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(IncidenteEmergencia incidente)
        {
            ModelState.Remove(nameof(incidente.CodigoIncidente));
            ModelState.Remove(nameof(incidente.Distrito));

            if (ModelState.IsValid)
            {
                var nuevo = await _incidenteService.RegistrarIncidenteAsync(incidente);
                var distritos = await _incidenteService.ObtenerDistritosAsync();
                var nombreDistrito = distritos.FirstOrDefault(d => d.DistritoId == nuevo.DistritoId)?.Nombre ?? "Distrito";

                // Push SignalR en tiempo real hacia el Dashboard provincial
                await _hubContext.Clients.All.SendAsync("NuevoIncidenteReportado", new
                {
                    incidenteId = nuevo.IncidenteId,
                    codigo = nuevo.CodigoIncidente,
                    sector = nuevo.SectorCritico,
                    referencia = nuevo.DireccionReferencia ?? "Sin referencia",
                    distritoId = nuevo.DistritoId,
                    distritoNombre = nombreDistrito,
                    tipo = nuevo.TipoEvento,
                    severidad = nuevo.Severidad,
                    familias = nuevo.FamiliasAfectadas,
                    fecha = nuevo.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                    estado = nuevo.Estado
                });

                // Redirigir al ciudadano a su comprobante con PIN generado
                return RedirectToAction(nameof(ConfirmacionRegistro), new { codigo = nuevo.CodigoIncidente });
            }

            var listaDistritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(listaDistritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", incidente.DistritoId);
            return View(incidente);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmacionRegistro(string codigo)
        {
            var incidente = await _incidenteService.ObtenerPorCodigoAsync(codigo);
            if (incidente == null) return RedirectToAction(nameof(Registrar));
            return View(incidente);
        }

        // ── Portal Ciudadano: Consulta y Validación OTP ───────────────────────
        [AllowAnonymous]
        public IActionResult Consultar()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarOtp(string codigoIncidente, string dni)
        {
            var incidente = await _incidenteService.ObtenerPorCodigoAsync(codigoIncidente);
            if (incidente == null || incidente.DniCiudadano != dni.Trim())
            {
                TempData["ErrorConsulta"] = "No se encontró ningún reporte con ese Código y DNI.";
                return RedirectToAction(nameof(Consultar));
            }

            string pin = await _incidenteService.GenerarOtpCiudadanoAsync(codigoIncidente, dni, incidente.Telefono ?? "999999999");
            TempData["PinSimulado"] = pin;
            TempData["CodigoIncidente"] = incidente.CodigoIncidente;
            TempData["DniCiudadano"] = dni;

            return RedirectToAction(nameof(ValidarOtp));
        }

        [AllowAnonymous]
        public IActionResult ValidarOtp()
        {
            if (TempData["CodigoIncidente"] == null) return RedirectToAction(nameof(Consultar));
            ViewBag.CodigoIncidente = TempData["CodigoIncidente"]?.ToString();
            ViewBag.DniCiudadano = TempData["DniCiudadano"]?.ToString();
            ViewBag.PinSimulado = TempData["PinSimulado"]?.ToString();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidarOtp(string codigoIncidente, string dni, string pin)
        {
            bool esValido = await _incidenteService.ValidarOtpCiudadanoAsync(codigoIncidente, dni, pin);
            if (!esValido)
            {
                ViewBag.Error = "PIN incorrecto o expirado (recuerde que dura 30 segundos). Solicite uno nuevo.";
                ViewBag.CodigoIncidente = codigoIncidente;
                ViewBag.DniCiudadano = dni;
                return View();
            }

            return RedirectToAction(nameof(Seguimiento), new { codigo = codigoIncidente });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Seguimiento(string codigo)
        {
            var incidente = await _incidenteService.ObtenerPorCodigoAsync(codigo);
            if (incidente == null) return RedirectToAction(nameof(Consultar));
            return View(incidente);
        }

        // 3. Operación de cambio de estado: Exclusivo para funcionarios autenticados
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            await _incidenteService.CambiarEstadoAsync(id, nuevoEstado);
            var inc = await _incidenteService.ObtenerPorIdAsync(id);

            if (inc != null)
            {
                await _hubContext.Clients.All.SendAsync("EstadoIncidenteActualizado", new
                {
                    codigo = inc.CodigoIncidente,
                    nuevoEstado = inc.Estado
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Portal(string? buscar, string? estado, string? fechaDesde, string? fechaHasta)
        {
            // Cargar distritos para los modales (tanto anónimo como logueado)
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre");

            // Si el ciudadano ya está logueado, le cargamos directamente sus reportes
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    // Obtenemos todos los incidentes y filtramos por su DNI
                    var todos = await _incidenteService.ObtenerTodosAsync(null, null);
                    var misReportes = todos.Where(i => i.DniCiudadano == usuario.Dni).ToList();

                    // Aplicar filtros
                    if (!string.IsNullOrWhiteSpace(buscar))
                    {
                        buscar = buscar.ToUpper().Trim();
                        misReportes = misReportes.Where(i => i.CodigoIncidente.Contains(buscar)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        misReportes = misReportes.Where(i => i.Estado == estado).ToList();
                    }

                    if (DateTime.TryParse(fechaDesde, out var desde))
                    {
                        misReportes = misReportes.Where(i => i.FechaRegistro.Date >= desde.Date).ToList();
                    }

                    if (DateTime.TryParse(fechaHasta, out var hasta))
                    {
                        misReportes = misReportes.Where(i => i.FechaRegistro.Date <= hasta.Date).ToList();
                    }

                    // Ordenar por fecha descendente
                    misReportes = misReportes.OrderByDescending(i => i.FechaRegistro).ToList();

                    ViewBag.MisReportes = misReportes;
                    ViewBag.UsuarioActual = usuario;
                    ViewBag.BusquedaActiva = buscar;
                    ViewBag.EstadoSeleccionado = estado;
                    ViewBag.FechaDesde = fechaDesde;
                    ViewBag.FechaHasta = fechaHasta;
                    ViewBag.Estados = new[] { "Reportado", "Constatado", "En Atención", "Atendido", "Desestimado" };
                }
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DescargarReportesPDF()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return RedirectToAction(nameof(Portal));

            var todos = await _incidenteService.ObtenerTodosAsync(null, null);
            var misReportes = todos.Where(i => i.DniCiudadano == usuario.Dni).OrderByDescending(i => i.FechaRegistro).ToList();

            if (!misReportes.Any())
            {
                TempData["ErrorDescarga"] = "No hay reportes para descargar.";
                return RedirectToAction(nameof(Portal));
            }

            var document = new PdfSharp.Pdf.PdfDocument();
            var page = document.AddPage();
            var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

            var titleFont = new PdfSharp.Drawing.XFont("Arial", 18);
            var headerFont = new PdfSharp.Drawing.XFont("Arial", 10);
            var normalFont = new PdfSharp.Drawing.XFont("Arial", 9);

            double yPos = 30;
            double xMargin = 30;

            gfx.DrawString("REPORTE DE MIS EMERGENCIAS", titleFont, PdfSharp.Drawing.XBrushes.Black,
                new PdfSharp.Drawing.XRect(xMargin, yPos, page.Width.Millimeter * 2.834 - 60, 25),
                PdfSharp.Drawing.XStringFormats.TopCenter);
            yPos += 30;

            gfx.DrawString($"Ciudadano: {usuario.NombreCompleto} | DNI: {usuario.Dni}", normalFont, PdfSharp.Drawing.XBrushes.Black,
                new PdfSharp.Drawing.XRect(xMargin, yPos, page.Width.Millimeter * 2.834 - 60, 15),
                PdfSharp.Drawing.XStringFormats.TopCenter);
            yPos += 15;

            gfx.DrawString($"Generado: {TimeHelper.Ahora:dd/MM/yyyy HH:mm}", normalFont, PdfSharp.Drawing.XBrushes.Gray,
                new PdfSharp.Drawing.XRect(xMargin, yPos, page.Width.Millimeter * 2.834 - 60, 12),
                PdfSharp.Drawing.XStringFormats.TopCenter);
            yPos += 25;

            gfx.DrawString("CÓDIGO | TIPO EVENTO | SECTOR | FAMILIAS | ESTADO | FECHA", headerFont,
                PdfSharp.Drawing.XBrushes.White,
                new PdfSharp.Drawing.XRect(xMargin - 5, yPos - 3, page.Width.Millimeter * 2.834 - 60, 15),
                PdfSharp.Drawing.XStringFormats.TopLeft);
            yPos += 18;

            int rowsPerPage = 30;
            int currentRow = 0;

            foreach (var reporte in misReportes)
            {
                if (currentRow >= rowsPerPage)
                {
                    gfx.Dispose();
                    page = document.AddPage();
                    gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                    yPos = 30;
                    currentRow = 0;
                }

                string lineText = $"{reporte.CodigoIncidente} | {reporte.TipoEvento} | {reporte.SectorCritico} | {reporte.FamiliasAfectadas} | {reporte.Estado} | {reporte.FechaRegistro:dd/MM/yyyy}";
                gfx.DrawString(lineText, normalFont, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XRect(xMargin, yPos, page.Width.Millimeter * 2.834 - 60, 12),
                    PdfSharp.Drawing.XStringFormats.TopLeft);

                yPos += 12;
                currentRow++;
            }

            yPos += 10;
            gfx.DrawString($"Total de reportes: {misReportes.Count}", normalFont, PdfSharp.Drawing.XBrushes.Black,
                new PdfSharp.Drawing.XRect(xMargin, yPos, page.Width.Millimeter * 2.834 - 60, 12),
                PdfSharp.Drawing.XStringFormats.TopRight);

            gfx.Dispose();

            var memoryStream = new System.IO.MemoryStream();
            document.Save(memoryStream, false);
            memoryStream.Position = 0;

            return File(memoryStream, "application/pdf", $"Reportes_Emergencias_{usuario.Dni}_{TimeHelper.Ahora:yyyyMMdd_HHmm}.pdf");
        }
    }
}