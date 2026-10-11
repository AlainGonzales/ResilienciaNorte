using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    // Ficha EDAN: verificación en campo de una incidencia. Se guarda al constatarla.
    [Table("FichasEdan")]
    public class FichaEdan
    {
        [Key]
        public int FichaEdanId { get; set; }

        public int IncidenciaId { get; set; }
        public Incidencia? Incidencia { get; set; }

        // Totales reales verificados en campo (reemplazan la suma de los reportes)
        public int FamiliasAfectadas { get; set; }
        public int ViviendasAfectadas { get; set; }
        public int PersonasAfectadas { get; set; }

        [StringLength(300)]
        public string? NecesidadesUrgentes { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public DateTime FechaVisita { get; set; }

        public string UsuarioEvaluadorId { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
