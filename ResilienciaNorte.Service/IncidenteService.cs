using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;

namespace ResilienciaNorte.Service
{
    public class IncidenteService : IIncidenteService
    {
        private readonly ResilienciaDbContext _context;

        public IncidenteService(ResilienciaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IncidenteEmergencia>> ObtenerTodosAsync(int? distritoId, string? estado)
        {
            var query = _context.IncidentesEmergencia.Include(i => i.Distrito).AsQueryable();

            if (distritoId.HasValue && distritoId.Value > 0)
                query = query.Where(i => i.DistritoId == distritoId.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(i => i.Estado == estado);

            return await query.OrderByDescending(i => i.FechaRegistro).ToListAsync();
        }

        public async Task<IncidenteEmergencia?> ObtenerPorIdAsync(int incidenteId)
        {
            return await _context.IncidentesEmergencia
                .Include(i => i.Distrito)
                .FirstOrDefaultAsync(i => i.IncidenteId == incidenteId);
        }

        public async Task<IncidenteEmergencia?> ObtenerPorCodigoAsync(string codigoIncidente)
        {
            return await _context.IncidentesEmergencia
                .Include(i => i.Distrito)
                .FirstOrDefaultAsync(i => i.CodigoIncidente == codigoIncidente.Trim().ToUpper());
        }

        public async Task<IEnumerable<Distrito>> ObtenerDistritosAsync()
        {
            return await _context.Distritos.Where(d => d.Activo).OrderBy(d => d.Nombre).ToListAsync();
        }

        public async Task<IncidenteEmergencia> RegistrarIncidenteAsync(IncidenteEmergencia incidente)
        {
            // 1. Generar código correlativo seguro ALT-2026-XXXX si no viene
            if (string.IsNullOrWhiteSpace(incidente.CodigoIncidente))
            {
                int totalHoy = await _context.IncidentesEmergencia.CountAsync() + 1;
                incidente.CodigoIncidente = $"ALT-{TimeHelper.Ahora.Year}-{totalHoy:D4}";
            }

            incidente.FechaRegistro = TimeHelper.Ahora;
            incidente.Estado = "Reportado";

            _context.IncidentesEmergencia.Add(incidente);
            await _context.SaveChangesAsync();
            return incidente;
        }

        public async Task<bool> CambiarEstadoAsync(int incidenteId, string nuevoEstado)
        {
            var incidente = await _context.IncidentesEmergencia.FindAsync(incidenteId);
            if (incidente == null) return false;

            incidente.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            return true;
        }

        // ── 2. Generación OTP de 30 Segundos con Hash SHA-256 ─────────────────
        public async Task<string> GenerarOtpCiudadanoAsync(string codigoIncidente, string dni, string telefono)
        {
            // Generar PIN aleatorio de 6 dígitos
            string pin = new Random().Next(100000, 999999).ToString();
            string pinHash = HashString(pin);

            // Invalidar tokens previos del mismo incidente
            var tokensPrevios = await _context.VerificacionesOtpCiudadano
                .Where(v => v.CodigoIncidente == codigoIncidente && !v.FueUtilizado)
                .ToListAsync();

            foreach (var t in tokensPrevios)
            {
                t.FueUtilizado = true;
            }

            var otp = new VerificacionOtpCiudadano
            {
                CodigoIncidente = codigoIncidente.Trim().ToUpper(),
                DniCiudadano = dni.Trim(),
                Telefono = telefono.Trim(),
                PinHash = pinHash,
                FechaCreacion = TimeHelper.Ahora,
                FechaExpiracion = TimeHelper.Ahora.AddSeconds(30), // Ventana estricta de 30 segundos
                IntentosFallidos = 0,
                FueUtilizado = false
            };

            _context.VerificacionesOtpCiudadano.Add(otp);
            await _context.SaveChangesAsync();

            return pin; // Retorna el PIN en claro para simular el SMS o mostrarlo en pantalla
        }

        // ── 3. Validación de Token OTP ───────────────────────────────────────
        public async Task<bool> ValidarOtpCiudadanoAsync(string codigoIncidente, string dni, string pinIngresado)
        {
            string hashIngresado = HashString(pinIngresado.Trim());
            var ahora = TimeHelper.Ahora;

            var otp = await _context.VerificacionesOtpCiudadano
                .Where(v => v.CodigoIncidente == codigoIncidente.Trim().ToUpper() && v.DniCiudadano == dni.Trim() && !v.FueUtilizado)
                .OrderByDescending(v => v.FechaCreacion)
                .FirstOrDefaultAsync();

            if (otp == null) return false;

            // Verificar si expiró (más de 30 segundos)
            if (ahora > otp.FechaExpiracion)
            {
                otp.FueUtilizado = true;
                await _context.SaveChangesAsync();
                return false;
            }

            if (otp.PinHash != hashIngresado)
            {
                otp.IntentosFallidos++;
                if (otp.IntentosFallidos >= 3)
                {
                    otp.FueUtilizado = true; // Bloqueo tras 3 fallos
                }
                await _context.SaveChangesAsync();
                return false;
            }

            // Consumir el token
            otp.FueUtilizado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private static string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}