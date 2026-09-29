using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
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

        [Range(0, 1000000)]
        public int StockDisponible { get; set; } = 0;

        [Range(0, 100000)]
        public int StockMinimo { get; set; } = 10;

        // Clave foránea al distrito custodio
        [Required]
        public int DistritoId { get; set; }

        [ForeignKey(nameof(DistritoId))]
        public Distrito? Distrito { get; set; }

        // Métodos de dominio (Experto en información)
        public bool EsStockCritico() => StockDisponible <= StockMinimo;

        public void DescontarStock(int cantidad)
        {
            if (cantidad > StockDisponible)
                throw new System.InvalidOperationException($"Stock insuficiente para {Nombre}.");
            StockDisponible -= cantidad;
        }

        public void IncrementarStock(int cantidad)
        {
            StockDisponible += cantidad;
        }
    }
}