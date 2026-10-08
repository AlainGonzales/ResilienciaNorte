using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;

namespace ResilienciaNorte.Web.Controllers
{
    // Consola móvil del Evaluador de Campo (triaje EDAN): marcar cada alerta como Constatado o Desestimado.
    public class EvaluacionController : Controller
    {
        private const string EvaluadorPorDefecto = "Evaluador de Campo";

        private readonly IEvaluacionService _evaluacionService;
        private readonly IHubContext<EmergenciaHub> _hubContext;

        public EvaluacionController(IEvaluacionService evaluacionService, IHubContext<EmergenciaHub> hubContext)
        {
            _evaluacionService = evaluacionService;
            _hubContext = hubContext;
        }

        // Cola de alertas pendientes, ordenada por prioridad
        public async Task<IActionResult> Consola()
        {
            ViewBag.Resumen = await _evaluacionService.ObtenerResumenAsync();
            var pendientes = await _evaluacionService.ObtenerPendientesAsync();
            return View(pendientes);
        }

        // Fragmento HTML de una tarjeta: lo usa la consola cuando SignalR avisa de una alerta nueva
        public async Task<IActionResult> Tarjeta(int id)
        {
            var incidente = await _evaluacionService.ObtenerPendientePorIdAsync(id);
            if (incidente == null) return NotFound();
            return PartialView("_TarjetaIncidente", incidente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Constatar(int id, string? observacion)
            => EvaluarAsync(id, constatar: true, observacion);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Desestimar(int id, string? observacion)
            => EvaluarAsync(id, constatar: false, observacion);

        private async Task<IActionResult> EvaluarAsync(int id, bool constatar, string? observacion)
        {
            string evaluador = User.Identity?.Name ?? EvaluadorPorDefecto;
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            var resultado = constatar
                ? await _evaluacionService.ConstatarAsync(id, evaluador, observacion, ip)
                : await _evaluacionService.DesestimarAsync(id, evaluador, observacion, ip);

            if (resultado.Exito)
            {
                // Misma señal que usa CambiarEstado: actualiza el seguimiento del ciudadano y las demás consolas
                await _hubContext.Clients.All.SendAsync("EstadoIncidenteActualizado", new
                {
                    codigo = resultado.CodigoIncidente,
                    nuevoEstado = resultado.NuevoEstado
                });
            }

            return Json(new
            {
                ok = resultado.Exito,
                mensaje = resultado.Mensaje,
                codigo = resultado.CodigoIncidente,
                nuevoEstado = resultado.NuevoEstado
            });
        }
    }
}
