using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ResilienciaDbContext _context;
        private readonly UserManager<UsuarioAplicacion> _userManager;

        public HomeController(ResilienciaDbContext context, UserManager<UsuarioAplicacion> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Si es Administrador, su pantalla principal nativa es Control de Usuarios
                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("Usuarios", "Admin");
                }

                var usuario = await _userManager.GetUserAsync(User);
                ViewBag.UsuarioActual = usuario;

                if (usuario != null)
                {
                    var roles = await _userManager.GetRolesAsync(usuario);
                    string rolPrincipal = roles.FirstOrDefault() ?? "Ciudadano";
                    ViewBag.RolPrincipal = rolPrincipal;

                    // Ciudadano logueado → redirigir a su portal de reportes
                    if (rolPrincipal == "Ciudadano")
                    {
                        return RedirectToAction("Portal", "Incidentes");
                    }

                    // Funcionarios operativos (Coordinador, Analista, Logístico)
                    ViewBag.TotalAlertas = await _context.IncidentesEmergencia.CountAsync();
                    ViewBag.AlertasCriticas = await _context.IncidentesEmergencia.CountAsync(i => i.Severidad == "Crítico" || i.Severidad == "Crítica");
                    ViewBag.FamiliasImpactadas = await _context.IncidentesEmergencia.SumAsync(i => (int?)i.FamiliasAfectadas) ?? 0;
                    ViewBag.TotalBienesBAH = await _context.RecursosAlmacen.SumAsync(r => (int?)r.StockDisponible) ?? 0;
                    ViewBag.UltimosIncidentes = await _context.IncidentesEmergencia
                        .Include(i => i.Distrito)
                        .OrderByDescending(i => i.FechaRegistro)
                        .Take(6)
                        .ToListAsync();
                }
            }

            return View();
        }
    }
}