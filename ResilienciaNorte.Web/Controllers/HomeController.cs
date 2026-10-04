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
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario != null)
                {
                    // Si el usuario registrado consulta, recuperamos todas sus alertas por DNI
                    var misReportes = await _context.IncidentesEmergencia
                        .Include(i => i.Distrito)
                        .Where(i => i.DniCiudadano == usuario.Dni)
                        .OrderByDescending(i => i.FechaRegistro)
                        .ToListAsync();

                    ViewBag.MisReportes = misReportes;
                    ViewBag.UsuarioActual = usuario;
                }
            }

            return View();
        }
    }
}