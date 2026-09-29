using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service;

public class IncidenteService : IIncidenteService
{
    private readonly ResilienciaDbContext _context;

    public IncidenteService(ResilienciaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IncidenteEmergencia>> ObtenerTodosAsync(int? distritoId = null, string? estado = null)
    {
        var query = _context.IncidentesEmergencia
            .Include(i => i.Distrito)
            .AsNoTracking()
            .AsQueryable();

        if (distritoId.HasValue && distritoId.Value > 0)
        {
            query = query.Where(i => i.DistritoId == distritoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(i => i.Estado == estado);
        }

        return await query.OrderByDescending(i => i.FechaRegistro).ToListAsync();
    }

    public async Task<IncidenteEmergencia?> ObtenerPorIdAsync(int id)
    {
        return await _context.IncidentesEmergencia
            .Include(i => i.Distrito)
            .FirstOrDefaultAsync(i => i.IncidenteId == id);
    }

    public async Task<IncidenteEmergencia> RegistrarIncidenteAsync(IncidenteEmergencia incidente)
    {
        // Generar código correlativo si no viene asignado
        if (string.IsNullOrWhiteSpace(incidente.CodigoIncidente))
        {
            incidente.CodigoIncidente = $"ALT-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        }

        // Regla de Negocio: Cálculo automático de Severidad según impacto inicial
        if (incidente.FamiliasAfectadas >= 15 || (!string.IsNullOrEmpty(incidente.TipoEvento) && incidente.TipoEvento.Contains("Desborde")))
        {
            incidente.Severidad = "Crítico";
        }
        else if (incidente.FamiliasAfectadas >= 6)
        {
            incidente.Severidad = "Grave";
        }
        else
        {
            incidente.Severidad = "Moderado";
        }

        incidente.FechaRegistro = DateTime.UtcNow;
        incidente.Estado = "Reportado";

        _context.IncidentesEmergencia.Add(incidente);
        await _context.SaveChangesAsync();
        return incidente;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        var incidente = await _context.IncidentesEmergencia.FindAsync(id);
        if (incidente == null) return false;

        incidente.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Distrito>> ObtenerDistritosAsync()
    {
        return await _context.Distritos
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .AsNoTracking()
            .ToListAsync();
    }
}