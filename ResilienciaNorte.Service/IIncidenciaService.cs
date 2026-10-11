using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    // Datos que el evaluador registra en la ficha EDAN al constatar la incidencia
    public record FichaEdanDatos(
        int FamiliasAfectadas,
        int ViviendasAfectadas,
        int PersonasAfectadas,
        string? NecesidadesUrgentes,
        string? Observaciones,
        DateTime FechaVisita);

    public interface IIncidenciaService
    {

        // Reportes del mismo distrito y tipo, dentro de la ventana de tiempo del reporte de referencia
        Task<IEnumerable<IncidenteEmergencia>> SugerirReportesAsync(int reporteId);

        // distritoAnalista: si tiene valor, todos los reportes deben pertenecer a ese distrito
        Task<Incidencia> CrearAsync(IEnumerable<int> reporteIds, string sector, string tipoEvento, string? observacion, int? distritoAnalista);
        // distritoId null = todas las incidencias de la provincia
        Task<IEnumerable<Incidencia>> ObtenerAsync(string? estado, int? distritoId);
        Task<Incidencia?> ObtenerPorIdAsync(int incidenciaId);
        Task AsignarEvaluadorAsync(int incidenciaId, string evaluadorId);

        // Incidencias del evaluador (o todas si evaluadorId es null, para el administrador)
        Task<IEnumerable<Incidencia>> ObtenerDelEvaluadorAsync(string? evaluadorId);

        // Constata o desestima la incidencia. Constatar exige la ficha EDAN; desestimar exige el motivo.
        Task<Incidencia> ResolverAsync(int incidenciaId, string usuarioId, bool esAdministrador, bool constatada, string? observacion, FichaEdanDatos? ficha);
    }
}
