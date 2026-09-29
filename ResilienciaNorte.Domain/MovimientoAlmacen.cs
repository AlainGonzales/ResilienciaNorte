using System;

namespace ResilienciaNorte.Domain
{
    public class MovimientoAlmacen
    {
        public int MovimientoId { get; set; }

        public int RecursoId { get; set; }
        public RecursoAlmacen? Recurso { get; set; }

        public int DistritoId { get; set; }
        public Distrito? Distrito { get; set; }

        // 'ENTRADA' o 'SALIDA'
        public string TipoMovimiento { get; set; } = string.Empty;

        // 'CompraDirecta', 'Donacion', 'DespachoAuxilio', 'TransferenciaSupradistrital'
        public string ConceptoMovimiento { get; set; } = string.Empty;

        // N° Orden Compra, Acta de Donación, N° de Orden BAH
        public string? DocumentoReferencia { get; set; }
        public string? EntidadOrigenDestino { get; set; } // Nombre del Proveedor, ONG o Municipio

        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockPosterior { get; set; }

        public string UsuarioResponsableId { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    }
}