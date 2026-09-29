using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers
{
    public class RecursosController : Controller
    {
        private readonly IRecursoService _recursoService;
        private readonly IIncidenteService _incidenteService;

        public RecursosController(IRecursoService recursoService, IIncidenteService incidenteService)
        {
            _recursoService = recursoService;
            _incidenteService = incidenteService;
        }

        // GET: /Recursos/Index
        public async Task<IActionResult> Index(int? distritoId)
        {
            // 1. Cargar distritos para el filtro del ViewBag
            var distritos = await _incidenteService.ObtenerDistritosAsync();
            ViewBag.Distritos = new SelectList(distritos ?? Enumerable.Empty<Distrito>(), "DistritoId", "Nombre", distritoId);

            // 2. Obtener el inventario BAH garantizando no-null
            var recursos = await _recursoService.ObtenerInventarioAsync(distritoId);
            var modelo = recursos ?? Enumerable.Empty<RecursoAlmacen>();

            return View(modelo);
        }
    }
}