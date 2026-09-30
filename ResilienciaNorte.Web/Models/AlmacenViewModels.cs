using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ResilienciaNorte.Web.Models
{
    // ────────────────────────────────────────────────────────────────────────────
    // ViewModel para registrar un INGRESO al almacén
    // Cubre: Compra Directa Edil, Donación y Sobrante
    // ────────────────────────────────────────────────────────────────────────────
    public class IngresoAlmacenViewModel
    {
        [Required(ErrorMessage = "Seleccione el recurso.")]
        [Display(Name = "Recurso")]
        public int RecursoId { get; set; }

        [Required(ErrorMessage = "Seleccione el distrito custodio.")]
        [Display(Name = "Distrito Custodio")]
        public int DistritoId { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad.")]
        [Range(1, 1_000_000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Seleccione el concepto de ingreso.")]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;  // CompraDirecta | Donacion | Sobrante

        [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
        [Display(Name = "Documento de Referencia")]
        public string? DocumentoReferencia { get; set; }  // N° Orden Compra, Acta Donación, etc.

        [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
        [Display(Name = "Entidad / Proveedor de Origen")]
        public string? EntidadOrigen { get; set; }

        // Para rellenar los <select> en la vista
        public IEnumerable<SelectListItem> Recursos { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Distritos { get; set; } = Enumerable.Empty<SelectListItem>();

        public static IEnumerable<SelectListItem> ConceptosIngreso =>
        [
            new SelectListItem("Compra Directa Edil",  "CompraDirecta"),
            new SelectListItem("Donación",             "Donacion"),
            new SelectListItem("Sobrante / Excedente", "Sobrante"),
        ];
    }

    // ────────────────────────────────────────────────────────────────────────────
    // ViewModel para filtrar y ver el KARDEX
    // ────────────────────────────────────────────────────────────────────────────
    public class KardexFiltroViewModel
    {
        [Display(Name = "Recurso")]
        public int? RecursoId { get; set; }

        [Display(Name = "Distrito")]
        public int? DistritoId { get; set; }

        [Display(Name = "Desde")]
        [DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [Display(Name = "Hasta")]
        [DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        // Para rellenar los <select> en la vista
        public IEnumerable<SelectListItem> Recursos { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Distritos { get; set; } = Enumerable.Empty<SelectListItem>();
    }

    // ────────────────────────────────────────────────────────────────────────────
    // ViewModel para crear una SOLICITUD DE REABASTECIMIENTO
    // ────────────────────────────────────────────────────────────────────────────
    public class ReabastecimientoViewModel
    {
        [Required(ErrorMessage = "Seleccione el distrito que solicita.")]
        [Display(Name = "Distrito Solicitante")]
        public int DistritoOrigenId { get; set; }

        [Required(ErrorMessage = "Seleccione el recurso.")]
        [Display(Name = "Recurso")]
        public int RecursoId { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad solicitada.")]
        [Range(1, 1_000_000, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        [Display(Name = "Cantidad Solicitada")]
        public int CantidadSolicitada { get; set; }

        [Required(ErrorMessage = "Seleccione el nivel de destino.")]
        [Display(Name = "Nivel de Destino")]
        public string NivelDestino { get; set; } = string.Empty;  // Provincial | Regional

        [Required(ErrorMessage = "Ingrese la justificación.")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        [Display(Name = "Justificación")]
        public string Justificacion { get; set; } = string.Empty;

        // Para rellenar los <select> en la vista
        public IEnumerable<SelectListItem> Recursos { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Distritos { get; set; } = Enumerable.Empty<SelectListItem>();

        public static IEnumerable<SelectListItem> NivelesDestino =>
        [
            new SelectListItem("Nivel Provincial", "Provincial"),
            new SelectListItem("Nivel Regional",   "Regional"),
        ];
    }
}
