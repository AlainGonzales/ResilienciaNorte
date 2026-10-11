using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Models;

namespace ResilienciaNorte.Web.Controllers
{
    public class CuentasController : Controller
    {
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly SignInManager<UsuarioAplicacion> _signInManager;
        private readonly IAuditoriaService _auditoria;

        public CuentasController(
            UserManager<UsuarioAplicacion> userManager,
            SignInManager<UsuarioAplicacion> signInManager,
            IAuditoriaService auditoria)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditoria = auditoria;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorLogin"] = "Por favor ingrese su usuario y contraseña.";
                return RedirectToAction("Index", "Home");
            }

            var usuario = await _userManager.FindByEmailAsync(model.EmailODni)
                          ?? await _userManager.Users.FirstOrDefaultAsync(u => u.Dni == model.EmailODni);

            if (usuario == null || !usuario.Activo)
            {
                TempData["ErrorLogin"] = "Credenciales inválidas o cuenta inactiva.";
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "LOGIN_FALLIDO", "Seguridad",
                    $"Cuenta inexistente o inactiva: {model.EmailODni}");
                return RedirectToAction("Index", "Home");
            }

            var resultado = await _signInManager.PasswordSignInAsync(usuario, model.Password, model.Recordarme, lockoutOnFailure: false);
            if (!resultado.Succeeded)
                await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "LOGIN_FALLIDO", "Seguridad",
                    $"Contraseña incorrecta: {usuario.Email}");

            if (resultado.Succeeded)
            {
                await _auditoria.RegistrarAsync(usuario.Email ?? usuario.UserName ?? "sistema",
                    string.Join(",", await _userManager.GetRolesAsync(usuario)),
                    HttpContext.Connection.RemoteIpAddress?.ToString(), "LOGIN", "Seguridad", "Inicio de sesión");

                // 1. Administrador -> Control de Usuarios
                if (await _userManager.IsInRoleAsync(usuario, "Administrador"))
                {
                    return RedirectToAction("Usuarios", "Admin");
                }

                // 2. Almacenero / Logístico -> Despachos (lo aprobado que debe salir)
                if (await _userManager.IsInRoleAsync(usuario, "LogisticoAlmacen"))
                {
                    return RedirectToAction("Solicitudes", "Recursos", new { estado = "Aprobada" });
                }

                // 3. Evaluador de Campo -> Triaje de sus incidencias
                if (await _userManager.IsInRoleAsync(usuario, "EvaluadorCampo"))
                {
                    return RedirectToAction("Index", "Triaje");
                }

                // 4b. Coordinador -> Solicitudes de su almacén (su pantalla principal)
                if (await _userManager.IsInRoleAsync(usuario, "Coordinador"))
                {
                    return RedirectToAction("Solicitudes", "Recursos");
                }

                // 4a. Analista -> Monitoreo de Reportes
                if (await _userManager.IsInRoleAsync(usuario, "Analista"))
                {
                    return RedirectToAction("Index", "Incidentes");
                }

                // 4. Operador COEP / Coordinadores -> Panel Operativo
                if (!await _userManager.IsInRoleAsync(usuario, "Ciudadano"))
                {
                    return RedirectToAction("Index", "Home");
                }

                // 5. Ciudadano -> Portal de Reportes
                return RedirectToAction("Portal", "Incidentes");
            }

            TempData["ErrorLogin"] = "Contraseña incorrecta.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroCiudadano(RegistroCiudadanoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorRegistro"] = "Verifique los datos ingresados en el formulario.";
                return RedirectToAction("Index", "Home");
            }

            // Validar unicidad del DNI
            var existeDni = await _userManager.Users.AnyAsync(u => u.Dni == model.Dni.Trim());
            if (existeDni)
            {
                TempData["ErrorRegistro"] = "El DNI ingresado ya se encuentra registrado.";
                return RedirectToAction("Index", "Home");
            }

            var nuevoUsuario = new UsuarioAplicacion
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                Dni = model.Dni.Trim(),
                PhoneNumber = model.Telefono.Trim(),
                NombreCompleto = model.NombreCompleto.Trim(),
                NivelJurisdiccion = "Ciudadano",
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            var resultado = await _userManager.CreateAsync(nuevoUsuario, model.Password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(nuevoUsuario, "Ciudadano");
                await _signInManager.SignInAsync(nuevoUsuario, isPersistent: false);

                TempData["ExitoRegistro"] = $"¡Registro exitoso! Bienvenido al sistema, {nuevoUsuario.NombreCompleto}.";
                await _auditoria.RegistrarAsync(nuevoUsuario.Email ?? "sistema", "Ciudadano",
                    HttpContext.Connection.RemoteIpAddress?.ToString(), "INSERT", "Seguridad",
                    $"Registro de ciudadano · DNI {nuevoUsuario.Dni}");
                return RedirectToAction("Index", "Home");
            }

            // Identity devuelve el mensaje en inglés: el correo repetido se explica en español
            bool correoRepetido = resultado.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail");
            TempData["ErrorRegistro"] = correoRepetido
                ? "Ya existe una cuenta registrada con ese correo electrónico. Inicie sesión o use otro correo."
                : string.Join(" ", resultado.Errors.Select(e => e.Description));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _auditoria.RegistrarAsync(User, HttpContext.Connection.RemoteIpAddress?.ToString(), "LOGOUT", "Seguridad", "Cierre de sesión");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}