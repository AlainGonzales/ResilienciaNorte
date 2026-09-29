using System;
using System.Collections.Generic;

namespace ResilienciaNorte.Domain
{
    public class OrdenAtencion
    {
        public int OrdenId { get; set; }
        public string CodigoOrden { get; set; } = string.Empty;

        // Relación 1 a 1 con el Incidente
        public int IncidenteId { get; set; }
        public IncidenteEmergencia? Incidente { get; set; }

        // Auditoría operativa
        public string UsuarioAnalistaId { get; set; } = string.Empty;
        public string? UsuarioCoordinadorId { get; set; }

        // Estados: Aprobada, EnDespacho, Entregado, Anulado
        public string Estado { get; set; } = "Aprobada";
        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
        public DateTime? FechaEntrega { get; set; }
        public string? Observaciones { get; set; }

        // Detalle de bienes (Maestro-Detalle)
        public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();
    }
}