using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service;

/// <summary>
/// Módulo 3 – Logística de Contingencia y Abastecimiento Multinivel.
/// Gestiona ingresos al almacén (CRUD), Kardex inmutable y el árbol de
/// reabastecimiento subsidiario Distrito → Provincia → Región.
/// Las notificaciones SignalR se emiten desde el controlador para mantener
/// la capa de servicio desacoplada del framework web (patrón del proyecto).
/// </summary>
public class AlmacenService : IAlmacenService
{
    private readonly ResilienciaDbContext _context;

    public AlmacenService(ResilienciaDbContext context)
    {
        _context = context;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INVENTARIO
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<RecursoAlmacen>> ObtenerInventarioAsync(
        int? distritoId = null, string? categoria = null)
    {
        var query = _context.RecursosAlmacen
            .Include(r => r.Distrito)
            .AsNoTracking()
            .AsQueryable();

        if (distritoId.HasValue && distritoId.Value > 0)
            query = query.Where(r => r.DistritoId == distritoId.Value);

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(r => r.Categoria == categoria);

        return await query.OrderBy(r => r.Categoria).ThenBy(r => r.Nombre).ToListAsync();
    }

    public async Task<RecursoAlmacen?> ObtenerRecursoPorIdAsync(int recursoId)
    {
        return await _context.RecursosAlmacen
            .Include(r => r.Distrito)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecursoId == recursoId);
    }

    public async Task<IEnumerable<RecursoAlmacen>> ObtenerRecursosConStockCriticoAsync(int? distritoId = null)
    {
        var query = _context.RecursosAlmacen
            .Include(r => r.Distrito)
            .AsNoTracking()
            .Where(r => r.StockDisponible <= r.StockMinimo);

        if (distritoId.HasValue && distritoId.Value > 0)
            query = query.Where(r => r.DistritoId == distritoId.Value);

        return await query.OrderBy(r => r.StockDisponible).ToListAsync();
    }

    public async Task<int> ContarAlertasStockCriticoAsync(int? distritoId = null)
    {
        var query = _context.RecursosAlmacen.AsQueryable();

        if (distritoId.HasValue && distritoId.Value > 0)
            query = query.Where(r => r.DistritoId == distritoId.Value);

        return await query.CountAsync(r => r.StockDisponible <= r.StockMinimo);
    }

