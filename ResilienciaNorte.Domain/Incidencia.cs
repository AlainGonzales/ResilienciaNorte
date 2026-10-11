using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    // Agrupa reportes ciudadanos del mismo hecho en un mismo lugar.
    [Table("Incidencias")]
    public class Incidencia
    {
        [Key]
        public int IncidenciaId { get; set; }

        [Required]
        [StringLength(30)]
        public string Codigo { get; set; } = string.Empty;

        public int DistritoId { get; set; }
        public Distrito? Distrito { get; set; }

        [Required]
        [StringLength(100)]
        public string Sector { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoEvento { get; set; } = string.Empty;

        // Abierta, Asignada, Constatada, Desestimada
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = EstadoIncidencia.Abierta;

        [StringLength(300)]
        public string? Observacion { get; set; }

        public string? UsuarioEvaluadorId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaAsignacion { get; set; }

        // Resolución del evaluador: obligatoria al desestimar
        [StringLength(300)]
        public string? ObservacionResolucion { get; set; }

        public ICollection<IncidenteEmergencia> Reportes { get; set; } = new List<IncidenteEmergencia>();

        // Existe solo cuando el evaluador constató la incidencia
        public FichaEdan? FichaEdan { get; set; }
    }
}
