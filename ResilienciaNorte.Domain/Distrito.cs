using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    [Table("Distritos")]
    public class Distrito
    {
        [Key]
        public int DistritoId { get; set; }

        [Required(ErrorMessage = "El nombre del distrito es obligatorio.")]
        [StringLength(60)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string NivelRiesgo { get; set; } = "Medio"; // Alto, Medio, Bajo

        [StringLength(120)]
        public string? UbicacionCOEL { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones de navegación
        public ICollection<IncidenteEmergencia> Incidentes { get; set; } = new List<IncidenteEmergencia>();
        public ICollection<RecursoAlmacen> Recursos { get; set; } = new List<RecursoAlmacen>();
    }
}