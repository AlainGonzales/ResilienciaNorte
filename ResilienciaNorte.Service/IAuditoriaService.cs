using System.Security.Claims;

namespace ResilienciaNorte.Service
{
    // Bitácora: registra qué hizo cada usuario en las operaciones críticas
    public interface IAuditoriaService
    {
        // Con sesión (o anónimo, si el ClaimsPrincipal no está autenticado).
        // operacion: INSERT, UPDATE, LOGIN, LOGOUT, LOGIN_FALLIDO, OTP_VALIDO, OTP_FALLIDO...
        Task RegistrarAsync(ClaimsPrincipal usuario, string? ip, string operacion, string modulo, string detalle);

        // Cuando todavía no hay sesión: login recién hecho, login fallido o registro
        Task RegistrarAsync(string usuario, string rol, string? ip, string operacion, string modulo, string detalle);
    }
}
