using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    public record ResultadoEvaluacion(bool Exito, string Mensaje, string? CodigoIncidente = null, string? NuevoEstado = null);

    public record ResumenEvaluacion(int Pendientes, int Constatados, int Desestimados);

    // Triaje EDAN: el evaluador de campo verifica cada alerta ciudadana (Reportado -> Constatado | Desestimado).
    public interface IEvaluacionService
    {
        // Incidentes en estado Reportado, ordenados por prioridad (severidad, familias y antigüedad).
        Task<IEnumerable<IncidenteEmergencia>> ObtenerPendientesAsync();
        Task<IncidenteEmergencia?> ObtenerPendientePorIdAsync(int incidenteId);
        Task<ResumenEvaluacion> ObtenerResumenAsync();

        Task<ResultadoEvaluacion> ConstatarAsync(int incidenteId, string evaluadorId, string? observacion, string? ipCliente);
        Task<ResultadoEvaluacion> DesestimarAsync(int incidenteId, string evaluadorId, string? observacion, string? ipCliente);
    }
}
