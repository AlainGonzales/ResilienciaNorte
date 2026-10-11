using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;
using ResilienciaNorte.Service;

namespace ResilienciaNorte.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ResilienciaDbContext _context;
        private readonly IAuditoriaService _auditoria;

        public AdminController(
            UserManager<UsuarioAplicacion> userManager,
            RoleManager<IdentityRole> roleManager,
            ResilienciaDbContext context,
            IAuditoriaService auditoria)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Usuarios(string? nivel, int? distritoId)
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
                    DistritoId = user.DistritoId,
                    DistritoNombre = user.Distrito?.Nombre ?? "Provincial / Todas",
                    Activo = user.Activo
                });
            }

            // Filtros: por nivel jurisdiccional y, si es distrital, por sede
            if (!string.IsNullOrEmpty(nivel))
                modelo = modelo.Where(u => u.NivelJurisdiccion == nivel).ToList();
            if (nivel == "Distrital" && distritoId != null)
                modelo = modelo.Where(u => u.DistritoId == distritoId).ToList();
            ViewBag.NivelSeleccionado = nivel ?? string.Empty;
            ViewBag.DistritoSeleccionado = nivel == "Distrital" ? distritoId : null;

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

            if (nivel == "Distrital" && distritoId == null)
            {
                TempData["MensajeError"] = "Un usuario distrital requiere su distrito.";
                return RedirectToAction(nameof(Usuarios));
            }

            // 1. Actualizar roles
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, nuevoRol);

            // 2. Actualizar Jurisdicción
            usuario.NivelJurisdiccion = nivel;
            usuario.DistritoId = (nivel == "Distrital") ? distritoId : null;
            await _userManager.UpdateAsync(usuario);

            TempData["Mensaje"] = $"Permisos de {usuario.NombreCompleto} actualizados a {nuevoRol} ({nivel}).";
            await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "UPDATE", "Seguridad",
                $"Permisos de {usuario.Email} cambiados a {nuevoRol} ({nivel})");
            return RedirectToAction(nameof(Usuarios));
        }

        // Crea personal operativo (nunca ciudadanos). El correo y la contraseña salen del rol y la jurisdicción.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(string dni, string nombreCompleto, string rol, string nivel, int? distritoId)
        {
            var rolesPermitidos = new[] { "Administrador", "Coordinador", "Analista", "EvaluadorCampo", "LogisticoAlmacen" };
            if (!rolesPermitidos.Contains(rol))
                return ErrorYVolver("Rol no válido para personal operativo.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(dni ?? string.Empty, @"^\d{8}$"))
                return ErrorYVolver("El DNI debe tener 8 dígitos.");

            if (await _userManager.Users.AnyAsync(u => u.Dni == dni))
                return ErrorYVolver("Ya existe un usuario con ese DNI.");

            Distrito? distrito = null;
            if (nivel == "Distrital")
            {
                if (distritoId == null)
                    return ErrorYVolver("Un usuario distrital requiere su distrito.");
                distrito = await _context.Distritos.FindAsync(distritoId.Value);
                if (distrito == null)
                    return ErrorYVolver("El distrito seleccionado no existe.");
            }

            // Lugar y dominio según la jurisdicción: distrital (COEL), provincial (COEP) o regional (COER)
            string lugar = nivel switch
            {
                "Distrital" => Slug(distrito!.Nombre),
                "Provincial" => "trujillo",
                _ => "lalibertad"
            };
            string dominio = nivel switch
            {
                "Distrital" => "coel.gob.pe",
                "Provincial" => "coep.gob.pe",
                _ => "coer.gob.pe"
            };

            string email = await CorreoDisponibleAsync($"{Slug(rol)}.{lugar}", dominio);
            string password = $"{rol}{char.ToUpperInvariant(lugar[0])}{lugar[1..]}{Random.Shared.Next(1000, 9999)}*";

            var usuario = new UsuarioAplicacion
            {
                UserName = email,
                Email = email,
                Dni = dni!,
                NombreCompleto = nombreCompleto.Trim(),
                NivelJurisdiccion = nivel,
                DistritoId = nivel == "Distrital" ? distritoId : null,
                Activo = true,
                EmailConfirmed = true,
                FechaRegistro = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(usuario, password);
            if (!resultado.Succeeded)
                return ErrorYVolver(string.Join(" ", resultado.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(usuario, rol);

            // Se muestra una sola vez para que el administrador las entregue
            TempData["Credenciales"] = $"{usuario.NombreCompleto} · Correo: {email} · Contraseña: {password}";
            await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Seguridad",
                $"Usuario creado: {email} · {rol} · {nivel}{(distrito != null ? " " + distrito.Nombre : "")}");
            return RedirectToAction(nameof(Usuarios));
        }

        private IActionResult ErrorYVolver(string mensaje)
        {
            TempData["MensajeError"] = mensaje;
            return RedirectToAction(nameof(Usuarios));
        }

        private async Task<string> CorreoDisponibleAsync(string baseLocal, string dominio)
        {
            string candidato = $"{baseLocal}@{dominio}";
            int numero = 2;
            while (await _userManager.FindByEmailAsync(candidato) != null)
            {
                candidato = $"{baseLocal}{numero}@{dominio}";
                numero++;
            }
            return candidato;
        }

        // "El Porvenir" -> "elporvenir"; "Víctor Larco" -> "victorlarco"
        private static string Slug(string texto)
        {
            var descompuesto = texto.Normalize(System.Text.NormalizationForm.FormD);
            var sinTildes = new string(descompuesto
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());
            return new string(sinTildes.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        }

        public async Task<IActionResult> Auditoria(string? buscar)
        {
            var consulta = _context.AuditoriaSistema.AsQueryable();

            // Búsqueda de texto libre en todas las columnas (sin distinguir mayúsculas)
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                string texto = buscar.Trim();
                consulta = consulta.Where(a =>
                    a.UsuarioId.Contains(texto) || a.RolUsuario.Contains(texto) ||
                    a.Operacion.Contains(texto) || a.Modulo.Contains(texto) ||
                    a.Detalle.Contains(texto) || (a.IpCliente ?? "").Contains(texto));
            }

            ViewBag.Buscar = buscar ?? string.Empty;
            var logs = await consulta
                .OrderByDescending(a => a.FechaHoraUtc)
                .Take(300)
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
        public int? DistritoId { get; set; }
        public string DistritoNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}