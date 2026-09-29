using System;

namespace ResilienciaNorte.Domain
{
    public class AuditoriaSistema
    {
        public long AuditoriaId { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string RolUsuario { get; set; } = string.Empty;

        // 'INSERT', 'UPDATE', 'DELETE', 'LOGIN_2FA', 'QUIEBRE_STOCK'
        public string Operacion { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty; // 'Almacen', 'Incidentes', 'Ordenes', 'Seguridad'
        public string Detalle { get; set; } = string.Empty;
        public string? IpCliente { get; set; }
        public DateTime FechaHoraUtc { get; set; } = DateTime.UtcNow;
    }
}