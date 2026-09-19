using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers;

public class RecursosController : Controller
{
    private readonly IRecursoService _recursoService;
    private readonly IIncidenteService _incidenteService;

    public RecursosController(IRecursoService recursoService, IIncidenteService incidenteService)
    {
        _recursoService = recursoService;
        _incidenteService = incidenteService;
    }

    // GET: /Recursos
    public async Task<IActionResult> Index(int? distritoId)
    {
        var inventario = await _recursoService.ObtenerInventarioAsync(distritoId);
        var distritos = await _incidenteService.ObtenerDistritosAsync();

        ViewBag.Distritos = new SelectList(distritos, "Id", "Nombre", distritoId);
        return View(inventario);
    }
}