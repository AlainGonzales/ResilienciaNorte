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
        var query = _context.Incidentes
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

        return await query.OrderByDescending(i => i.FechaReporte).ToListAsync();
    }

    public async Task<IncidenteEmergencia?> ObtenerPorIdAsync(int id)
    {
        return await _context.Incidentes
            .Include(i => i.Distrito)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IncidenteEmergencia> RegistrarIncidenteAsync(IncidenteEmergencia incidente)
    {
        // Regla de Negocio: Cálculo automático de Severidad según impacto inicial
        if (incidente.FamiliasAfectadas >= 15 || incidente.TipoDesastre.Contains("Desborde"))
        {
            incidente.NivelSeveridad = "Crítico";
        }
        else if (incidente.FamiliasAfectadas >= 6)
        {
            incidente.NivelSeveridad = "Alto";
        }
        else
        {
            incidente.NivelSeveridad = "Moderado";
        }

        incidente.FechaReporte = DateTime.Now;
        incidente.Estado = "Pendiente";

        _context.Incidentes.Add(incidente);
        await _context.SaveChangesAsync();
        return incidente;
    }

    public async Task<bool> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        var incidente = await _context.Incidentes.FindAsync(id);
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