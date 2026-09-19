using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers;

public class IncidentesController : Controller
{
    private readonly IIncidenteService _incidenteService;

    public IncidentesController(IIncidenteService incidenteService)
    {
        _incidenteService = incidenteService;
    }

    // GET: /Incidentes
    public async Task<IActionResult> Index(int? distritoId, string? estado)
    {
        var incidentes = await _incidenteService.ObtenerTodosAsync(distritoId, estado);
        var distritos = await _incidenteService.ObtenerDistritosAsync();

        ViewBag.Distritos = new SelectList(distritos, "Id", "Nombre", distritoId);
        ViewBag.DistritoSeleccionado = distritoId;
        ViewBag.EstadoSeleccionado = estado;

        return View(incidentes);
    }

    // GET: /Incidentes/Registrar
    public async Task<IActionResult> Registrar()
    {
        var distritos = await _incidenteService.ObtenerDistritosAsync();
        ViewBag.Distritos = new SelectList(distritos, "Id", "Nombre");
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
            TempData["Mensaje"] = "Incidente reportado exitosamente. Se asignó prioridad y está en evaluación.";
            return RedirectToAction(nameof(Index));
        }

        var distritos = await _incidenteService.ObtenerDistritosAsync();
        ViewBag.Distritos = new SelectList(distritos, "Id", "Nombre", incidente.DistritoId);
        return View(incidente);
    }

    // POST: /Incidentes/CambiarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        await _incidenteService.CambiarEstadoAsync(id, nuevoEstado);
        TempData["Mensaje"] = $"El estado del incidente #{id} fue actualizado a '{nuevoEstado}'.";
        return RedirectToAction(nameof(Index));
    }
}