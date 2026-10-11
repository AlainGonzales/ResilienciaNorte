namespace ResilienciaNorte.Domain
{
    // Estado del reporte ciudadano (columna Estado de IncidenteEmergencia).
    // Un reporte toma el estado de la incidencia a la que pertenece.
    public static class EstadoIncidente
    {
        public const string Reportado = "Reportado";
        public const string Asignado = "Asignado";
        public const string Constatado = "Constatado";
        public const string Desestimado = "Desestimado";
        public const string Atendido = "Atendido";
    }

    // Estado de la incidencia creada por el analista a partir de reportes similares.
    public static class EstadoIncidencia
    {
        public const string Abierta = "Abierta";
        public const string Asignada = "Asignada";
        public const string Constatada = "Constatada";
        public const string Desestimada = "Desestimada";
        // Todos sus bienes fueron despachados
        public const string Atendida = "Atendida";
    }
}
