using Microsoft.AspNetCore.Authorization;
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

        public IncidentesController(IIncidenteService incidenteService, IHubContext<EmergenciaHub> hubContext)
        {
            _incidenteService = incidenteService;
            _hubContext = hubContext;
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

        // 2. Formulario Público de Alerta (Acceso Ciudadano Anónimo)
        [AllowAnonymous]
        public async Task<IActionResult> Registrar()
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre");
            return View(new IncidenteEmergencia());
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
    }
}