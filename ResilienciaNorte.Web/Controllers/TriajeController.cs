using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;

namespace ResilienciaNorte.Web.Controllers
{
    // Triaje del Evaluador de Campo: constata o desestima las incidencias que le fueron asignadas.
    [Authorize(Roles = "EvaluadorCampo,Administrador")]
    public class TriajeController : Controller
    {
        private readonly IIncidenciaService _incidenciaService;
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly IHubContext<EmergenciaHub> _hubContext;
        private readonly IAuditoriaService _auditoria;

        public TriajeController(IIncidenciaService incidenciaService, UserManager<UsuarioAplicacion> userManager, IHubContext<EmergenciaHub> hubContext, IAuditoriaService auditoria)
        {
            _incidenciaService = incidenciaService;
            _userManager = userManager;
            _hubContext = hubContext;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index()
        {
            // El evaluador ve solo las suyas; el administrador ve todas las asignadas
            string? evaluadorId = User.IsInRole("Administrador") ? null : _userManager.GetUserId(User);
            return View(await _incidenciaService.ObtenerDelEvaluadorAsync(evaluadorId));
        }

        // Constatar se confirma con la ficha EDAN: los totales verificados en campo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Constatar(int id, int familiasAfectadas, int viviendasAfectadas, int personasAfectadas,
            string? necesidadesUrgentes, string? observaciones, DateTime fechaVisita)
        {
            var ficha = new FichaEdanDatos(familiasAfectadas, viviendasAfectadas, personasAfectadas,
                necesidadesUrgentes, observaciones, fechaVisita);
            return ResolverAsync(id, constatada: true, observacion: null, ficha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Desestimar(int id, string observacion)
            => ResolverAsync(id, constatada: false, observacion, ficha: null);

        private async Task<IActionResult> ResolverAsync(int id, bool constatada, string? observacion, FichaEdanDatos? ficha)
        {
            try
            {
                var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
                bool esAdministrador = User.IsInRole("Administrador");
                var incidencia = await _incidenciaService.ResolverAsync(id, usuarioId, esAdministrador, constatada, observacion, ficha);

                // Avisa al seguimiento de cada ciudadano que su reporte cambió de estado
                string nuevoEstado = constatada ? EstadoIncidente.Constatado : EstadoIncidente.Desestimado;
                foreach (var reporte in incidencia.Reportes)
                {
                    await _hubContext.Clients.All.SendAsync("EstadoIncidenteActualizado", new
                    {
                        codigo = reporte.CodigoIncidente,
                        nuevoEstado
                    });
                }

                TempData["MensajeExito"] = $"{incidencia.Codigo} {(constatada ? "constatada" : "desestimada")}. {incidencia.Reportes.Count} reporte(s) actualizados.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Incidentes",
                    constatada
                        ? $"Incidencia {incidencia.Codigo} constatada · ficha: {ficha?.FamiliasAfectadas} familias"
                        : $"Incidencia {incidencia.Codigo} desestimada: {observacion}");
            }
            catch (InvalidOperationException ex)
            {
                TempData["MensajeError"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
