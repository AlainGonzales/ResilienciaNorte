namespace ResilienciaNorte.Domain
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo ZonaHoraPerú = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

        /// <summary>
        /// Obtiene la hora actual de Perú (UTC-5)
        /// </summary>
        public static DateTime Ahora => TimeZoneInfo.ConvertTime(DateTime.Now, ZonaHoraPerú);

        /// <summary>
        /// Convierte una hora UTC a la zona horaria de Perú
        /// </summary>
        public static DateTime ConvertirAPerú(DateTime tiempoUTC) => TimeZoneInfo.ConvertTime(tiempoUTC, ZonaHoraPerú);

        /// <summary>
        /// Obtiene la hora UTC actual
        /// </summary>
        public static DateTime AhoraUTC => DateTime.UtcNow;
    }
}
