using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers
{
    public class IncidentesController : Controller
    {
        private readonly IIncidenteService _incidenteService;

        public IncidentesController(IIncidenteService incidenteService)
        {
            _incidenteService = incidenteService;
        }

        // GET: /Incidentes/ o /Incidentes/Index
        public async Task<IActionResult> Index(int? distritoId, string? estado)
        {
            // 1. Cargar el listado de distritos para el dropdown de filtros
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", distritoId);

            // 2. Mantener el estado seleccionado en el ViewBag
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;

            // 3. Obtener los incidentes filtrados garantizando que la colección nunca sea null
            var incidentes = await _incidenteService.ObtenerTodosAsync(distritoId, estado);
            var modelo = incidentes ?? Enumerable.Empty<IncidenteEmergencia>();

            return View(modelo);
        }

        // POST: /Incidentes/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            await _incidenteService.CambiarEstadoAsync(id, nuevoEstado);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Incidentes/Registrar
        public async Task<IActionResult> Registrar()
        {
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre");
            return View(new IncidenteEmergencia());
        }

        // POST: /Incidentes/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(IncidenteEmergencia incidente)
        {
            if (ModelState.IsValid)
            {
                await _incidenteService.RegistrarIncidenteAsync(incidente);
                return RedirectToAction(nameof(Index));
            }

            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", incidente.DistritoId);
            return View(incidente);
        }
    }
}