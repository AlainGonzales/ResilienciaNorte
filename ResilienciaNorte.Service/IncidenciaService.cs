using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class IncidenciaService : IIncidenciaService
    {
        // Ventana de tiempo para sugerir reportes del mismo hecho
        public const int VentanaSugerenciaHoras = 4;

        private readonly ResilienciaDbContext _context;

        public IncidenciaService(ResilienciaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncidenteEmergencia>> SugerirReportesAsync(int reporteId)
        {
            var referencia = await _context.IncidentesEmergencia.AsNoTracking()
                .FirstOrDefaultAsync(i => i.IncidenteId == reporteId);
            if (referencia == null) return Enumerable.Empty<IncidenteEmergencia>();

            var desde = referencia.FechaRegistro.AddHours(-VentanaSugerenciaHoras);
            var hasta = referencia.FechaRegistro.AddHours(VentanaSugerenciaHoras);

            return await _context.IncidentesEmergencia
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Where(i => i.Estado == EstadoIncidente.Reportado
                            && i.IncidenciaId == null
                            && i.DistritoId == referencia.DistritoId
                            && i.TipoEvento == referencia.TipoEvento
                            && i.FechaRegistro >= desde
                            && i.FechaRegistro <= hasta)
                .OrderBy(i => i.FechaRegistro)
                .ToListAsync();
        }

        public async Task<Incidencia> CrearAsync(IEnumerable<int> reporteIds, string sector, string tipoEvento, string? observacion, int? distritoAnalista)
        {
            var ids = reporteIds.Distinct().ToList();
            if (ids.Count == 0)
                throw new InvalidOperationException("Seleccione al menos un reporte para crear la incidencia.");

            var reportes = await _context.IncidentesEmergencia
                .Where(i => ids.Contains(i.IncidenteId))
                .ToListAsync();

            if (reportes.Count != ids.Count)
                throw new InvalidOperationException("Alguno de los reportes seleccionados ya no existe.");

            if (reportes.Any(r => r.Estado != EstadoIncidente.Reportado || r.IncidenciaId != null))
                throw new InvalidOperationException("Solo se agrupan reportes pendientes que no estén en otra incidencia.");

            if (distritoAnalista.HasValue && reportes.Any(r => r.DistritoId != distritoAnalista.Value))
                throw new InvalidOperationException("Solo puede agrupar reportes de su distrito.");

            if (reportes.Select(r => r.TipoEvento).Distinct().Count() > 1)
                throw new InvalidOperationException("Los reportes de una incidencia deben ser del mismo tipo de evento.");

            if (reportes.Select(r => r.DistritoId).Distinct().Count() > 1)
                throw new InvalidOperationException("Los reportes de una incidencia deben pertenecer al mismo distrito.");

            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                string prefijoInc = $"INC-{DateTime.UtcNow.Year}-";
                var codigosInc = await _context.Incidencias
                    .Where(i => i.Codigo.StartsWith(prefijoInc))
                    .Select(i => i.Codigo)
                    .ToListAsync();
                int correlativo = codigosInc
                    .Select(c => int.TryParse(c.Substring(prefijoInc.Length), out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;
                var incidencia = new Incidencia
                {
                    Codigo = $"{prefijoInc}{correlativo:D4}",
                    DistritoId = reportes[0].DistritoId,
                    Sector = sector.Trim(),
                    TipoEvento = reportes[0].TipoEvento,
                    Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
                    Estado = EstadoIncidencia.Abierta,
                    FechaCreacion = DateTime.UtcNow
                };

                foreach (var reporte in reportes)
                    incidencia.Reportes.Add(reporte);

                _context.Incidencias.Add(incidencia);
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return incidencia;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Incidencia>> ObtenerAsync(string? estado, int? distritoId)
        {
            var query = _context.Incidencias
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Include(i => i.Reportes)
                .Include(i => i.FichaEdan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(i => i.Estado == estado);

            if (distritoId.HasValue)
                query = query.Where(i => i.DistritoId == distritoId.Value);

            return await query.OrderByDescending(i => i.FechaCreacion).ToListAsync();
        }

        public async Task<Incidencia?> ObtenerPorIdAsync(int incidenciaId)
        {
            return await _context.Incidencias
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Include(i => i.Reportes)
                .Include(i => i.FichaEdan)
                .FirstOrDefaultAsync(i => i.IncidenciaId == incidenciaId);
        }

        public async Task AsignarEvaluadorAsync(int incidenciaId, string evaluadorId)
        {
            var incidencia = await _context.Incidencias
                .Include(i => i.Reportes)
                .FirstOrDefaultAsync(i => i.IncidenciaId == incidenciaId)
                ?? throw new InvalidOperationException("La incidencia no existe.");

            if (incidencia.Estado != EstadoIncidencia.Abierta)
                throw new InvalidOperationException($"Solo se asignan incidencias abiertas (estado actual: {incidencia.Estado}).");

            incidencia.Estado = EstadoIncidencia.Asignada;
            incidencia.UsuarioEvaluadorId = evaluadorId;
            incidencia.FechaAsignacion = DateTime.UtcNow;

            foreach (var reporte in incidencia.Reportes)
                reporte.Estado = EstadoIncidente.Asignado;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Incidencia>> ObtenerDelEvaluadorAsync(string? evaluadorId)
        {
            var query = _context.Incidencias
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Include(i => i.Reportes)
                .Include(i => i.FichaEdan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(evaluadorId))
                query = query.Where(i => i.UsuarioEvaluadorId == evaluadorId);

            // Primero las pendientes de resolver, luego las ya resueltas
            return await query
                .Where(i => i.UsuarioEvaluadorId != null)
                .OrderBy(i => i.Estado == EstadoIncidencia.Asignada ? 0 : 1)
                .ThenByDescending(i => i.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<Incidencia> ResolverAsync(int incidenciaId, string usuarioId, bool esAdministrador, bool constatada, string? observacion, FichaEdanDatos? ficha)
        {
            var incidencia = await _context.Incidencias
                .Include(i => i.Reportes)
                .Include(i => i.FichaEdan)
                .FirstOrDefaultAsync(i => i.IncidenciaId == incidenciaId)
                ?? throw new InvalidOperationException("La incidencia no existe.");

            if (incidencia.Estado != EstadoIncidencia.Asignada)
                throw new InvalidOperationException($"Solo se resuelven incidencias asignadas (estado actual: {incidencia.Estado}).");

            // El administrador puede resolver cualquier incidencia; el evaluador solo las suyas
            if (!esAdministrador && incidencia.UsuarioEvaluadorId != usuarioId)
                throw new InvalidOperationException("Solo el evaluador asignado puede resolver esta incidencia.");

            string? texto = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (!constatada && texto == null)
                throw new InvalidOperationException("Debe indicar el motivo para desestimar la incidencia.");

            if (constatada)
            {
                if (ficha == null)
                    throw new InvalidOperationException("Complete la ficha EDAN antes de constatar la incidencia.");
                if (ficha.FamiliasAfectadas < 0 || ficha.ViviendasAfectadas < 0 || ficha.PersonasAfectadas < 0)
                    throw new InvalidOperationException("Los totales de la ficha EDAN no pueden ser negativos.");

                incidencia.FichaEdan = new FichaEdan
                {
                    IncidenciaId = incidencia.IncidenciaId,
                    FamiliasAfectadas = ficha.FamiliasAfectadas,
                    ViviendasAfectadas = ficha.ViviendasAfectadas,
                    PersonasAfectadas = ficha.PersonasAfectadas,
                    NecesidadesUrgentes = string.IsNullOrWhiteSpace(ficha.NecesidadesUrgentes) ? null : ficha.NecesidadesUrgentes.Trim(),
                    Observaciones = string.IsNullOrWhiteSpace(ficha.Observaciones) ? null : ficha.Observaciones.Trim(),
                    FechaVisita = ficha.FechaVisita,
                    UsuarioEvaluadorId = usuarioId,
                    FechaRegistro = DateTime.UtcNow
                };
            }

            incidencia.Estado = constatada ? EstadoIncidencia.Constatada : EstadoIncidencia.Desestimada;
            incidencia.ObservacionResolucion = texto;
            string estadoReporte = constatada ? EstadoIncidente.Constatado : EstadoIncidente.Desestimado;

            foreach (var reporte in incidencia.Reportes)
                reporte.Estado = estadoReporte;

            await _context.SaveChangesAsync();
            return incidencia;
        }
    }
}
