using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class OrdenAtencionService : IOrdenAtencionService
    {
        private const int LongitudMaximaDetalle = 500; // AuditoriaSistema.Detalle

        private readonly ResilienciaDbContext _context;

        public OrdenAtencionService(ResilienciaDbContext context)
        {
            _context = context;
        }

        // ── Consultas ────────────────────────────────────────────────────────
        public async Task<IEnumerable<OrdenAtencion>> ObtenerTodasAsync(string? estado)
        {
            var query = _context.OrdenesAtencion
                .AsNoTracking()
                .Include(o => o.Incidente).ThenInclude(i => i!.Distrito)
                .Include(o => o.Detalles)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(o => o.Estado == estado);

            return await query.OrderByDescending(o => o.FechaEmision).ToListAsync();
        }

        public async Task<OrdenAtencion?> ObtenerPorIdAsync(int ordenId)
        {
            return await _context.OrdenesAtencion
                .AsNoTracking()
                .Include(o => o.Incidente).ThenInclude(i => i!.Distrito)
                .Include(o => o.Detalles).ThenInclude(d => d.Recurso).ThenInclude(r => r!.Distrito)
                .FirstOrDefaultAsync(o => o.OrdenId == ordenId);
        }

        public async Task<OrdenAtencion?> ObtenerPorIncidenteAsync(int incidenteId)
        {
            return await _context.OrdenesAtencion
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.IncidenteId == incidenteId);
        }

        public async Task<IEnumerable<IncidenteEmergencia>> ObtenerIncidentesPorAsignarAsync()
        {
            var constatados = await _context.IncidentesEmergencia
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Where(i => i.Estado == EstadoIncidente.Constatado)
                .ToListAsync();

            return constatados
                .OrderByDescending(i => NivelSeveridadExtensions.DesdeEtiqueta(i.Severidad))
                .ThenByDescending(i => i.FamiliasAfectadas)
                .ThenBy(i => i.FechaRegistro)
                .ToList();
        }

        public async Task<IncidenteEmergencia?> ObtenerIncidenteAsignableAsync(int incidenteId)
        {
            return await _context.IncidentesEmergencia
                .AsNoTracking()
                .Include(i => i.Distrito)
                .FirstOrDefaultAsync(i => i.IncidenteId == incidenteId && i.Estado == EstadoIncidente.Constatado);
        }

        public async Task<IEnumerable<RecursoAlmacen>> ObtenerRecursosDisponiblesAsync()
        {
            return await _context.RecursosAlmacen
                .AsNoTracking()
                .Include(r => r.Distrito)
                .Where(r => r.StockDisponible > 0)
                .OrderBy(r => r.Categoria).ThenBy(r => r.Nombre)
                .ToListAsync();
        }

        // ── Alta de la orden (cabecera + detalle en una sola transacción) ─────
        public async Task<OrdenAtencion> CrearOrdenAsync(int incidenteId, string analistaId, string? observaciones, IEnumerable<LineaOrden> lineas)
        {
            // Una línea por bien: si el usuario repite el recurso, se consolida la cantidad.
            var lineasConsolidadas = lineas
                .Where(l => l.RecursoId > 0)
                .GroupBy(l => l.RecursoId)
                .Select(g => new LineaOrden(
                    g.Key,
                    g.Sum(l => l.Cantidad),
                    string.Join(" | ", g.Select(l => l.Observaciones?.Trim()).Where(o => !string.IsNullOrEmpty(o)))))
                .ToList();

            if (lineasConsolidadas.Count == 0)
                throw new InvalidOperationException("La orden debe incluir al menos un bien BAH.");

            if (lineasConsolidadas.Any(l => l.Cantidad <= 0))
                throw new InvalidOperationException("Las cantidades solicitadas deben ser mayores a 0.");

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                // Se calcula antes de reemplazar una orden anulada, para que el código nunca repita el de la orden eliminada.
                string codigoOrden = await GenerarCodigoAsync();

                var incidente = await _context.IncidentesEmergencia.FindAsync(incidenteId)
                    ?? throw new InvalidOperationException("El incidente no existe.");

                if (incidente.Estado != EstadoIncidente.Constatado)
                    throw new InvalidOperationException(
                        $"Solo se asignan recursos a incidentes constatados (estado actual de {incidente.CodigoIncidente}: {incidente.Estado}).");

                // La relación incidente-orden es 1 a 1: una orden anulada se reemplaza por la nueva.
                var ordenPrevia = await _context.OrdenesAtencion.FirstOrDefaultAsync(o => o.IncidenteId == incidenteId);
                if (ordenPrevia != null)
                {
                    if (ordenPrevia.Estado != EstadoOrden.Anulado)
                        throw new InvalidOperationException($"{incidente.CodigoIncidente} ya tiene la orden {ordenPrevia.CodigoOrden}.");

                    _context.OrdenesAtencion.Remove(ordenPrevia); // el detalle se elimina en cascada
                    await _context.SaveChangesAsync(); // libera el índice único de IncidenteId (sigue dentro de la transacción)
                }

                var ids = lineasConsolidadas.Select(l => l.RecursoId).ToList();
                var recursos = await _context.RecursosAlmacen
                    .Where(r => ids.Contains(r.RecursoId))
                    .ToDictionaryAsync(r => r.RecursoId);

                var orden = new OrdenAtencion
                {
                    CodigoOrden = codigoOrden,
                    IncidenteId = incidenteId,
                    UsuarioAnalistaId = analistaId,
                    Estado = EstadoOrden.Aprobada,
                    FechaEmision = DateTime.UtcNow,
                    Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()
                };

                foreach (var linea in lineasConsolidadas)
                {
                    if (!recursos.TryGetValue(linea.RecursoId, out var recurso))
                        throw new InvalidOperationException("Uno de los bienes seleccionados no existe en el catálogo BAH.");

                    if (linea.Cantidad > recurso.StockDisponible)
                        throw new InvalidOperationException(
                            $"Stock insuficiente de {recurso.Nombre}: solicitado {linea.Cantidad}, disponible {recurso.StockDisponible}.");

                    orden.Detalles.Add(new DetalleOrden
                    {
                        RecursoId = linea.RecursoId,
                        CantidadSolicitada = linea.Cantidad,
                        CantidadEntregada = 0,
                        Observaciones = string.IsNullOrWhiteSpace(linea.Observaciones) ? null : linea.Observaciones
                    });
                }

                incidente.Estado = EstadoIncidente.ConAsignacion;
                _context.OrdenesAtencion.Add(orden);

                Auditar(analistaId, "Analista COEP", "INSERT",
                    $"Orden {orden.CodigoOrden} para {incidente.CodigoIncidente} con {orden.Detalles.Count} bien(es) BAH.");

                // Un único SaveChanges: la orden y su detalle se guardan juntos (o ninguno).
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                return orden;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // ── Despacho: descuenta stock y registra la salida en el Kardex ───────
        public async Task<OrdenAtencion> DespacharAsync(int ordenId, string usuarioId)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var orden = await CargarOrdenAsync(ordenId);

                if (orden.Estado != EstadoOrden.Aprobada)
                    throw new InvalidOperationException($"Solo se despachan órdenes aprobadas (estado actual: {orden.Estado}).");

                foreach (var detalle in orden.Detalles)
                {
                    var recurso = detalle.Recurso
                        ?? throw new InvalidOperationException("Bien BAH no encontrado en el catálogo.");

                    if (detalle.CantidadSolicitada > recurso.StockDisponible)
                        throw new InvalidOperationException(
                            $"Stock insuficiente de {recurso.Nombre}: requerido {detalle.CantidadSolicitada}, disponible {recurso.StockDisponible}.");

                    int stockAnterior = recurso.StockDisponible;
                    recurso.DescontarStock(detalle.CantidadSolicitada);

                    _context.MovimientosAlmacen.Add(new MovimientoAlmacen
                    {
                        RecursoId = recurso.RecursoId,
                        DistritoId = recurso.DistritoId,
                        TipoMovimiento = "SALIDA",
                        ConceptoMovimiento = "DespachoAuxilio",
                        DocumentoReferencia = orden.CodigoOrden,
                        EntidadOrigenDestino = $"Atención de Emergencia EDAN - {orden.Incidente?.CodigoIncidente}",
                        Cantidad = detalle.CantidadSolicitada,
                        StockAnterior = stockAnterior,
                        StockPosterior = recurso.StockDisponible,
                        UsuarioResponsableId = usuarioId,
                        FechaHora = DateTime.UtcNow
                    });
                }

                orden.Estado = EstadoOrden.EnDespacho;
                if (orden.Incidente != null) orden.Incidente.Estado = EstadoIncidente.EnDespacho;

                Auditar(usuarioId, "Logística BAH", "UPDATE", $"Despacho de la orden {orden.CodigoOrden}: stock descontado y Kardex registrado.");

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return orden;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task<OrdenAtencion> EntregarAsync(int ordenId, string usuarioId)
        {
            var orden = await CargarOrdenAsync(ordenId);

            if (orden.Estado != EstadoOrden.EnDespacho)
                throw new InvalidOperationException($"Solo se entregan órdenes en despacho (estado actual: {orden.Estado}).");

            foreach (var detalle in orden.Detalles)
                detalle.CantidadEntregada = detalle.CantidadSolicitada;

            orden.Estado = EstadoOrden.Entregado;
            orden.FechaEntrega = DateTime.UtcNow;
            orden.UsuarioCoordinadorId = usuarioId;
            if (orden.Incidente != null) orden.Incidente.Estado = EstadoIncidente.Atendido;

            Auditar(usuarioId, "Coordinador COEP", "UPDATE", $"Entrega confirmada de la orden {orden.CodigoOrden}; incidente atendido.");

            await _context.SaveChangesAsync();
            return orden;
        }

        public async Task<OrdenAtencion> AnularAsync(int ordenId, string usuarioId, string? motivo)
        {
            var orden = await CargarOrdenAsync(ordenId);

            // Con la orden ya despachada el stock salió del almacén: anular dejaría el Kardex inconsistente.
            if (orden.Estado != EstadoOrden.Aprobada)
                throw new InvalidOperationException($"Solo se anulan órdenes aprobadas (estado actual: {orden.Estado}).");

            orden.Estado = EstadoOrden.Anulado;
            orden.UsuarioCoordinadorId = usuarioId;
            if (!string.IsNullOrWhiteSpace(motivo))
                orden.Observaciones = string.IsNullOrWhiteSpace(orden.Observaciones)
                    ? $"Anulada: {motivo.Trim()}"
                    : $"{orden.Observaciones} | Anulada: {motivo.Trim()}";

            // El incidente vuelve a quedar disponible para una nueva asignación.
            if (orden.Incidente != null) orden.Incidente.Estado = EstadoIncidente.Constatado;

            Auditar(usuarioId, "Coordinador COEP", "UPDATE", $"Anulación de la orden {orden.CodigoOrden}.{(string.IsNullOrWhiteSpace(motivo) ? "" : " Motivo: " + motivo.Trim())}");

            await _context.SaveChangesAsync();
            return orden;
        }

        // ── Internos ─────────────────────────────────────────────────────────
        private async Task<OrdenAtencion> CargarOrdenAsync(int ordenId)
        {
            return await _context.OrdenesAtencion
                .Include(o => o.Incidente)
                .Include(o => o.Detalles).ThenInclude(d => d.Recurso)
                .FirstOrDefaultAsync(o => o.OrdenId == ordenId)
                ?? throw new InvalidOperationException("La orden de atención no existe.");
        }

        private async Task<string> GenerarCodigoAsync()
        {
            int correlativo = (await _context.OrdenesAtencion.MaxAsync(o => (int?)o.OrdenId) ?? 0) + 1;
            return $"OAT-{DateTime.UtcNow.Year}-{correlativo:D4}";
        }

        private void Auditar(string usuarioId, string rol, string operacion, string detalle)
        {
            _context.AuditoriaSistema.Add(new AuditoriaSistema
            {
                UsuarioId = usuarioId,
                RolUsuario = rol,
                Operacion = operacion,
                Modulo = "Ordenes",
                Detalle = detalle.Length > LongitudMaximaDetalle ? detalle[..LongitudMaximaDetalle] : detalle,
                FechaHoraUtc = DateTime.UtcNow
            });
        }
    }
}
