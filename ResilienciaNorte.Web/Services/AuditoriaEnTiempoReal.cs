using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ResilienciaNorte.Service;
using ResilienciaNorte.Web.Hubs;

namespace ResilienciaNorte.Web.Services
{
    // Decorador de la bitácora: guarda el registro y avisa en vivo a las pantallas abiertas
    public class AuditoriaEnTiempoReal : IAuditoriaService
    {
        private readonly AuditoriaService _auditoria;
        private readonly IHubContext<EmergenciaHub> _hubContext;

        public AuditoriaEnTiempoReal(AuditoriaService auditoria, IHubContext<EmergenciaHub> hubContext)
        {
            _auditoria = auditoria;
            _hubContext = hubContext;
        }

        public async Task RegistrarAsync(ClaimsPrincipal usuario, string? ip, string operacion, string modulo, string detalle)
        {
            await _auditoria.RegistrarAsync(usuario, ip, operacion, modulo, detalle);
            await AvisarAsync(modulo, detalle);
        }

        public async Task RegistrarAsync(string usuario, string rol, string? ip, string operacion, string modulo, string detalle)
        {
            await _auditoria.RegistrarAsync(usuario, rol, ip, operacion, modulo, detalle);
            await AvisarAsync(modulo, detalle);
        }

        private Task AvisarAsync(string modulo, string detalle) =>
            _hubContext.Clients.All.SendAsync("CambioOperativo", new { modulo, detalle });
    }
}
