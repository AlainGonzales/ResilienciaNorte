using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    // Catálogo de Bienes de Ayuda Humanitaria. El stock vive en ExistenciaAlmacen.
    [Table("RecursosAlmacen")]
    public class RecursoAlmacen
    {
        [Key]
        public int RecursoId { get; set; }

        [Required]
        [StringLength(20)]
        public string CodigoBAH { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty; // Techo, Abrigo, Herramientas, Alimentos

        [Required]
        [StringLength(20)]
        public string UnidadMedida { get; set; } = "Unidades"; // Unidades, Rollos, Planchas, Kits
    }
}
