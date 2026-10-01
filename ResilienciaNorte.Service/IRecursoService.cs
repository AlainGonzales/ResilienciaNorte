using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    public interface IRecursoService
    {
        // Método consumido por RecursosController.cs (Index)
        Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(int? distritoId);

        // Consultas de inventario y Kardex
        Task<IEnumerable<RecursoAlmacen>> ObtenerTodosAsync(int? distritoId, string? categoria);
        Task<RecursoAlmacen?> ObtenerPorIdAsync(int recursoId);
        Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexPorRecursoAsync(int recursoId);

        // Transacciones de entrada y salida atómica (Kardex inmutable)
        Task RegistrarIngresoAsync(int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen, string usuarioId);
        Task<bool> DeducirStockAtomicoAsync(int recursoId, int cantidad, string concepto, string documento, string usuarioId);

        // Reabastecimiento subsidiario
        Task<SolicitudReabastecimiento> SolicitarReabastecimientoAsync(int recursoId, int distritoOrigenId, int cantidad, string justificacion);
        Task<IEnumerable<SolicitudReabastecimiento>> ObtenerSolicitudesAsync();
    }
}