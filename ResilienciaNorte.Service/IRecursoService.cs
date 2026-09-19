using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service;

public interface IRecursoService
{
    Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(int? distritoId = null);
    Task<bool> HayAlertaStockBajoAsync(int distritoId);
}