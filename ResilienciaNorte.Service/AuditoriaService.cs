using System.Security.Claims;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ResilienciaDbContext _context;

        public AuditoriaService(ResilienciaDbContext context)
        {
            _context = context;
        }

        public async Task RegistrarAsync(ClaimsPrincipal usuario, string? ip, string operacion, string modulo, string detalle)
        {
            // Sin sesión (visitante o ciudadano anónimo) el registro queda como "anónimo"
            string nombre = usuario.Identity?.IsAuthenticated == true ? usuario.Identity.Name ?? "sistema" : "anónimo";
            string rol = usuario.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            await RegistrarAsync(nombre, rol, ip, operacion, modulo, detalle);
        }

        public async Task RegistrarAsync(string usuario, string rol, string? ip, string operacion, string modulo, string detalle)
        {
            // Los límites coinciden con la tabla: un texto largo se recorta en vez de fallar la operación
            _context.AuditoriaSistema.Add(new AuditoriaSistema
            {
                UsuarioId = Recortar(usuario, 50),
                RolUsuario = Recortar(rol, 30),
                Operacion = Recortar(operacion, 50),
                Modulo = Recortar(modulo, 50),
                Detalle = Recortar(detalle, 500),
                IpCliente = ip,
                FechaHoraUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private static string Recortar(string texto, int largo) =>
            texto.Length <= largo ? texto : texto[..largo];
    }
}
