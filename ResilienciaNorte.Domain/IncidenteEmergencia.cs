using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain;

[Table("IncidentesEmergencia")]
public class IncidenteEmergencia
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título o resumen del incidente es obligatorio.")]
    [StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción de los hechos es obligatoria.")]
    [StringLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de incidente es obligatorio.")]
    [StringLength(50)]
    public string TipoDesastre { get; set; } = "Inundación Pluvial"; // Inundación Pluvial, Desborde de Quebrada, Colapso de Drenaje

    [Required(ErrorMessage = "El nivel de severidad es obligatorio.")]
    [StringLength(20)]
    public string NivelSeveridad { get; set; } = "Moderado"; // Moderado, Alto, Crítico

    [Required(ErrorMessage = "La ubicación o referencia exacta es obligatoria.")]
    [StringLength(200)]
    public string DireccionReferencia { get; set; } = string.Empty;

    [Display(Name = "Familias Afectadas")]
    [Range(0, 10000, ErrorMessage = "El número de familias debe ser positivo.")]
    public int FamiliasAfectadas { get; set; }

    public DateTime FechaReporte { get; set; } = DateTime.Now;

    [StringLength(30)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, En Evaluación, Atendido

    // Clave foránea hacia Distrito
    [Required(ErrorMessage = "Debe seleccionar un distrito.")]
    public int DistritoId { get; set; }

    [ForeignKey(nameof(DistritoId))]
    public Distrito? Distrito { get; set; }
}