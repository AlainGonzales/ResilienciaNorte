using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Web.Models
{
    // Formulario Maestro-Detalle: cabecera (incidente + observaciones) y líneas de bienes BAH.
    public class CrearOrdenViewModel
    {
        [Required]
        public int IncidenteId { get; set; }

        [StringLength(300, ErrorMessage = "Las observaciones no pueden superar los 300 caracteres.")]
        public string? Observaciones { get; set; }

        public List<LineaOrdenViewModel> Detalles { get; set; } = new();

        // Datos de apoyo para pintar la vista; no se enlazan desde el formulario.
        [BindNever, ValidateNever]
        public IncidenteEmergencia? Incidente { get; set; }

        [BindNever, ValidateNever]
        public IEnumerable<RecursoAlmacen> RecursosDisponibles { get; set; } = Enumerable.Empty<RecursoAlmacen>();
    }

    public class LineaOrdenViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un bien BAH.")]
        public int RecursoId { get; set; }

        [Range(1, 1000000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        public int Cantidad { get; set; }

        [StringLength(200)]
        public string? Observaciones { get; set; }
    }
}
