using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain;

[Table("RecursosAlmacen")]
public class RecursoAlmacen
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string CodigoBAH { get; set; } = string.Empty; // Código de Bienes de Ayuda Humanitaria

    [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UnidadMedida { get; set; } = "Unidades"; // Unidades, Rollos, Equipos

    [Range(0, 100000)]
    public int StockDisponible { get; set; }

    [Range(0, 10000)]
    public int StockMinimoSeguridad { get; set; }

    // Clave foránea al distrito donde está ubicado el almacén de contingencia
    [Required]
    public int DistritoId { get; set; }

    [ForeignKey(nameof(DistritoId))]
    public Distrito? Distrito { get; set; }
}