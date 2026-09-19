using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service;

public class RecursoService : IRecursoService
{
    private readonly ResilienciaDbContext _context;

    public RecursoService(ResilienciaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(int? distritoId = null)
    {
        var query = _context.Recursos
            .Include(r => r.Distrito)
            .AsNoTracking()
            .AsQueryable();

        if (distritoId.HasValue && distritoId.Value > 0)
        {
            query = query.Where(r => r.DistritoId == distritoId.Value);
        }

        return await query.OrderBy(r => r.Nombre).ToListAsync();
    }

    public async Task<bool> HayAlertaStockBajoAsync(int distritoId)
    {
        return await _context.Recursos
            .AnyAsync(r => r.DistritoId == distritoId && r.StockDisponible <= r.StockMinimoSeguridad);
    }
}