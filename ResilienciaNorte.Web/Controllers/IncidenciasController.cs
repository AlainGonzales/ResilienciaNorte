using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers
{
    [Authorize(Roles = "Analista,Administrador")]
    public class IncidenciasController : Controller
    {
        private readonly IIncidenciaService _incidenciaService;
        private readonly IRecursoService _recursoService;
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly IAuditoriaService _auditoria;

        public IncidenciasController(IIncidenciaService incidenciaService, IRecursoService recursoService, UserManager<UsuarioAplicacion> userManager, IAuditoriaService auditoria)
        {
            _incidenciaService = incidenciaService;
            _recursoService = recursoService;
            _userManager = userManager;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index(string? estado)
        {
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;

            // El analista distrital solo ve su distrito y sus evaluadores; el administrador ve todo
            int? distritoId = await DistritoDelAnalistaAsync();
            ViewBag.Evaluadores = (await _userManager.GetUsersInRoleAsync("EvaluadorCampo"))
                .Where(u => distritoId == null || u.DistritoId == distritoId)
                .ToList();

            // Solo el analista pide bienes; el stock que ve es el de su almacén distrital
            ViewBag.PuedePedirBienes = User.IsInRole("Analista") && !User.IsInRole("Administrador") && distritoId != null;
            ViewBag.StockDistrito = distritoId == null
                ? Enumerable.Empty<ExistenciaAlmacen>()
                : await StockDelDistritoAsync(distritoId.Value);

            // Bienes pedidos por incidencia: se ven en solo lectura y, si nadie ha decidido, se pueden editar
            var agrupados = (await _recursoService.ObtenerSolicitudesAsync())
                .Where(s => s.IncidenciaId != null)
                .GroupBy(s => s.IncidenciaId!.Value)
                .ToList();
            ViewBag.EstadoPedido = agrupados.ToDictionary(
                g => g.Key,
                g => g.All(s => s.Estado == "Pendiente") ? "Editable" : "Bloqueado");
            var solicitudesPorIncidencia = agrupados.ToDictionary(g => g.Key.ToString(), g => g.Select(s => new
            {
                codigo = s.CodigoSolicitud,
                bien = s.Recurso?.Nombre ?? string.Empty,
                recursoId = s.RecursoId,
                cantidad = s.CantidadSolicitada,
                estado = s.Estado,
                motivo = s.MotivoRechazo ?? string.Empty,
                justificacion = s.Justificacion
            }).ToList());
            ViewBag.SolicitudesJson = System.Text.Json.JsonSerializer.Serialize(solicitudesPorIncidencia);

            return View(await _incidenciaService.ObtenerAsync(estado, distritoId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int[] reporteIds, string sector, string tipoEvento, string? observacion)
        {
            try
            {
                int? distritoAnalista = await DistritoDelAnalistaAsync();
                var incidencia = await _incidenciaService.CrearAsync(reporteIds, sector, tipoEvento, observacion, distritoAnalista);
                TempData["MensajeExito"] = $"Incidencia {incidencia.Codigo} creada con {reporteIds.Length} reporte(s).";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Incidentes",
                    $"Incidencia {incidencia.Codigo} creada con {reporteIds.Length} reporte(s) · {sector}");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction("Index", "Incidentes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int id, string evaluadorId)
        {
            try
            {
                var incidencia = await _incidenciaService.ObtenerPorIdAsync(id)
                    ?? throw new InvalidOperationException("La incidencia no existe.");
                var evaluador = await _userManager.FindByIdAsync(evaluadorId)
                    ?? throw new InvalidOperationException("El evaluador no existe.");

                int? distritoAnalista = await DistritoDelAnalistaAsync();
                if (distritoAnalista != null && incidencia.DistritoId != distritoAnalista)
                    throw new InvalidOperationException("Solo puede asignar incidencias de su distrito.");
                if (evaluador.DistritoId != incidencia.DistritoId)
                    throw new InvalidOperationException("El evaluador debe pertenecer al mismo distrito que la incidencia.");

                await _incidenciaService.AsignarEvaluadorAsync(id, evaluadorId);
                TempData["MensajeExito"] = "Evaluador asignado. Los reportes quedan como Asignados.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Incidentes",
                    $"Incidencia {incidencia.Codigo} asignada a {evaluador.NombreCompleto}");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // El analista pide bienes desde una incidencia constatada de su distrito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PedirBienes(int incidenciaId, int[] recursoIds, int[] cantidades, string justificacion)
        {
            try
            {
                var incidencia = await _incidenciaService.ObtenerPorIdAsync(incidenciaId)
                    ?? throw new InvalidOperationException("La incidencia no existe.");

                int? distritoAnalista = await DistritoDelAnalistaAsync();
                if (distritoAnalista != null && incidencia.DistritoId != distritoAnalista)
                    throw new InvalidOperationException("Solo puede pedir bienes para incidencias de su distrito.");

                if (recursoIds.Length != cantidades.Length)
                    throw new InvalidOperationException("Cada bien debe tener su cantidad.");

                var lineas = recursoIds.Zip(cantidades, (recurso, cantidad) => (RecursoId: recurso, Cantidad: cantidad));
                var solicitudes = (await _recursoService.SolicitarParaIncidenciaAsync(incidencia, lineas, justificacion)).ToList();
                TempData["MensajeExito"] = $"{solicitudes.Count} bien(es) enviados al coordinador del distrito.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Almacen",
                    $"Pedido de {solicitudes.Count} bien(es) para la incidencia {incidencia.Codigo}");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // Edita el pedido de una incidencia constatada mientras el coordinador no haya decidido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPedido(int incidenciaId, int[] recursoIds, int[] cantidades, string justificacion)
        {
            try
            {
                var incidencia = await _incidenciaService.ObtenerPorIdAsync(incidenciaId)
                    ?? throw new InvalidOperationException("La incidencia no existe.");

                int? distritoAnalista = await DistritoDelAnalistaAsync();
                if (distritoAnalista != null && incidencia.DistritoId != distritoAnalista)
                    throw new InvalidOperationException("Solo puede editar pedidos de incidencias de su distrito.");

                if (recursoIds.Length != cantidades.Length)
                    throw new InvalidOperationException("Cada bien debe tener su cantidad.");

                var lineas = recursoIds.Zip(cantidades, (recurso, cantidad) => (RecursoId: recurso, Cantidad: cantidad));
                var solicitudes = (await _recursoService.EditarPedidoIncidenciaAsync(incidencia, lineas, justificacion)).ToList();
                TempData["MensajeExito"] = $"Pedido actualizado: {solicitudes.Count} bien(es).";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Almacen",
                    $"Pedido de {incidencia.Codigo} editado: {solicitudes.Count} bien(es)");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Sugerir(int reporteId)
        {
            var ids = (await _incidenciaService.SugerirReportesAsync(reporteId)).Select(r => r.IncidenteId).ToList();
            return Json(ids);
        }

        // Distrito del analista distrital; null si el usuario ve toda la provincia
        private async Task<int?> DistritoDelAnalistaAsync()
        {
            if (User.IsInRole("Administrador")) return null;

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || usuario.NivelJurisdiccion != "Distrital") return null;
            return usuario.DistritoId;
        }

        private async Task<IEnumerable<ExistenciaAlmacen>> StockDelDistritoAsync(int distritoId)
        {
            var almacen = (await _recursoService.ObtenerAlmacenesAsync())
                .FirstOrDefault(a => a.Nivel == "Distrital" && a.DistritoId == distritoId);
            return almacen == null
                ? Enumerable.Empty<ExistenciaAlmacen>()
                : await _recursoService.ObtenerExistenciasAsync(almacen.AlmacenId, null);
        }
    }
}
