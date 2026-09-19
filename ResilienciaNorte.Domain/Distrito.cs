using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain;

[Table("Distritos")]
public class Distrito
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del distrito es obligatorio.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string ZonaVulnerablePrincipal { get; set; } = string.Empty;

    public int PoblacionEstimada { get; set; }

    public bool Activo { get; set; } = true;

    // Relaciones
    public ICollection<IncidenteEmergencia> Incidentes { get; set; } = new List<IncidenteEmergencia>();
    public ICollection<RecursoAlmacen> Recursos { get; set; } = new List<RecursoAlmacen>();
}