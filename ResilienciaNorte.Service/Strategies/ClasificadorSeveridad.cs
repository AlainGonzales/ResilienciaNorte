using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service.Strategies
{
    public record ResultadoSeveridad(NivelSeveridad Nivel, IReadOnlyList<string> Motivos)
    {
        public string Etiqueta => Nivel.ToEtiqueta();
    }

    public interface IClasificadorSeveridad
    {
        ResultadoSeveridad Clasificar(IncidenteEmergencia incidente);
    }

    // Contexto del patrón Strategy: ejecuta todas las estrategias registradas y
    // adopta la severidad más alta (criterio conservador en gestión de emergencias).
    public class ClasificadorSeveridad : IClasificadorSeveridad
    {
        private readonly IReadOnlyList<ISeveridadStrategy> _estrategias;

        public ClasificadorSeveridad(IEnumerable<ISeveridadStrategy> estrategias)
        {
            _estrategias = estrategias.ToList();
        }

        public ResultadoSeveridad Clasificar(IncidenteEmergencia incidente)
        {
            if (_estrategias.Count == 0)
                return new ResultadoSeveridad(NivelSeveridad.Moderado, new[] { "Sin criterios de priorización configurados" });

            var evaluaciones = _estrategias
                .Select(e => (Estrategia: e, Resultado: e.Evaluar(incidente)))
                .ToList();

            var nivelFinal = evaluaciones.Max(x => x.Resultado.Nivel);

            // Motivos: solo los criterios que determinaron el nivel final.
            var motivos = evaluaciones
                .Where(x => x.Resultado.Nivel == nivelFinal)
                .Select(x => $"{x.Estrategia.Nombre}: {x.Resultado.Motivo}")
                .ToList();

            return new ResultadoSeveridad(nivelFinal, motivos);
        }
    }
}
