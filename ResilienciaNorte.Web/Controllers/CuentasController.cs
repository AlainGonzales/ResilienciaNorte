using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Web.Models;

namespace ResilienciaNorte.Web.Controllers
{
    public class CuentasController : Controller
    {
        private readonly UserManager<UsuarioAplicacion> _userManager;
        private readonly SignInManager<UsuarioAplicacion> _signInManager;

        public CuentasController(
            UserManager<UsuarioAplicacion> userManager,
            SignInManager<UsuarioAplicacion> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            // Permitir inicio de sesión con Email o DNI
            var usuario = await _userManager.FindByEmailAsync(model.EmailODni)
                          ?? await _userManager.Users.FirstOrDefaultAsync(u => u.Dni == model.EmailODni);

            if (usuario == null || !usuario.Activo)
            {
                TempData["ErrorLogin"] = "Credenciales inválidas o cuenta inactiva.";
                return RedirectToAction("Index", "Home");
            }

            var resultado = await _signInManager.PasswordSignInAsync(usuario, model.Password, model.Recordarme, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
            }

            TempData["ErrorRegistro"] = string.Join(" ", resultado.Errors.Select(e => e.Description));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}