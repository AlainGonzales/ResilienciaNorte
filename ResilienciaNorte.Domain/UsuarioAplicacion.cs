using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ResilienciaNorte.Domain
{
    public class UsuarioAplicacion : IdentityUser
    {
        [Required]
        [StringLength(8)]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string NombreCompleto { get; set; } = string.Empty;

        // "Distrital", "Provincial", "Regional" (o "Ciudadano")
        [Required]
        [StringLength(30)]
        public string NivelJurisdiccion { get; set; } = "Distrital";

        // Jurisdicción distrital (opcional para ciudadanos, obligatoria para evaluadores/logísticos de Trujillo)
        public int? DistritoId { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relación de navegación
        [ForeignKey("DistritoId")]
        public virtual Distrito? Distrito { get; set; }
    }
}