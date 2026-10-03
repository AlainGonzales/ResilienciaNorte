using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service.Strategies
{
    // Patrón Strategy: cada criterio de priorización encapsula su propia regla de severidad.
    // Agregar un criterio nuevo = crear una clase que implemente esta interfaz y registrarla en DI
    // (Open/Closed: el contexto ClasificadorSeveridad no cambia).
    public interface ISeveridadStrategy
    {
        string Nombre { get; }

        // Devuelve el nivel que este criterio asigna al incidente y el motivo legible.
        ResultadoEstrategia Evaluar(IncidenteEmergencia incidente);
    }

    public record ResultadoEstrategia(NivelSeveridad Nivel, string Motivo);
}
