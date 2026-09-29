namespace ResilienciaNorte.Domain
{
    public class DetalleOrden
    {
        public int DetalleId { get; set; }

        // Clave foránea al Maestro
        public int OrdenId { get; set; }
        public OrdenAtencion? OrdenAtencion { get; set; }

        // Clave foránea al Catálogo de Almacén
        public int RecursoId { get; set; }
        public RecursoAlmacen? Recurso { get; set; }

        public int CantidadSolicitada { get; set; }
        public int CantidadEntregada { get; set; } = 0;
        public string? Observaciones { get; set; }
    }
}