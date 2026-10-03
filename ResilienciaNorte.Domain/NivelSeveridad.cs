namespace ResilienciaNorte.Domain
{
    // El valor numérico define la prioridad: a mayor valor, mayor urgencia.
    public enum NivelSeveridad
    {
        Moderado = 1,
        Grave = 2,
        Critico = 3
    }

    public static class NivelSeveridadExtensions
    {
        // Etiqueta persistida en IncidenteEmergencia.Severidad (la vista compara con "Crítico").
        public static string ToEtiqueta(this NivelSeveridad nivel) => nivel switch
        {
            NivelSeveridad.Critico => "Crítico",
            NivelSeveridad.Grave => "Grave",
            _ => "Moderado"
        };

        public static NivelSeveridad DesdeEtiqueta(string? etiqueta) => etiqueta?.Trim() switch
        {
            "Crítico" or "Critico" => NivelSeveridad.Critico,
            "Grave" or "Alto" => NivelSeveridad.Grave,
            _ => NivelSeveridad.Moderado
        };
    }
}
