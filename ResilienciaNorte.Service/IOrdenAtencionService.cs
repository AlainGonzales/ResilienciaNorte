using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    // Línea solicitada para el detalle de la orden (bien BAH + cantidad).
    public record LineaOrden(int RecursoId, int Cantidad, string? Observaciones);

    // Maestro-Detalle: OrdenAtencion (cabecera) + DetalleOrden (bienes BAH asignados).
    // Las violaciones de reglas de negocio se reportan con InvalidOperationException (mensaje apto para el usuario).
    public interface IOrdenAtencionService
    {
        Task<IEnumerable<OrdenAtencion>> ObtenerTodasAsync(string? estado);
        Task<OrdenAtencion?> ObtenerPorIdAsync(int ordenId);
        Task<OrdenAtencion?> ObtenerPorIncidenteAsync(int incidenteId);

        // Incidentes constatados que aún no tienen recursos asignados.
        Task<IEnumerable<IncidenteEmergencia>> ObtenerIncidentesPorAsignarAsync();
        Task<IncidenteEmergencia?> ObtenerIncidenteAsignableAsync(int incidenteId);
        Task<IEnumerable<RecursoAlmacen>> ObtenerRecursosDisponiblesAsync();

        Task<OrdenAtencion> CrearOrdenAsync(int incidenteId, string analistaId, string? observaciones, IEnumerable<LineaOrden> lineas);

        // Aprobada -> EnDespacho: descuenta el stock y registra la SALIDA en el Kardex.
        Task<OrdenAtencion> DespacharAsync(int ordenId, string usuarioId);

        // EnDespacho -> Entregado: cierra la orden y el incidente (Atendido).
        Task<OrdenAtencion> EntregarAsync(int ordenId, string usuarioId);

        // Aprobada -> Anulado: libera el incidente para una nueva asignación.
        Task<OrdenAtencion> AnularAsync(int ordenId, string usuarioId, string? motivo);
    }
}
