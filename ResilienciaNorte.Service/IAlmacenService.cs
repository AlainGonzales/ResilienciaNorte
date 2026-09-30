using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service;

public interface IAlmacenService
{
    // ── Inventario ────────────────────────────────────────────────────────────
    Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(int? distritoId = null, string? categoria = null);
    Task<RecursoAlmacen?> ObtenerRecursoPorIdAsync(int recursoId);
    Task<IEnumerable<RecursoAlmacen>> ObtenerRecursosConStockCriticoAsync(int? distritoId = null);

    // ── Ingresos al Almacén (Compra Directa, Donación, Sobrante) ─────────────
    Task<MovimientoAlmacen> RegistrarIngresoAsync(
        int recursoId,
        int distritoId,
        int cantidad,
        string concepto,            // 'CompraDirecta' | 'Donacion' | 'Sobrante'
        string usuarioId,
        string? documentoReferencia = null,
        string? entidadOrigen = null);

    // ── Kardex ────────────────────────────────────────────────────────────────
    Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexAsync(
        int? recursoId = null,
        int? distritoId = null,
        DateTime? desde = null,
        DateTime? hasta = null);

    // ── Reabastecimiento Subsidiario ──────────────────────────────────────────
    Task<SolicitudReabastecimiento> CrearSolicitudReabastecimientoAsync(
        int distritoOrigenId,
        int recursoId,
        int cantidadSolicitada,
        string nivelDestino,        // 'Provincial' | 'Regional'
        string justificacion,
        string usuarioId);

    Task<IEnumerable<SolicitudReabastecimiento>> ObtenerSolicitudesAsync(
        int? distritoId = null,
        string? estado = null);

    Task AprobarSolicitudAsync(int solicitudId, string usuarioAprobadorId);
    Task RechazarSolicitudAsync(int solicitudId, string usuarioAprobadorId, string motivo);

    // ── Alertas ───────────────────────────────────────────────────────────────
    Task<int> ContarAlertasStockCriticoAsync(int? distritoId = null);
    Task<IEnumerable<Distrito>> ObtenerDistritosAsync();
}
