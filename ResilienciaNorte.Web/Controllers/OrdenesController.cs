using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;
using ResilienciaNorte.Web.Models;

namespace ResilienciaNorte.Web.Controllers
{
    // Módulo Maestro-Detalle: OrdenAtencion (cabecera) + DetalleOrden (bienes BAH asignados)
    public class OrdenesController : Controller
    {
        private readonly IOrdenAtencionService _ordenService;
        private readonly IHubContext<EmergenciaHub> _hubContext;

        public OrdenesController(IOrdenAtencionService ordenService, IHubContext<EmergenciaHub> hubContext)
        {
            _ordenService = ordenService;
            _hubContext = hubContext;
        }

        private string UsuarioActual(string porDefecto) => User.Identity?.Name ?? porDefecto;

        // 1. Bandeja: incidentes por asignar + órdenes emitidas
        public async Task<IActionResult> Index(string? estado)
        {
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;
            ViewBag.PorAsignar = await _ordenService.ObtenerIncidentesPorAsignarAsync();
            var ordenes = await _ordenService.ObtenerTodasAsync(estado);
            return View(ordenes);
        }

        // 2. Nueva orden para un incidente constatado
        public async Task<IActionResult> Crear(int incidenteId)
        {
            var modelo = new CrearOrdenViewModel { IncidenteId = incidenteId };
            if (!await CargarApoyoAsync(modelo))
            {
                TempData["MensajeError"] = "El incidente no existe o no está constatado, por lo que no admite asignación de recursos.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearOrdenViewModel modelo)
        {
            // Las filas totalmente vacías se ignoran en vez de bloquear el envío. Al quitarlas cambian
            // los índices del formulario, así que se revalida el modelo ya depurado.
            modelo.Detalles = modelo.Detalles.Where(d => d.RecursoId > 0 || d.Cantidad > 0).ToList();
            ModelState.Clear();
            TryValidateModel(modelo);

            if (modelo.Detalles.Count == 0)
                ModelState.AddModelError(string.Empty, "Agregue al menos un bien BAH a la orden.");

            if (ModelState.IsValid)
            {
                try
                {
                    var lineas = modelo.Detalles.Select(d => new LineaOrden(d.RecursoId, d.Cantidad, d.Observaciones));
                    var orden = await _ordenService.CrearOrdenAsync(
                        modelo.IncidenteId, UsuarioActual("Analista COEP"), modelo.Observaciones, lineas);

                    var creada = await _ordenService.ObtenerPorIdAsync(orden.OrdenId);
                    await NotificarEstadoAsync(creada?.Incidente);

                    TempData["MensajeExito"] = $"Orden {orden.CodigoOrden} emitida con {orden.Detalles.Count} bien(es) BAH.";
                    return RedirectToAction(nameof(Detalle), new { id = orden.OrdenId });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            if (!await CargarApoyoAsync(modelo))
            {
                TempData["MensajeError"] = "El incidente ya no está disponible para asignación.";
                return RedirectToAction(nameof(Index));
            }
            return View(modelo);
        }

        // 3. Detalle de una orden (cabecera + bienes)
        public async Task<IActionResult> Detalle(int id)
        {
            var orden = await _ordenService.ObtenerPorIdAsync(id);
            if (orden == null) return NotFound();
            return View(orden);
        }

        // Acceso directo desde el dashboard de incidentes
        public async Task<IActionResult> PorIncidente(int id)
        {
            var orden = await _ordenService.ObtenerPorIncidenteAsync(id);
            if (orden == null) return RedirectToAction(nameof(Crear), new { incidenteId = id });
            return RedirectToAction(nameof(Detalle), new { id = orden.OrdenId });
        }

        // 4. Transiciones de la orden
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Despachar(int id)
            => EjecutarAsync(id, "Orden despachada: el stock fue descontado y registrado en el Kardex.",
                () => _ordenService.DespacharAsync(id, UsuarioActual("Logística BAH")));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Entregar(int id)
            => EjecutarAsync(id, "Entrega confirmada: el incidente figura como Atendido.",
                () => _ordenService.EntregarAsync(id, UsuarioActual("Coordinador COEP")));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Anular(int id, string? motivo)
            => EjecutarAsync(id, "Orden anulada: el incidente quedó disponible para una nueva asignación.",
                () => _ordenService.AnularAsync(id, UsuarioActual("Coordinador COEP"), motivo));

        private async Task<IActionResult> EjecutarAsync(int id, string mensajeExito, Func<Task<OrdenAtencion>> accion)
        {
            try
            {
                var orden = await accion();
                await NotificarEstadoAsync(orden.Incidente);
                TempData["MensajeExito"] = mensajeExito;
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Detalle), new { id });
        }

        // Carga el incidente y el catálogo de bienes; false si el incidente ya no admite asignación
        private async Task<bool> CargarApoyoAsync(CrearOrdenViewModel modelo)
        {
            modelo.Incidente = await _ordenService.ObtenerIncidenteAsignableAsync(modelo.IncidenteId);
            modelo.RecursosDisponibles = await _ordenService.ObtenerRecursosDisponiblesAsync();
            return modelo.Incidente != null;
        }

        // Mantiene viva la barra de progreso del ciudadano (Seguimiento)
        private Task NotificarEstadoAsync(IncidenteEmergencia? incidente)
        {
            if (incidente == null) return Task.CompletedTask;

            return _hubContext.Clients.All.SendAsync("EstadoIncidenteActualizado", new
            {
                codigo = incidente.CodigoIncidente,
                nuevoEstado = incidente.Estado
            });
        }
    }
}
