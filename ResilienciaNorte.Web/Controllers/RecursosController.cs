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
    [Authorize(Roles = "Administrador,Coordinador,LogisticoAlmacen")]
    public class RecursosController : Controller
    {
        private readonly IRecursoService _recursoService;
        private readonly IHubContext<EmergenciaHub> _hubContext;
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly IAuditoriaService _auditoria;

        public RecursosController(IRecursoService recursoService, IHubContext<EmergenciaHub> hubContext, UserManager<UsuarioAplicacion> userManager, IAuditoriaService auditoria)
        {
            _recursoService = recursoService;
            _hubContext = hubContext;
            _userManager = userManager;
            _auditoria = auditoria;
        }

        // 1. Existencias de un almacén (por defecto, el provincial)
        [Authorize(Roles = "Administrador,LogisticoAlmacen")]
        public async Task<IActionResult> Index(int? almacenId, string? categoria)
        {
            var almacenes = (await _recursoService.ObtenerAlmacenesAsync()).ToList();
            var seleccionado = almacenes.FirstOrDefault(a => a.AlmacenId == almacenId)
                ?? almacenes.FirstOrDefault(a => a.Nivel == "Regional");

            ViewBag.Almacenes = OpcionesAlmacenes(almacenes, seleccionado?.AlmacenId);
            ViewBag.AlmacenSeleccionado = seleccionado;
            ViewBag.Categorias = await _recursoService.ObtenerCategoriasAsync();
            ViewBag.CategoriaSeleccionada = categoria ?? string.Empty;

            var existencias = seleccionado == null
                ? Enumerable.Empty<ExistenciaAlmacen>()
                : await _recursoService.ObtenerExistenciasAsync(seleccionado.AlmacenId, categoria);
            return View(existencias);
        }

        // 2. Ingreso transaccional (compra directa, donación o sobrante)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,LogisticoAlmacen")]
        public async Task<IActionResult> RegistrarIngreso(int almacenId, int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen)
        {
            try
            {
                string responsable = User.Identity?.Name ?? "Logística BAH";
                var movimiento = await _recursoService.RegistrarIngresoAsync(
                    almacenId, recursoId, cantidad, tipoEntrada, concepto, documento, entidadOrigen, responsable);

                var existencia = await _recursoService.ObtenerExistenciaAsync(almacenId, recursoId);

                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Almacen",
                    $"Ingreso de {cantidad} ({tipoEntrada}) · almacén {almacenId} · bien {recursoId} · doc {documento}");

                await _hubContext.Clients.All.SendAsync("NuevoMovimientoKardex", new
                {
                    almacenId,
                    recursoId,
                    bien = existencia?.Recurso?.Nombre,
                    fechaHora = TimeHelper.Ahora.ToString("dd/MM/yyyy HH:mm"),
                    tipo = movimiento.TipoMovimiento,
                    concepto = movimiento.ConceptoMovimiento,
                    documento = movimiento.DocumentoReferencia,
                    origenDestino = movimiento.EntidadOrigenDestino,
                    stockAnterior = movimiento.StockAnterior,
                    cantidad = movimiento.Cantidad,
                    stockPosterior = movimiento.StockPosterior,
                    responsable
                });

                TempData["MensajeExito"] = "Ingreso registrado correctamente en el Kardex.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al registrar ingreso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { almacenId });
        }

        // 3. Kardex inmutable del almacén completo (filtrable por bien)
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> Kardex(int? almacenId, int? recursoId)
        {
            var almacenes = (await _recursoService.ObtenerAlmacenesAsync()).ToList();
            // Sin selección, el coordinador ve el kardex de su almacén distrital
            var almacen = almacenes.FirstOrDefault(a => a.AlmacenId == almacenId)
                ?? await AlmacenDelUsuarioAsync(almacenes)
                ?? almacenes.FirstOrDefault(a => a.Nivel == "Regional");
            if (almacen == null) return NotFound();

            ViewBag.Almacen = almacen;
            ViewBag.Almacenes = OpcionesAlmacenes(almacenes, almacen.AlmacenId);
            ViewBag.Bienes = await _recursoService.ObtenerExistenciasAsync(almacen.AlmacenId, null);
            ViewBag.RecursoFiltro = recursoId;

            var movimientos = await _recursoService.ObtenerKardexAsync(almacen.AlmacenId, recursoId);

            // El responsable se muestra con el correo del usuario, no con su ID interno
            var correos = new Dictionary<string, string>();
            foreach (var id in movimientos.Select(m => m.UsuarioResponsableId).Distinct())
            {
                var usuario = await _userManager.FindByIdAsync(id);
                if (usuario != null) correos[id] = usuario.Email ?? usuario.UserName ?? id;
            }
            ViewBag.Correos = correos;

            return View(movimientos);
        }

        // 4. Reabastecimiento: la solicitud va al almacén padre
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,LogisticoAlmacen")]
        public async Task<IActionResult> SolicitarReabastecimiento(int almacenId, int recursoId, int cantidad, string justificacion)
        {
            try
            {
                var solicitud = await _recursoService.SolicitarAbastecimientoAsync(almacenId, recursoId, cantidad, justificacion);
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Almacen",
                    $"Abastecimiento solicitado {solicitud.CodigoSolicitud}: {cantidad} · almacén {almacenId} · bien {recursoId}");
                var existencia = await _recursoService.ObtenerExistenciaAsync(almacenId, recursoId);
                var destino = await _recursoService.ObtenerAlmacenPorIdAsync(solicitud.AlmacenProveedorId ?? 0);

                await _hubContext.Clients.All.SendAsync("AlertaStockCritico", new
                {
                    codigoSolicitud = solicitud.CodigoSolicitud,
                    recurso = existencia?.Recurso?.Nombre,
                    almacen = existencia?.Almacen?.Nombre,
                    stockActual = existencia?.StockDisponible,
                    stockMinimo = existencia?.StockMinimo,
                    nivelDestino = destino?.Nombre
                });

                TempData["MensajeExito"] = $"Solicitud {solicitud.CodigoSolicitud} enviada hacia {destino?.Nombre}.";
                return RedirectToAction(nameof(Solicitudes));
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToAction(nameof(Index), new { almacenId });
            }
        }

        private static List<SelectListItem> OpcionesAlmacenes(IEnumerable<Almacen> almacenes, int? seleccionadoId)
        {
            var grupos = new Dictionary<string, SelectListGroup>();
            return almacenes.Select(a =>
            {
                if (!grupos.TryGetValue(a.Nivel, out var grupo))
                    grupos[a.Nivel] = grupo = new SelectListGroup { Name = a.Nivel };
                return new SelectListItem
                {
                    Value = a.AlmacenId.ToString(),
                    Text = a.Nombre,
                    Selected = a.AlmacenId == seleccionadoId,
                    Group = grupo
                };
            }).ToList();
        }

        // 5. Bandeja de solicitudes: cada coordinador y logístico ve las de su almacén; el administrador, todas
        public async Task<IActionResult> Solicitudes(string? estado)
        {
            var visibles = new List<Solicitud>();
            foreach (var s in await _recursoService.ObtenerSolicitudesAsync())
            {
                if (!string.IsNullOrEmpty(estado) && s.Estado != estado) continue;
                if (await EsDelAlmacenDelUsuarioAsync(s.AlmacenProveedorId))
                    visibles.Add(s);
            }

            // Stock del almacén proveedor, para que el coordinador decida según lo que hay
            var stock = new Dictionary<int, int>();
            foreach (var s in visibles)
            {
                var existencia = s.AlmacenProveedorId == null ? null
                    : await _recursoService.ObtenerExistenciaAsync(s.AlmacenProveedorId.Value, s.RecursoId);
                stock[s.SolicitudId] = existencia?.StockDisponible ?? 0;
            }

            ViewBag.Stock = stock;
            ViewBag.EstadoFiltro = estado ?? string.Empty;
            ViewBag.PuedeAprobar = User.IsInRole("Administrador") || User.IsInRole("Coordinador");
            ViewBag.PuedeDespachar = User.IsInRole("Administrador") || User.IsInRole("LogisticoAlmacen");
            return View(visibles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> Aprobar(int id)
        {
            try
            {
                var solicitud = await BuscarSolicitudAsync(id);
                await _recursoService.AprobarSolicitudAsync(id, _userManager.GetUserId(User) ?? string.Empty);
                TempData["MensajeExito"] = $"Solicitud {solicitud.CodigoSolicitud} aprobada. Lista para despacho.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Almacen",
                    $"Solicitud {solicitud.CodigoSolicitud} aprobada ({solicitud.CantidadSolicitada} de {solicitud.Recurso?.Nombre})");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Solicitudes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Coordinador")]
        public async Task<IActionResult> Rechazar(int id, string motivo)
        {
            try
            {
                var solicitud = await BuscarSolicitudAsync(id);
                await _recursoService.RechazarSolicitudAsync(id, _userManager.GetUserId(User) ?? string.Empty, motivo);
                TempData["MensajeExito"] = $"Solicitud {solicitud.CodigoSolicitud} rechazada.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Almacen",
                    $"Solicitud {solicitud.CodigoSolicitud} rechazada: {motivo}");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Solicitudes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,LogisticoAlmacen")]
        public async Task<IActionResult> Despachar(int id)
        {
            try
            {
                var solicitud = await BuscarSolicitudAsync(id);
                bool atendida = await _recursoService.DespacharSolicitudAsync(id, _userManager.GetUserId(User) ?? string.Empty);
                TempData["MensajeExito"] = $"Solicitud {solicitud.CodigoSolicitud} despachada. Salida registrada en el Kardex.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Almacen",
                    $"Solicitud {solicitud.CodigoSolicitud} despachada ({solicitud.CantidadSolicitada} de {solicitud.Recurso?.Nombre})");

                if (atendida)
                {
                    TempData["MensajeExito"] = $"Incidencia {solicitud.Incidencia?.Codigo} atendida: todos sus bienes fueron despachados.";
                    await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Incidentes",
                        $"Incidencia {solicitud.Incidencia?.Codigo} atendida: todos sus bienes despachados");
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Solicitudes));
        }

        private async Task<Solicitud> BuscarSolicitudAsync(int id)
        {
            var solicitud = (await _recursoService.ObtenerSolicitudesAsync()).FirstOrDefault(s => s.SolicitudId == id);
            if (solicitud == null) throw new InvalidOperationException("La solicitud no existe.");
            if (!await EsDelAlmacenDelUsuarioAsync(solicitud.AlmacenProveedorId))
                throw new InvalidOperationException("Solo puede gestionar solicitudes de su almacén.");
            return solicitud;
        }

        // El administrador ve todo; el resto solo los almacenes de su mismo distrito
        // Almacén distrital del usuario (coordinador o logístico); el administrador no tiene uno propio
        private async Task<Almacen?> AlmacenDelUsuarioAsync(IEnumerable<Almacen> almacenes)
        {
            if (User.IsInRole("Administrador")) return null;
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.DistritoId == null) return null;
            return almacenes.FirstOrDefault(a => a.Nivel == "Distrital" && a.DistritoId == usuario.DistritoId);
        }

        private async Task<bool> EsDelAlmacenDelUsuarioAsync(int? almacenId)
        {
            if (User.IsInRole("Administrador")) return true;
            if (almacenId == null) return false;

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.DistritoId == null) return false;

            var almacen = await _recursoService.ObtenerAlmacenPorIdAsync(almacenId.Value);
            return almacen?.DistritoId == usuario.DistritoId;
        }
    }
}
