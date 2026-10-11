using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    [Table("Almacenes")]
    public class Almacen
    {
        [Key]
        public int AlmacenId { get; set; }

        [Required]
        [StringLength(80)]
        public string Nombre { get; set; } = string.Empty;

        // Regional, Provincial o Distrital
        [Required]
        [StringLength(20)]
        public string Nivel { get; set; } = string.Empty;

        // Ubicación: el distrito solo aplica al almacén distrital
        public int? DistritoId { get; set; }
        public Distrito? Distrito { get; set; }

        // Ubicación: la provincia aplica a distrital y provincial
        public int? ProvinciaId { get; set; }
        public Provincia? Provincia { get; set; }

        // Almacén al que se le pide abastecimiento (null en el regional)
        public int? AlmacenPadreId { get; set; }
        public Almacen? AlmacenPadre { get; set; }

        public ICollection<ExistenciaAlmacen> Existencias { get; set; } = new List<ExistenciaAlmacen>();

        // Ruta jerárquica para mostrar la ubicación sin repetir nombres
        public string RutaUbicacion => Nivel switch
        {
            "Distrital" => $"Región La Libertad › Provincia {Provincia?.Nombre} › Distrito {Distrito?.Nombre}",
            "Provincial" => $"Región La Libertad › Provincia {Provincia?.Nombre}",
            _ => "Región La Libertad"
        };
    }
}
