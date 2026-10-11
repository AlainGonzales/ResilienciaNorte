using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    public interface IRecursoService
    {
        // Almacenes en tres niveles
        Task<IEnumerable<Almacen>> ObtenerAlmacenesAsync();
        Task<Almacen?> ObtenerAlmacenPorIdAsync(int almacenId);
        Task<IEnumerable<string>> ObtenerCategoriasAsync();

        // Existencias y Kardex por almacén
        Task<IEnumerable<ExistenciaAlmacen>> ObtenerExistenciasAsync(int almacenId, string? categoria);
        Task<ExistenciaAlmacen?> ObtenerExistenciaAsync(int almacenId, int recursoId);
        Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexAsync(int almacenId, int? recursoId);

        // Ingresos (compra, donación o sobrante) y salidas atómicas (Kardex inmutable)
        Task<MovimientoAlmacen> RegistrarIngresoAsync(int almacenId, int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen, string usuarioId);
        Task<bool> DeducirStockAtomicoAsync(int almacenId, int recursoId, int cantidad, string concepto, string documento, string usuarioId);

        // Solicitudes: abastecimiento al almacén padre
        Task<Solicitud> SolicitarAbastecimientoAsync(int almacenSolicitanteId, int recursoId, int cantidad, string justificacion);
        Task<IEnumerable<Solicitud>> ObtenerSolicitudesAsync();

        // Solicitud de bienes del analista para una incidencia constatada (sale del almacén de su distrito)
        Task<IEnumerable<Solicitud>> SolicitarParaIncidenciaAsync(Incidencia incidencia, IEnumerable<(int RecursoId, int Cantidad)> lineas, string justificacion);
        // El coordinador aprueba solo si el almacén proveedor tiene stock suficiente
        Task AprobarSolicitudAsync(int solicitudId, string usuarioId);
        Task RechazarSolicitudAsync(int solicitudId, string usuarioId, string motivo);
        // El logístico despacha lo aprobado: descuenta stock y registra la salida en el Kardex
        // Devuelve true si con este despacho la incidencia quedó Atendida
        Task<bool> DespacharSolicitudAsync(int solicitudId, string usuarioId);

        // Reemplaza el pedido de la incidencia mientras el coordinador no haya decidido sobre ninguna línea
        Task<IEnumerable<Solicitud>> EditarPedidoIncidenciaAsync(Incidencia incidencia, IEnumerable<(int RecursoId, int Cantidad)> lineas, string justificacion);
    }
}
