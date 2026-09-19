using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service;

public interface IIncidenteService
{
    Task<IEnumerable<IncidenteEmergencia>> ObtenerTodosAsync(int? distritoId = null, string? estado = null);
    Task<IncidenteEmergencia?> ObtenerPorIdAsync(int id);
    Task<IncidenteEmergencia> RegistrarIncidenteAsync(IncidenteEmergencia incidente);
    Task<bool> CambiarEstadoAsync(int id, string nuevoEstado);
    Task<IEnumerable<Distrito>> ObtenerDistritosAsync();
}