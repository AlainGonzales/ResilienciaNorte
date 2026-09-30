using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;
using ResilienciaNorte.Web.Models;

namespace ResilienciaNorte.Web.Controllers
{
    /// <summary>
    /// Controlador del Módulo 3: Almacén, Kardex y Reabastecimiento.
    /// Responsabilidades:
    ///   - CRUD de ingresos (Compra Directa, Donación, Sobrante).
    ///   - Consulta del Kardex con filtros.
    ///   - Gestión de solicitudes de reabastecimiento subsidiario.
    ///   - Emisión de alertas en tiempo real vía SignalR.
    /// </summary>
    public class AlmacenController : Controller
    {
        private readonly IAlmacenService _almacenService;
        private readonly IHubContext<EmergenciaHub> _hubContext;

        public AlmacenController(IAlmacenService almacenService, IHubContext<EmergenciaHub> hubContext)
        {
            _almacenService = almacenService;
            _hubContext     = hubContext;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // INVENTARIO (Página principal del módulo)
        // ═══════════════════════════════════════════════════════════════════════

        public async Task<IActionResult> Index(int? distritoId, string? categoria)
        {
            var distritos = await _almacenService.ObtenerDistritosAsync();
            ViewBag.Distritos          = new SelectList(distritos, "DistritoId", "Nombre", distritoId);
            ViewBag.DistritoSeleccionado = distritoId;
            ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;

            // Alertas de stock crítico para el banner superior
            ViewBag.AlertasStock = await _almacenService.ContarAlertasStockCriticoAsync(distritoId);

            var inventario = await _almacenService.ObtenerInventarioAsync(distritoId, categoria);
            return View(inventario ?? Enumerable.Empty<RecursoAlmacen>());
        }

        // ═══════════════════════════════════════════════════════════════════════
        // INGRESO AL ALMACÉN
        // ═══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Ingreso()
        {
            var vm = await ConstruirViewModelIngresoAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ingreso(IngresoAlmacenViewModel vm)
        {
            // El usuario logueado (simulado; en producción usar User.Identity.Name).
            string usuarioId = User.Identity?.Name ?? "sistema";

            if (!ModelState.IsValid)
            {
                vm = await ConstruirViewModelIngresoAsync(vm);
                return View(vm);
            }

            try
            {
                var movimiento = await _almacenService.RegistrarIngresoAsync(
                    recursoId:          vm.RecursoId,
                    distritoId:         vm.DistritoId,
                    cantidad:           vm.Cantidad,
                    concepto:           vm.Concepto,
                    usuarioId:          usuarioId,
                    documentoReferencia: vm.DocumentoReferencia,
                    entidadOrigen:      vm.EntidadOrigen);

                // Obtener datos completos para la notificación SignalR.
                var recurso = await _almacenService.ObtenerRecursoPorIdAsync(vm.RecursoId);

                // Emitir actualización de inventario en tiempo real al grupo del distrito.
                await _hubContext.Clients
                    .Group($"Distrito_{vm.DistritoId}")
                    .SendAsync("ActualizarInventario", new
                    {
                        recursoId    = vm.RecursoId,
                        nombre       = recurso?.Nombre ?? string.Empty,
                        distritoId   = vm.DistritoId,
                        stockActual  = movimiento.StockPosterior,
                        esCritico    = recurso?.EsStockCritico() ?? false,
                        concepto     = vm.Concepto,
                        cantidad     = vm.Cantidad
                    });

                // Si el stock sigue siendo crítico después del ingreso, emitir alerta.
                if (recurso?.EsStockCritico() == true)
                {
                    await _hubContext.Clients.All.SendAsync("AlertaStockCritico", new
                    {
                        recursoId   = vm.RecursoId,
                        nombre      = recurso.Nombre,
                        distritoId  = vm.DistritoId,
                        stockActual = recurso.StockDisponible,
                        stockMinimo = recurso.StockMinimo
                    });
                }

                TempData["Exito"] = $"Ingreso registrado correctamente. Movimiento ID: {movimiento.MovimientoId}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                vm = await ConstruirViewModelIngresoAsync(vm);
                return View(vm);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // KARDEX
        // ═══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Kardex(
            int? recursoId, int? distritoId, DateTime? desde, DateTime? hasta)
        {
            var distritos = await _almacenService.ObtenerDistritosAsync();
            var recursos  = await _almacenService.ObtenerInventarioAsync();

            var filtro = new KardexFiltroViewModel
            {
                RecursoId  = recursoId,
                DistritoId = distritoId,
                Desde      = desde,
                Hasta      = hasta,
                Distritos  = new SelectList(distritos, "DistritoId", "Nombre", distritoId),
                Recursos   = new SelectList(recursos,  "RecursoId",  "Nombre", recursoId)
            };

            var movimientos = await _almacenService.ObtenerKardexAsync(recursoId, distritoId, desde, hasta);
            ViewBag.Movimientos = movimientos ?? Enumerable.Empty<MovimientoAlmacen>();

            return View(filtro);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // REABASTECIMIENTO SUBSIDIARIO
        // ═══════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> Reabastecimiento(int? distritoId, string? estado)
        {
            var distritos   = await _almacenService.ObtenerDistritosAsync();
            var solicitudes = await _almacenService.ObtenerSolicitudesAsync(distritoId, estado);

            ViewBag.Distritos           = new SelectList(distritos, "DistritoId", "Nombre", distritoId);
            ViewBag.DistritoSeleccionado = distritoId;
            ViewBag.EstadoSeleccionado  = estado ?? string.Empty;
            ViewBag.Solicitudes         = solicitudes ?? Enumerable.Empty<SolicitudReabastecimiento>();

            // Formulario para nueva solicitud
            var vm = await ConstruirViewModelReabastecimientoAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolicitud(ReabastecimientoViewModel vm)
        {
            string usuarioId = User.Identity?.Name ?? "sistema";

            if (!ModelState.IsValid)
            {
                // Reconstruir la vista completa con los filtros actuales.
                var distritos   = await _almacenService.ObtenerDistritosAsync();
                var solicitudes = await _almacenService.ObtenerSolicitudesAsync();
                vm = await ConstruirViewModelReabastecimientoAsync(vm);

                ViewBag.Distritos   = new SelectList(distritos, "DistritoId", "Nombre");
                ViewBag.Solicitudes = solicitudes;
                return View("Reabastecimiento", vm);
            }

            try
            {
                var solicitud = await _almacenService.CrearSolicitudReabastecimientoAsync(
                    distritoOrigenId:  vm.DistritoOrigenId,
                    recursoId:         vm.RecursoId,
                    cantidadSolicitada: vm.CantidadSolicitada,
                    nivelDestino:      vm.NivelDestino,
                    justificacion:     vm.Justificacion,
                    usuarioId:         usuarioId);

                // Notificar en tiempo real al distrito solicitante.
                await _hubContext.Clients
                    .Group($"Distrito_{vm.DistritoOrigenId}")
                    .SendAsync("AlertaReabastecimiento", new
                    {
                        codigoSolicitud    = solicitud.CodigoSolicitud,
                        nivelDestino       = solicitud.NivelDestino,
                        recursoId          = solicitud.RecursoId,
                        cantidadSolicitada = solicitud.CantidadSolicitada,
                        estado             = solicitud.Estado
                    });

                // Notificar al nivel superior (Provincial o Regional).
                await _hubContext.Clients
                    .Group($"Nivel_{vm.NivelDestino}")
                    .SendAsync("NuevaSolicitudReabastecimiento", new
                    {
                        codigoSolicitud = solicitud.CodigoSolicitud,
                        distritoId      = vm.DistritoOrigenId,
                        recursoId       = vm.RecursoId,
                        cantidad        = vm.CantidadSolicitada
                    });

                TempData["Exito"] = $"Solicitud {solicitud.CodigoSolicitud} creada correctamente.";
                return RedirectToAction(nameof(Reabastecimiento));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var distritos   = await _almacenService.ObtenerDistritosAsync();
                var solicitudes = await _almacenService.ObtenerSolicitudesAsync();
                vm = await ConstruirViewModelReabastecimientoAsync(vm);

                ViewBag.Distritos   = new SelectList(distritos, "DistritoId", "Nombre");
                ViewBag.Solicitudes = solicitudes;
                return View("Reabastecimiento", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarSolicitud(int solicitudId)
        {
            string usuarioId = User.Identity?.Name ?? "sistema";
            try
            {
                await _almacenService.AprobarSolicitudAsync(solicitudId, usuarioId);

                // Notificar al grupo: la solicitud fue aprobada.
                await _hubContext.Clients.All.SendAsync("SolicitudAprobada", new { solicitudId });

                TempData["Exito"] = "Solicitud aprobada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Reabastecimiento));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarSolicitud(int solicitudId, string motivo)
        {
            string usuarioId = User.Identity?.Name ?? "sistema";
            try
            {
                await _almacenService.RechazarSolicitudAsync(solicitudId, usuarioId, motivo);

                await _hubContext.Clients.All.SendAsync("SolicitudRechazada", new { solicitudId, motivo });

                TempData["Exito"] = "Solicitud rechazada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Reabastecimiento));
        }

        // ═══════════════════════════════════════════════════════════════════════
        // HELPERS PRIVADOS
        // ═══════════════════════════════════════════════════════════════════════

        private async Task<IngresoAlmacenViewModel> ConstruirViewModelIngresoAsync(
            IngresoAlmacenViewModel? vm = null)
        {
            var distritos = await _almacenService.ObtenerDistritosAsync();
            var recursos  = await _almacenService.ObtenerInventarioAsync();

            vm ??= new IngresoAlmacenViewModel();
            vm.Distritos = new SelectList(distritos, "DistritoId", "Nombre", vm.DistritoId);

            // Proyectar a tipo anónimo para mostrar [CodigoBAH] Nombre en el dropdown.
            var recursosProyectados = recursos.Select(r => new
            {
                r.RecursoId,
                Display = $"[{r.CodigoBAH}] {r.Nombre}"
            });
            vm.Recursos = new SelectList(recursosProyectados, "RecursoId", "Display", vm.RecursoId);

            return vm;
        }

        private async Task<ReabastecimientoViewModel> ConstruirViewModelReabastecimientoAsync(
            ReabastecimientoViewModel? vm = null)
        {
            var distritos = await _almacenService.ObtenerDistritosAsync();
            var recursos  = await _almacenService.ObtenerRecursosConStockCriticoAsync();

            vm ??= new ReabastecimientoViewModel();
            vm.Distritos = new SelectList(distritos, "DistritoId", "Nombre", vm.DistritoOrigenId);

            // Mostrar código, nombre y stock actual para facilitar la selección.
            var recursosProyectados = recursos.Select(r => new
            {
                r.RecursoId,
                Display = $"[{r.CodigoBAH}] {r.Nombre} (Stock: {r.StockDisponible})"
            });
            vm.Recursos = new SelectList(recursosProyectados, "RecursoId", "Display", vm.RecursoId);

            return vm;
        }
    }
}