    public async Task<IEnumerable<Distrito>> ObtenerDistritosAsync()
    {
        return await _context.Distritos
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .AsNoTracking()
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // INGRESOS AL ALMACÉN (Compra Directa, Donación, Sobrante)
    // Atomicidad garantizada: el stock y el movimiento Kardex se graban en la
    // misma transacción de base de datos.
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<MovimientoAlmacen> RegistrarIngresoAsync(
        int recursoId,
        int distritoId,
        int cantidad,
        string concepto,
        string usuarioId,
        string? documentoReferencia = null,
        string? entidadOrigen = null)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));

        // Transacción explícita para atomicidad: si falla algún paso se revierte todo.
        await using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            // Obtener recurso con tracking activo para poder modificarlo.
            var recurso = await _context.RecursosAlmacen
                .FirstOrDefaultAsync(r => r.RecursoId == recursoId && r.DistritoId == distritoId)
                ?? throw new KeyNotFoundException(
                    $"Recurso ID {recursoId} no encontrado en el distrito ID {distritoId}.");

            int stockAnterior = recurso.StockDisponible;

            // Actualizar stock usando el método de dominio (Experto en Información).
            recurso.IncrementarStock(cantidad);

            // Grabar movimiento inmutable en el Kardex.
            var movimiento = new MovimientoAlmacen
            {
                RecursoId            = recursoId,
                DistritoId           = distritoId,
                TipoMovimiento       = "ENTRADA",
                ConceptoMovimiento   = concepto,   // CompraDirecta | Donacion | Sobrante
                DocumentoReferencia  = documentoReferencia,
                EntidadOrigenDestino = entidadOrigen,
                Cantidad             = cantidad,
                StockAnterior        = stockAnterior,
                StockPosterior       = recurso.StockDisponible,
                UsuarioResponsableId = usuarioId,
                FechaHora            = DateTime.UtcNow
            };

            _context.MovimientosAlmacen.Add(movimiento);
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

    // ═══════════════════════════════════════════════════════════════════════════
    // KARDEX – Consulta del historial inmutable de movimientos
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<IEnumerable<MovimientoAlmacen>> ObtenerKardexAsync(
        int? recursoId = null,
        int? distritoId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var query = _context.MovimientosAlmacen
            .Include(m => m.Recurso)
            .Include(m => m.Distrito)
            .AsNoTracking()
            .AsQueryable();

        if (recursoId.HasValue && recursoId.Value > 0)
            query = query.Where(m => m.RecursoId == recursoId.Value);

        if (distritoId.HasValue && distritoId.Value > 0)
            query = query.Where(m => m.DistritoId == distritoId.Value);

        if (desde.HasValue)
            query = query.Where(m => m.FechaHora >= desde.Value.ToUniversalTime());

        if (hasta.HasValue)
        {
            // Hasta el final del día indicado (inclusive).
            var hastaFin = hasta.Value.ToUniversalTime().Date.AddDays(1).AddTicks(-1);
            query = query.Where(m => m.FechaHora <= hastaFin);
        }

        return await query.OrderByDescending(m => m.FechaHora).ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // REABASTECIMIENTO SUBSIDIARIO Distrito → Provincia → Región
    // ═══════════════════════════════════════════════════════════════════════════

    public async Task<SolicitudReabastecimiento> CrearSolicitudReabastecimientoAsync(
        int distritoOrigenId,
        int recursoId,
        int cantidadSolicitada,
        string nivelDestino,
        string justificacion,
        string usuarioId)
    {
        if (cantidadSolicitada <= 0)
            throw new ArgumentException("La cantidad solicitada debe ser mayor a cero.", nameof(cantidadSolicitada));

        if (nivelDestino != "Provincial" && nivelDestino != "Regional")
            throw new ArgumentException(
                "El nivel destino debe ser 'Provincial' o 'Regional'.", nameof(nivelDestino));

        // Validar existencia de distrito y recurso.
        bool distritoExiste = await _context.Distritos.AnyAsync(d => d.DistritoId == distritoOrigenId);
        if (!distritoExiste)
            throw new KeyNotFoundException($"Distrito ID {distritoOrigenId} no encontrado.");

        bool recursoExiste = await _context.RecursosAlmacen.AnyAsync(r => r.RecursoId == recursoId);
        if (!recursoExiste)
            throw new KeyNotFoundException($"Recurso ID {recursoId} no encontrado.");

        // Regla de negocio: no duplicar solicitudes activas para el mismo recurso/distrito.
        bool solicitudActiva = await _context.SolicitudesReabastecimiento.AnyAsync(s =>
            s.DistritoOrigenId == distritoOrigenId &&
            s.RecursoId        == recursoId &&
            (s.Estado == "Pendiente" || s.Estado == "EnTransito"));

        if (solicitudActiva)
            throw new InvalidOperationException(
                "Ya existe una solicitud activa (Pendiente o En Tránsito) para este recurso en este distrito.");

        // Código único legible: REA-PROV-260930-112345-3
        string prefijo = nivelDestino == "Provincial" ? "PROV" : "REG";
        string codigo  = $"REA-{prefijo}-{DateTime.UtcNow:yyMMdd-HHmmss}-{distritoOrigenId}";

        var solicitud = new SolicitudReabastecimiento
        {
            CodigoSolicitud    = codigo,
            DistritoOrigenId   = distritoOrigenId,
            NivelDestino       = nivelDestino,
            RecursoId          = recursoId,
            CantidadSolicitada = cantidadSolicitada,
            Justificacion      = justificacion,
            Estado             = "Pendiente",
            FechaSolicitud     = DateTime.UtcNow
        };

        _context.SolicitudesReabastecimiento.Add(solicitud);
        await _context.SaveChangesAsync();

        return solicitud;
    }

    public async Task<IEnumerable<SolicitudReabastecimiento>> ObtenerSolicitudesAsync(
        int? distritoId = null, string? estado = null)
    {
        var query = _context.SolicitudesReabastecimiento
            .Include(s => s.DistritoOrigen)
            .Include(s => s.Recurso)
            .AsNoTracking()
            .AsQueryable();

        if (distritoId.HasValue && distritoId.Value > 0)
            query = query.Where(s => s.DistritoOrigenId == distritoId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(s => s.Estado == estado);

        return await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();
    }

    public async Task AprobarSolicitudAsync(int solicitudId, string usuarioAprobadorId)
    {
        var solicitud = await _context.SolicitudesReabastecimiento
            .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId)
            ?? throw new KeyNotFoundException($"Solicitud ID {solicitudId} no encontrada.");

        if (solicitud.Estado != "Pendiente")
            throw new InvalidOperationException(
                $"No se puede aprobar una solicitud en estado '{solicitud.Estado}'.");

        solicitud.Estado             = "Aprobado";
        solicitud.FechaAtencion      = DateTime.UtcNow;
        solicitud.UsuarioAprobadorId = usuarioAprobadorId;

        await _context.SaveChangesAsync();
    }

    public async Task RechazarSolicitudAsync(int solicitudId, string usuarioAprobadorId, string motivo)
    {
        var solicitud = await _context.SolicitudesReabastecimiento
            .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId)
            ?? throw new KeyNotFoundException($"Solicitud ID {solicitudId} no encontrada.");

        if (solicitud.Estado != "Pendiente")
            throw new InvalidOperationException(
                $"No se puede rechazar una solicitud en estado '{solicitud.Estado}'.");

        solicitud.Estado             = "Rechazado";
        solicitud.FechaAtencion      = DateTime.UtcNow;
        solicitud.UsuarioAprobadorId = usuarioAprobadorId;
        // Se reutiliza Justificacion para guardar el motivo de rechazo (no hay campo separado en la entidad).
        solicitud.Justificacion      = $"{solicitud.Justificacion} | MOTIVO RECHAZO: {motivo}";

        await _context.SaveChangesAsync();
    }
}
