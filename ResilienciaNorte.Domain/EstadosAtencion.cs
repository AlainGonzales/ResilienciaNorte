namespace ResilienciaNorte.Domain
{
    // Estados del ciclo de vida del IncidenteEmergencia (columna Estado, máx. 20 caracteres).
    // Los valores coinciden con los usados por el filtro del dashboard (Incidentes/Index).
    public static class EstadoIncidente
    {
        public const string Reportado = "Reportado";
        public const string Constatado = "Constatado";
        public const string Desestimado = "Desestimado";
        public const string ConAsignacion = "Con Asignación";
        public const string EnDespacho = "En Despacho";
        public const string Atendido = "Atendido";
    }

    // Estados de la OrdenAtencion (cabecera del Maestro-Detalle).
    public static class EstadoOrden
    {
        public const string Aprobada = "Aprobada";
        public const string EnDespacho = "EnDespacho";
        public const string Entregado = "Entregado";
        public const string Anulado = "Anulado";
    }
}
