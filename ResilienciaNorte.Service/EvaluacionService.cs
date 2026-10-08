using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class EvaluacionService : IEvaluacionService
    {
        private const string RolEvaluador = "Evaluador de Campo";
        private const int LongitudMaximaDetalle = 500; // AuditoriaSistema.Detalle

        private readonly ResilienciaDbContext _context;

        public EvaluacionService(ResilienciaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncidenteEmergencia>> ObtenerPendientesAsync()
        {
            var pendientes = await _context.IncidentesEmergencia
                .AsNoTracking()
                .Include(i => i.Distrito)
                .Where(i => i.Estado == EstadoIncidente.Reportado)
                .ToListAsync();

            // La severidad se guarda como texto, por eso la prioridad se ordena en memoria.
            return pendientes
                .OrderByDescending(i => NivelSeveridadExtensions.DesdeEtiqueta(i.Severidad))
                .ThenByDescending(i => i.FamiliasAfectadas)
                .ThenBy(i => i.FechaRegistro)
                .ToList();
        }

        public async Task<IncidenteEmergencia?> ObtenerPendientePorIdAsync(int incidenteId)
        {
            return await _context.IncidentesEmergencia
                .AsNoTracking()
                .Include(i => i.Distrito)
                .FirstOrDefaultAsync(i => i.IncidenteId == incidenteId && i.Estado == EstadoIncidente.Reportado);
        }

        public async Task<ResumenEvaluacion> ObtenerResumenAsync()
        {
            var conteos = await _context.IncidentesEmergencia
                .GroupBy(i => i.Estado)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToListAsync();

            int Total(string estado) => conteos.FirstOrDefault(c => c.Estado == estado)?.Total ?? 0;

            // "Constatados" acumula todo incidente que ya superó la verificación en campo.
            int constatados = Total(EstadoIncidente.Constatado) + Total(EstadoIncidente.ConAsignacion)
                            + Total(EstadoIncidente.EnDespacho) + Total(EstadoIncidente.Atendido);

            return new ResumenEvaluacion(Total(EstadoIncidente.Reportado), constatados, Total(EstadoIncidente.Desestimado));
        }

        public Task<ResultadoEvaluacion> ConstatarAsync(int incidenteId, string evaluadorId, string? observacion, string? ipCliente)
            => EvaluarAsync(incidenteId, EstadoIncidente.Constatado, evaluadorId, observacion, ipCliente);

        public Task<ResultadoEvaluacion> DesestimarAsync(int incidenteId, string evaluadorId, string? observacion, string? ipCliente)
            => EvaluarAsync(incidenteId, EstadoIncidente.Desestimado, evaluadorId, observacion, ipCliente);

        private async Task<ResultadoEvaluacion> EvaluarAsync(int incidenteId, string nuevoEstado, string evaluadorId, string? observacion, string? ipCliente)
        {
            var incidente = await _context.IncidentesEmergencia.FindAsync(incidenteId);
            if (incidente == null)
                return new ResultadoEvaluacion(false, "El incidente no existe.");

            // Solo se evalúa una vez: evita que dos evaluadores contradigan el triaje del otro.
            if (incidente.Estado != EstadoIncidente.Reportado)
                return new ResultadoEvaluacion(false,
                    $"{incidente.CodigoIncidente} ya fue evaluado (estado actual: {incidente.Estado}).",
                    incidente.CodigoIncidente, incidente.Estado);

            string estadoAnterior = incidente.Estado;
            incidente.Estado = nuevoEstado;

            string detalle = $"{incidente.CodigoIncidente}: {estadoAnterior} -> {nuevoEstado}";
            if (!string.IsNullOrWhiteSpace(observacion))
                detalle += $". Obs: {observacion.Trim()}";

            _context.AuditoriaSistema.Add(new AuditoriaSistema
            {
                UsuarioId = evaluadorId,
                RolUsuario = RolEvaluador,
                Operacion = "UPDATE",
                Modulo = "Incidentes",
                Detalle = detalle.Length > LongitudMaximaDetalle ? detalle[..LongitudMaximaDetalle] : detalle,
                IpCliente = ipCliente,
                FechaHoraUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return new ResultadoEvaluacion(true, $"{incidente.CodigoIncidente} marcado como {nuevoEstado}.",
                incidente.CodigoIncidente, nuevoEstado);
        }
    }
}
