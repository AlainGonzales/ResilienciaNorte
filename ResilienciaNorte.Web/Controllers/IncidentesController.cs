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

        public async Task<IActionResult> Index(int? distritoId, string? estado)
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", distritoId);
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;

            var incidentes = await _incidenteService.ObtenerTodosAsync(distritoId, estado);
            return View(incidentes ?? Enumerable.Empty<IncidenteEmergencia>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            await _incidenteService.CambiarEstadoAsync(id, nuevoEstado);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Registrar()
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre");
            return View(new IncidenteEmergencia());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(IncidenteEmergencia incidente)
        {
            // 1. Ignorar campos que no provienen del formulario del usuario
            ModelState.Remove(nameof(incidente.CodigoIncidente));
            ModelState.Remove(nameof(incidente.Distrito));

            // 2. Si no trae código de incidente, generarlo automáticamente
            if (string.IsNullOrWhiteSpace(incidente.CodigoIncidente))
            {
                incidente.CodigoIncidente = $"ALT-{DateTime.UtcNow.Year}-{new Random().Next(1000, 9999)}";
            }

            if (ModelState.IsValid)
            {
                var nuevo = await _incidenteService.RegistrarIncidenteAsync(incidente);
                var distritos = await _incidenteService.ObtenerDistritosAsync();
                var nombreDistrito = distritos.FirstOrDefault(d => d.DistritoId == nuevo.DistritoId)?.Nombre ?? "Distrito";

                // Emitir notificación en vivo por SignalR hacia el Dashboard
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

                return RedirectToAction(nameof(Index));
            }

            var listaDistritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(listaDistritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", incidente.DistritoId);
            return View(incidente);
        }
    }
}