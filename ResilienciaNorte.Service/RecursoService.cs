using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class RecursoService : IRecursoService
    {
        private const int StockMinimoPorDefecto = 10;

        private readonly ResilienciaDbContext _context;

        public RecursoService(ResilienciaDbContext context)
        {
            _context = context;
        }

        // ── Almacenes ────────────────────────────────────────────────────────
        public async Task<IEnumerable<Almacen>> ObtenerAlmacenesAsync()
        {
            var almacenes = await _context.Almacenes
                .AsNoTracking()
                .Include(a => a.Distrito)
                .Include(a => a.Provincia)
                .ToListAsync();

            return almacenes
                .OrderBy(a => a.Nivel == "Regional" ? 0 : a.Nivel == "Provincial" ? 1 : 2)
                .ThenBy(a => a.Provincia?.Nombre)
                .ThenBy(a => a.Nombre)
                .ToList();
        }

        public async Task<Almacen?> ObtenerAlmacenPorIdAsync(int almacenId)
        {
            return await _context.Almacenes
                .AsNoTracking()
                .Include(a => a.Distrito)
                .Include(a => a.Provincia)
                .Include(a => a.AlmacenPadre)
                .FirstOrDefaultAsync(a => a.AlmacenId == almacenId);
        }

        public async Task<IEnumerable<string>> ObtenerCategoriasAsync()
        {
            return await _context.RecursosAlmacen
                .AsNoTracking()
                .Select(r => r.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        // ── Existencias y Kardex ─────────────────────────────────────────────
        public async Task<IEnumerable<ExistenciaAlmacen>> ObtenerExistenciasAsync(int almacenId, string? categoria)
        {
            var query = _context.ExistenciasAlmacen
                .AsNoTracking()
                .Include(e => e.Recurso)
                .Include(e => e.Almacen).ThenInclude(a => a!.AlmacenPadre)
                .Where(e => e.AlmacenId == almacenId);

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(e => e.Recurso!.Categoria == categoria);

            return await query.OrderBy(e => e.Recurso!.Nombre).ToListAsync();
        }

        public async Task<ExistenciaAlmacen?> ObtenerExistenciaAsync(int almacenId, int recursoId)
        {
            return await _context.ExistenciasAlmacen
                .AsNoTracking()
                .Include(e => e.Recurso)
                .Include(e => e.Almacen).ThenInclude(a => a!.AlmacenPadre)
                .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.RecursoId == recursoId);
        }

        public async Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexAsync(int almacenId, int? recursoId)
        {
            var query = _context.MovimientosAlmacen
                .AsNoTracking()
                .Include(m => m.Recurso)
                .Where(m => m.AlmacenId == almacenId);

            if (recursoId.HasValue)
                query = query.Where(m => m.RecursoId == recursoId.Value);

            return await query.OrderByDescending(m => m.FechaHora).ToListAsync();
        }

        // ── Ingreso (compra directa, donación o sobrante) ────────────────────
        public async Task<MovimientoAlmacen> RegistrarIngresoAsync(int almacenId, int recursoId, int cantidad, string tipoEntrada, string concepto, string documento, string entidadOrigen, string usuarioId)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var existencia = await _context.ExistenciasAlmacen
                    .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.RecursoId == recursoId);

                if (existencia == null)
                {
                    existencia = new ExistenciaAlmacen
                    {
                        AlmacenId = almacenId,
                        RecursoId = recursoId,
                        StockDisponible = 0,
                        StockMinimo = StockMinimoPorDefecto
                    };
                    _context.ExistenciasAlmacen.Add(existencia);
                }

                int stockAnterior = existencia.StockDisponible;
                existencia.IncrementarStock(cantidad);

                var movimiento = new MovimientoAlmacen
                {
                    AlmacenId = almacenId,
                    RecursoId = recursoId,
                    TipoMovimiento = "ENTRADA",
                    ConceptoMovimiento = $"{tipoEntrada}: {concepto}",
                    DocumentoReferencia = documento,
                    EntidadOrigenDestino = entidadOrigen,
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = existencia.StockDisponible,
                    UsuarioResponsableId = usuarioId,
                    FechaHora = DateTime.UtcNow
                };
                _context.MovimientosAlmacen.Add(movimiento);

                // Compras y donaciones quedan como solicitud recepcionada, para trazabilidad
                string? tipoSolicitud = tipoEntrada switch
                {
                    "Compra Directa Edil" => "Compra",
                    "Donación" => "Donacion",
                    _ => null
                };

                if (tipoSolicitud != null)
                {
                    _context.Solicitudes.Add(new Solicitud
                    {
                        CodigoSolicitud = $"SOL-{DateTime.UtcNow.Year}-{Random.Shared.Next(1000, 9999)}",
                        Tipo = tipoSolicitud,
                        AlmacenSolicitanteId = almacenId,
                        RecursoId = recursoId,
                        CantidadSolicitada = cantidad,
                        Justificacion = concepto,
                        Estado = "Recepcionado",
                        FechaSolicitud = DateTime.UtcNow,
                        FechaAtencion = DateTime.UtcNow,
                        UsuarioAprobadorId = usuarioId
                    });
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return movimiento;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        // ── Salida atómica (Kardex inmutable) ────────────────────────────────
        public async Task<bool> DeducirStockAtomicoAsync(int almacenId, int recursoId, int cantidad, string concepto, string documento, string usuarioId)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var existencia = await _context.ExistenciasAlmacen
                    .FirstOrDefaultAsync(e => e.AlmacenId == almacenId && e.RecursoId == recursoId);

                if (existencia == null || existencia.StockDisponible < cantidad)
                {
                    await transaccion.RollbackAsync();
                    return false;
                }

                int stockAnterior = existencia.StockDisponible;
                existencia.DescontarStock(cantidad);

                _context.MovimientosAlmacen.Add(new MovimientoAlmacen
                {
                    AlmacenId = almacenId,
                    RecursoId = recursoId,
                    TipoMovimiento = "SALIDA",
                    ConceptoMovimiento = concepto,
                    DocumentoReferencia = documento,
                    EntidadOrigenDestino = "Atención de Emergencia EDAN",
                    Cantidad = cantidad,
                    StockAnterior = stockAnterior,
                    StockPosterior = existencia.StockDisponible,
                    UsuarioResponsableId = usuarioId,
                    FechaHora = DateTime.UtcNow
                });

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

        // ── Abastecimiento: el proveedor es el almacén padre ─────────────────
        public async Task<Solicitud> SolicitarAbastecimientoAsync(int almacenSolicitanteId, int recursoId, int cantidad, string justificacion)
        {
            var solicitante = await _context.Almacenes.FindAsync(almacenSolicitanteId)
                ?? throw new KeyNotFoundException("Almacén no encontrado.");

            if (solicitante.AlmacenPadreId == null)
                throw new InvalidOperationException("El almacén regional no tiene un nivel superior al que pedir abastecimiento.");

            var solicitud = new Solicitud
            {
                CodigoSolicitud = $"SOL-{DateTime.UtcNow.Year}-{Random.Shared.Next(1000, 9999)}",
                Tipo = "Abastecimiento",
                AlmacenSolicitanteId = almacenSolicitanteId,
                AlmacenProveedorId = solicitante.AlmacenPadreId.Value,
                RecursoId = recursoId,
                CantidadSolicitada = cantidad,
                Justificacion = justificacion,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();
            return solicitud;
        }

        public async Task<IEnumerable<Solicitud>> SolicitarParaIncidenciaAsync(Incidencia incidencia, IEnumerable<(int RecursoId, int Cantidad)> lineas, string justificacion)
        {
            if (incidencia.Estado != EstadoIncidencia.Constatada)
                throw new InvalidOperationException("Solo se piden bienes para incidencias constatadas.");

            var lista = lineas.ToList();
            if (lista.Count == 0)
                throw new InvalidOperationException("Agregue al menos un bien al pedido.");
            if (lista.Any(l => l.Cantidad <= 0))
                throw new InvalidOperationException("La cantidad de cada bien debe ser mayor a cero.");
            if (lista.GroupBy(l => l.RecursoId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Un mismo bien no puede repetirse en el pedido.");

            var almacen = await _context.Almacenes
                .FirstOrDefaultAsync(a => a.Nivel == "Distrital" && a.DistritoId == incidencia.DistritoId)
                ?? throw new InvalidOperationException("El distrito no tiene almacén distrital.");

            // Cada bien es una solicitud propia: el coordinador la aprueba o rechaza según su stock
            var solicitudes = lista.Select(l => new Solicitud
            {
                CodigoSolicitud = $"SOL-{DateTime.UtcNow.Year}-{Random.Shared.Next(1000, 9999)}",
                Tipo = "Incidencia",
                AlmacenSolicitanteId = almacen.AlmacenId,
                AlmacenProveedorId = almacen.AlmacenId,
                RecursoId = l.RecursoId,
                CantidadSolicitada = l.Cantidad,
                Justificacion = justificacion.Trim(),
                IncidenciaId = incidencia.IncidenciaId,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            }).ToList();

            _context.Solicitudes.AddRange(solicitudes);
            await _context.SaveChangesAsync();
            return solicitudes;
        }

        public async Task AprobarSolicitudAsync(int solicitudId, string usuarioId)
        {
            var solicitud = await BuscarSolicitudAsync(solicitudId);
            if (solicitud.Estado != "Pendiente")
                throw new InvalidOperationException($"Solo se aprueban solicitudes pendientes (estado actual: {solicitud.Estado}).");

            var existencia = solicitud.AlmacenProveedorId == null ? null
                : await _context.ExistenciasAlmacen.FirstOrDefaultAsync(e => e.AlmacenId == solicitud.AlmacenProveedorId && e.RecursoId == solicitud.RecursoId);
            if (existencia == null || existencia.StockDisponible < solicitud.CantidadSolicitada)
                throw new InvalidOperationException("El stock del almacén no alcanza para aprobar esta solicitud.");

            solicitud.Estado = "Aprobada";
            solicitud.UsuarioAprobadorId = usuarioId;
            await _context.SaveChangesAsync();
        }

        public async Task RechazarSolicitudAsync(int solicitudId, string usuarioId, string motivo)
        {
            var solicitud = await BuscarSolicitudAsync(solicitudId);
            if (solicitud.Estado != "Pendiente")
                throw new InvalidOperationException($"Solo se rechazan solicitudes pendientes (estado actual: {solicitud.Estado}).");
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("Indique el motivo del rechazo.");

            solicitud.Estado = "Rechazada";
            solicitud.MotivoRechazo = motivo.Trim();
            solicitud.UsuarioAprobadorId = usuarioId;
            solicitud.FechaAtencion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DespacharSolicitudAsync(int solicitudId, string usuarioId)
        {
            var solicitud = await BuscarSolicitudAsync(solicitudId);
            if (solicitud.Estado != "Aprobada")
                throw new InvalidOperationException($"Solo se despachan solicitudes aprobadas (estado actual: {solicitud.Estado}).");
            if (solicitud.AlmacenProveedorId == null)
                throw new InvalidOperationException("La solicitud no tiene almacén proveedor.");

            string concepto = solicitud.Tipo == "Incidencia" ? "Despacho a incidencia" : "Despacho de abastecimiento";
            bool despachado = await DeducirStockAtomicoAsync(solicitud.AlmacenProveedorId.Value, solicitud.RecursoId,
                solicitud.CantidadSolicitada, concepto, solicitud.CodigoSolicitud, usuarioId);
            if (!despachado)
                throw new InvalidOperationException("El stock ya no alcanza para despachar esta solicitud.");

            solicitud.Estado = "Despachada";
            solicitud.FechaAtencion = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Cuando no queda nada pendiente de la incidencia, pasa a Atendida junto con sus reportes
            if (solicitud.IncidenciaId == null) return false;

            bool quedaPendiente = await _context.Solicitudes.AnyAsync(s => s.IncidenciaId == solicitud.IncidenciaId
                                                                          && (s.Estado == "Pendiente" || s.Estado == "Aprobada"));
            if (quedaPendiente) return false;

            var incidencia = await _context.Incidencias
                .Include(i => i.Reportes)
                .FirstAsync(i => i.IncidenciaId == solicitud.IncidenciaId.Value);
            incidencia.Estado = EstadoIncidencia.Atendida;
            foreach (var reporte in incidencia.Reportes)
                reporte.Estado = EstadoIncidente.Atendido;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Solicitud>> EditarPedidoIncidenciaAsync(Incidencia incidencia, IEnumerable<(int RecursoId, int Cantidad)> lineas, string justificacion)
        {
            if (incidencia.Estado != EstadoIncidencia.Constatada)
                throw new InvalidOperationException("Solo se editan pedidos de incidencias constatadas.");

            var existentes = await _context.Solicitudes
                .Where(s => s.IncidenciaId == incidencia.IncidenciaId)
                .ToListAsync();
            if (existentes.Count == 0)
                throw new InvalidOperationException("La incidencia aún no tiene un pedido que editar.");
            if (existentes.Any(s => s.Estado != "Pendiente"))
                throw new InvalidOperationException("El coordinador ya tomó decisiones sobre este pedido; ya no se puede editar.");

            var lista = lineas.ToList();
            if (lista.Count == 0)
                throw new InvalidOperationException("Agregue al menos un bien al pedido.");
            if (lista.Any(l => l.Cantidad <= 0))
                throw new InvalidOperationException("La cantidad de cada bien debe ser mayor a cero.");
            if (lista.GroupBy(l => l.RecursoId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("Un mismo bien no puede repetirse en el pedido.");

            var almacen = await _context.Almacenes
                .FirstOrDefaultAsync(a => a.Nivel == "Distrital" && a.DistritoId == incidencia.DistritoId)
                ?? throw new InvalidOperationException("El distrito no tiene almacén distrital.");

            // Las líneas pendientes se reemplazan por las nuevas; ninguna se duplica
            _context.Solicitudes.RemoveRange(existentes);
            var nuevas = lista.Select(l => new Solicitud
            {
                CodigoSolicitud = $"SOL-{DateTime.UtcNow.Year}-{Random.Shared.Next(1000, 9999)}",
                Tipo = "Incidencia",
                AlmacenSolicitanteId = almacen.AlmacenId,
                AlmacenProveedorId = almacen.AlmacenId,
                RecursoId = l.RecursoId,
                CantidadSolicitada = l.Cantidad,
                Justificacion = justificacion.Trim(),
                IncidenciaId = incidencia.IncidenciaId,
                Estado = "Pendiente",
                FechaSolicitud = DateTime.UtcNow
            }).ToList();

            _context.Solicitudes.AddRange(nuevas);
            await _context.SaveChangesAsync();
            return nuevas;
        }

        private async Task<Solicitud> BuscarSolicitudAsync(int solicitudId)
        {
            return await _context.Solicitudes.FirstOrDefaultAsync(s => s.SolicitudId == solicitudId)
                ?? throw new InvalidOperationException("La solicitud no existe.");
        }

        public async Task<IEnumerable<Solicitud>> ObtenerSolicitudesAsync()
        {
            return await _context.Solicitudes
                .AsNoTracking()
                .Include(s => s.AlmacenSolicitante)
                .Include(s => s.AlmacenProveedor)
                .Include(s => s.Recurso)
                .Include(s => s.Incidencia)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }
    }
}
