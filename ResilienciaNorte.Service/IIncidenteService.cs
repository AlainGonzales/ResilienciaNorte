using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service
{
    public interface IIncidenteService
    {
        Task<IEnumerable<IncidenteEmergencia>> ObtenerTodosAsync(int? distritoId, string? estado);
        Task<IncidenteEmergencia?> ObtenerPorIdAsync(int incidenteId);
        Task<IncidenteEmergencia?> ObtenerPorCodigoAsync(string codigoIncidente);
        Task<IEnumerable<Distrito>> ObtenerDistritosAsync();
        Task<IncidenteEmergencia> RegistrarIncidenteAsync(IncidenteEmergencia incidente);
        Task<bool> CambiarEstadoAsync(int incidenteId, string nuevoEstado);

        // Métodos de Daniel (OTP y Portal Ciudadano)
        Task<string> GenerarOtpCiudadanoAsync(string codigoIncidente, string dni, string telefono);
        Task<bool> ValidarOtpCiudadanoAsync(string codigoIncidente, string dni, string pinIngresado);
    }
}