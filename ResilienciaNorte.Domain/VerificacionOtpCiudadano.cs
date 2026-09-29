using System;

namespace ResilienciaNorte.Domain
{
    public class VerificacionOtpCiudadano
    {
        public int OtpId { get; set; }
        public string DniCiudadano { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string CodigoIncidente { get; set; } = string.Empty;

        // PIN de 4 dígitos (se almacena hasheado por seguridad)
        public string PinHash { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; } // Vigencia de 30 segundos
        public int IntentosFallidos { get; set; } = 0;
        public bool FueUtilizado { get; set; } = false;
    }
}