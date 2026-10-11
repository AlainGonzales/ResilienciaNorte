using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    [Table("Provincias")]
    public class Provincia
    {
        [Key]
        public int ProvinciaId { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; } = string.Empty;
    }
}
