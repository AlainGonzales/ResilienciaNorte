using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ResilienciaDbContext _context;

        public AdminController(
            UserManager<UsuarioAplicacion> userManager,
            RoleManager<IdentityRole> roleManager,
            ResilienciaDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _userManager.Users.Include(u => u.Distrito).ToListAsync();
            var modelo = new List<UsuarioGestionViewModel>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                modelo.Add(new UsuarioGestionViewModel
                {
                    Id = user.Id,
                    Dni = user.Dni,
                    NombreCompleto = user.NombreCompleto,
                    Email = user.Email ?? "",
                    RolActual = roles.FirstOrDefault() ?? "Ciudadano",
                    NivelJurisdiccion = user.NivelJurisdiccion,
                    DistritoNombre = user.Distrito?.Nombre ?? "Provincial / Todas",
                    Activo = user.Activo
                });
            }

            ViewBag.Distritos = await _context.Distritos.ToListAsync();
            ViewBag.RolesDisponibles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPermisos(string usuarioId, string nuevoRol, string nivel, int? distritoId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null) return NotFound();

            // 1. Actualizar roles
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, nuevoRol);

            // 2. Actualizar Jurisdicción
            usuario.NivelJurisdiccion = nivel;
            usuario.DistritoId = (nivel == "Distrital") ? distritoId : null;
            await _userManager.UpdateAsync(usuario);

            TempData["Mensaje"] = $"Permisos de {usuario.NombreCompleto} actualizados a {nuevoRol} ({nivel}).";
            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> Auditoria()
        {
            var logs = await _context.AuditoriaSistema
                .OrderByDescending(a => a.FechaHoraUtc)
                .Take(100)
                .ToListAsync();
            return View(logs);
        }
    }

    public class UsuarioGestionViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RolActual { get; set; } = string.Empty;
        public string NivelJurisdiccion { get; set; } = string.Empty;
        public string DistritoNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}