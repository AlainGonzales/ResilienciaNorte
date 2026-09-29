using System;

namespace ResilienciaNorte.Domain
{
    public class SolicitudReabastecimiento
    {
        public int SolicitudId { get; set; }
        public string CodigoSolicitud { get; set; } = string.Empty;

        // Distrito que sufre el quiebre de stock
        public int DistritoOrigenId { get; set; }
        public Distrito? DistritoOrigen { get; set; }

        // Destino al que se le pide auxilio: 'Provincial' o 'Regional'
        public string NivelDestino { get; set; } = string.Empty;

        public int RecursoId { get; set; }
        public RecursoAlmacen? Recurso { get; set; }

        public int CantidadSolicitada { get; set; }
        public string Justificacion { get; set; } = string.Empty;

        // Estados: Pendiente, Aprobado, EnTransito, Recepcionado, Rechazado
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaAtencion { get; set; }
        public string? UsuarioAprobadorId { get; set; }
    }
}