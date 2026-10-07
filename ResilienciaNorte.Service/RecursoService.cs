using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class RecursoService : IRecursoService
    {
        private readonly ResilienciaDbContext _context;

        public RecursoService(ResilienciaDbContext context)
        {
            _context = context;
        }

        // ── Implementación del método base para RecursosController ──────────
        public async Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(int? distritoId)
        {
            return await ObtenerTodosAsync(distritoId, null);
        }

        public async Task<IEnumerable<RecursoAlmacen>> ObtenerTodosAsync(int? distritoId, string? categoria)
        {
            var query = _context.RecursosAlmacen.Include(r => r.Distrito).AsQueryable();

            if (distritoId.HasValue && distritoId.Value > 0)
                query = query.Where(r => r.DistritoId == distritoId.Value);

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(r => r.Categoria == categoria);

            return await query.OrderBy(r => r.Nombre).ToListAsync();
        }

        public async Task<RecursoAlmacen?> ObtenerPorIdAsync(int recursoId)
        {
            return await _context.RecursosAlmacen
                .Include(r => r.Distrito)
                .FirstOrDefaultAsync(r => r.RecursoId == recursoId);
        }

        public async Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexPorRecursoAsync(int recursoId)
        {
            return await _context.MovimientosAlmacen
                .Include(m => m.Distrito)
                .Where(m => m.RecursoId == recursoId)
                .OrderByDescending(m => m.FechaHora)
                .ToListAsync();
        }

        // ── Transacción Atómica de Entrada (Kardex Inmutable) ─────────────────
        public async Task RegistrarIngresoAsync(int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen, string usuarioId)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var recurso = await _context.RecursosAlmacen.FindAsync(recursoId)
                    ?? throw new KeyNotFoundException("Recurso no encontrado.");

                int stockAnterior = recurso.StockDisponible;
                int stockPosterior = stockAnterior + cantidad;

                recurso.StockDisponible = stockPosterior;

                var movimiento = new MovimientoAlmacen
                {
                    RecursoId = recursoId,
                    DistritoId = recurso.DistritoId,
                    TipoMovimiento = "ENTRADA",
                    ConceptoMovimiento = $"{tipoEntrada}: {concepto}",
                    DocumentoReferencia = documento,
                    EntidadOrigenDestino = entidadOrigen,
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = stockPosterior,
                    UsuarioResponsableId = usuarioId,
                    FechaHora = TimeHelper.Ahora
                };

                _context.MovimientosAlmacen.Add(movimiento);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // ── Transacción Atómica de Salida (Kardex Inmutable) ──────────────────
        public async Task<bool> DeducirStockAtomicoAsync(int recursoId, int cantidad, string concepto, string documento, string usuarioId)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var recurso = await _context.RecursosAlmacen.FindAsync(recursoId);
                if (recurso == null || recurso.StockDisponible < cantidad)
                {
                    await transaccion.RollbackAsync();
                    return false;
                }

                int stockAnterior = recurso.StockDisponible;
                int stockPosterior = stockAnterior - cantidad;

                recurso.StockDisponible = stockPosterior;

                var movimiento = new MovimientoAlmacen
                {
                    RecursoId = recursoId,
                    DistritoId = recurso.DistritoId,
                    TipoMovimiento = "SALIDA",
                    ConceptoMovimiento = concepto,
                    DocumentoReferencia = documento,
                    EntidadOrigenDestino = "Atención de Emergencia EDAN",
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = stockPosterior,
                    UsuarioResponsableId = usuarioId,
                    FechaHora = TimeHelper.Ahora
                };

                _context.MovimientosAlmacen.Add(movimiento);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return true;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // ── Lógica de Reabastecimiento Subsidiario ───────────────────────────
        public async Task<SolicitudReabastecimiento> SolicitarReabastecimientoAsync(int recursoId, int distritoOrigenId, int cantidad, string justificacion)
        {
            var recurso = await _context.RecursosAlmacen.FindAsync(recursoId)
                ?? throw new KeyNotFoundException("Recurso no encontrado.");

            // Regla Subsidiaria: Almacén distrital eleva a Provincia; Trujillo Centro eleva a Región
            string nivelDestino = (distritoOrigenId == 10) ? "Región La Libertad" : "Provincia Trujillo";

            var solicitud = new SolicitudReabastecimiento
            {
                CodigoSolicitud = $"SOL-{TimeHelper.Ahora.Year}-{new Random().Next(1000, 9999)}",
                DistritoOrigenId = distritoOrigenId,
                NivelDestino = nivelDestino,
                RecursoId = recursoId,
                CantidadSolicitada = cantidad,
                Justificacion = justificacion,
                Estado = "Pendiente",
                FechaSolicitud = TimeHelper.Ahora
            };

            _context.SolicitudesReabastecimiento.Add(solicitud);
            await _context.SaveChangesAsync();

            return solicitud;
        }

        public async Task<IEnumerable<SolicitudReabastecimiento>> ObtenerSolicitudesAsync()
        {
            return await _context.SolicitudesReabastecimiento
                .Include(s => s.DistritoOrigen)
                .Include(s => s.Recurso)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }
    }
}