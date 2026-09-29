using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResilienciaNorte.Domain
{
    [Table("IncidentesEmergencia")]
    public class IncidenteEmergencia
    {
        [Key]
        public int IncidenteId { get; set; }

        [Required]
        [StringLength(30)]
        public string CodigoIncidente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 dígitos.")]
        public string DniCiudadano { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del ciudadano es obligatorio.")]
        [StringLength(120)]
        public string NombreCiudadano { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "Debe especificar el sector o quebrada.")]
        [StringLength(100)]
        public string SectorCritico { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TipoEvento { get; set; } = "Inundación Pluvial"; // Desborde Quebrada, Lluvia Torrencial

        [Range(0, 50000, ErrorMessage = "El número de familias debe ser positivo.")]
        public int FamiliasAfectadas { get; set; } = 0;

        [Required(ErrorMessage = "La descripción de los daños es obligatoria.")]
        [StringLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DireccionReferencia { get; set; }

        [StringLength(20)]
        public string Severidad { get; set; } = "Moderado"; // Crítico, Grave, Moderado

        [StringLength(20)]
        public string Estado { get; set; } = "Reportado"; // Reportado, Constatado, Desestimado, ConAsignacion, Atendido

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Clave foránea hacia Distrito
        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        public int DistritoId { get; set; }

        [ForeignKey(nameof(DistritoId))]
        public Distrito? Distrito { get; set; }
    }
}