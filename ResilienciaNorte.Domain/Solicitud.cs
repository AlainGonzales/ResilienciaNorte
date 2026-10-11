using System;

namespace ResilienciaNorte.Domain
{
    // Un solo proceso para abastecimiento, compra y donación.
    public class Solicitud
    {
        public int SolicitudId { get; set; }
        public string CodigoSolicitud { get; set; } = string.Empty;

        // Abastecimiento, Compra o Donacion
        public string Tipo { get; set; } = string.Empty;

        // Almacén que recibe los bienes
        public int AlmacenSolicitanteId { get; set; }
        public Almacen? AlmacenSolicitante { get; set; }

        // Almacén que entrega (solo abastecimiento)
        public int? AlmacenProveedorId { get; set; }
        public Almacen? AlmacenProveedor { get; set; }

        public int RecursoId { get; set; }
        public RecursoAlmacen? Recurso { get; set; }

        public int CantidadSolicitada { get; set; }
        public string Justificacion { get; set; } = string.Empty;

        // Abastecimiento/Incidencia: Pendiente, Aprobada, Rechazada, Despachada. Compra/Donacion: Pendiente, Recepcionado
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime? FechaAtencion { get; set; }
        public string? UsuarioAprobadorId { get; set; }

        // Solicitud de bienes de una incidencia constatada, y motivo si el coordinador la rechaza
        public int? IncidenciaId { get; set; }
        public Incidencia? Incidencia { get; set; }

        public string? MotivoRechazo { get; set; }
    }
}
