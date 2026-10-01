using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;

namespace ResilienciaNorte.Web.Controllers
{
    public class RecursosController : Controller
    {
        private readonly IRecursoService _recursoService;
        private readonly IIncidenteService _incidenteService;
        private readonly IHubContext<EmergenciaHub> _hubContext;

        public RecursosController(
            IRecursoService recursoService,
            IIncidenteService incidenteService,
            IHubContext<EmergenciaHub> hubContext)
        {
            _recursoService = recursoService;
            _incidenteService = incidenteService;
            _hubContext = hubContext;
        }

        // 1. Listado de Almacén con Filtro
        public async Task<IActionResult> Index(int? distritoId, string? categoria)
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", distritoId);
            ViewBag.DistritoSeleccionado = distritoId;
            ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;

            var recursos = await _recursoService.ObtenerTodosAsync(distritoId, categoria);
            return View(recursos);
        }

        // 2. Registro Transaccional de Ingreso (Compras ediles, Donaciones, Sobrantes)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarIngreso(int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen)
        {
            try
            {
                string responsable = User.Identity?.Name ?? "Logística BAH";
                await _recursoService.RegistrarIngresoAsync(
                    recursoId,
                    cantidad,
                    tipoEntrada,
                    concepto,
                    documento,
                    entidadOrigen,
                    usuarioId: responsable
                );

                var recurso = await _recursoService.ObtenerPorIdAsync(recursoId);

                // Emitir actualización de Kardex en tiempo real hacia todas las consolas abiertas
                await _hubContext.Clients.All.SendAsync("NuevoMovimientoKardex", new
                {
                    recursoId = recursoId,
                    fechaHora = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    tipo = "ENTRADA",
                    concepto = $"{tipoEntrada}: {concepto}",
                    documento = documento,
                    origenDestino = entidadOrigen,
                    stockAnterior = recurso != null ? recurso.StockDisponible - cantidad : 0,
                    cantidad = cantidad,
                    stockPosterior = recurso != null ? recurso.StockDisponible : cantidad,
                    responsable = responsable
                });

                TempData["MensajeExito"] = "Ingreso registrado correctamente en el Kardex.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al registrar ingreso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Vista de Kardex Inmutable por Recurso
        public async Task<IActionResult> Kardex(int id)
        {
            var recurso = await _recursoService.ObtenerPorIdAsync(id);
            if (recurso == null) return NotFound();

            ViewBag.Recurso = recurso;
            var movimientos = await _recursoService.ObtenerKardexPorRecursoAsync(id);
            return View(movimientos);
        }

        // 4. Reabastecimiento Subsidiario (Distrito -> Provincia -> Región) con Push SignalR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SolicitarReabastecimiento(int recursoId, int cantidad, string justificacion)
        {
            var recurso = await _recursoService.ObtenerPorIdAsync(recursoId);
            if (recurso == null) return NotFound();

            var solicitud = await _recursoService.SolicitarReabastecimientoAsync(
                recursoId,
                recurso.DistritoId,
                cantidad,
                justificacion
            );

            // Alerta transversal en tiempo real
            await _hubContext.Clients.All.SendAsync("AlertaStockCritico", new
            {
                codigoSolicitud = solicitud.CodigoSolicitud,
                recurso = recurso.Nombre,
                distrito = recurso.Distrito?.Nombre,
                stockActual = recurso.StockDisponible,
                stockMinimo = recurso.StockMinimo,
                nivelDestino = solicitud.NivelDestino
            });

            TempData["MensajeExito"] = $"Solicitud {solicitud.CodigoSolicitud} enviada hacia {solicitud.NivelDestino}.";
            return RedirectToAction(nameof(Solicitudes));
        }

        // 5. Bandeja de Reabastecimiento Multinivel
        public async Task<IActionResult> Solicitudes()
        {
            var solicitudes = await _recursoService.ObtenerSolicitudesAsync();
            return View(solicitudes);
        }
    }
}