using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service.Strategies
{
    // Criterio 1: magnitud del impacto humano según el número de familias afectadas.
    public class SeveridadPorFamiliasStrategy : ISeveridadStrategy
    {
        public const int UmbralCritico = 50;
        public const int UmbralGrave = 15;

        public string Nombre => "Familias afectadas";

        public ResultadoEstrategia Evaluar(IncidenteEmergencia incidente)
        {
            int familias = incidente.FamiliasAfectadas;

            if (familias >= UmbralCritico)
                return new(NivelSeveridad.Critico, $"{familias} familias afectadas (≥ {UmbralCritico})");

            if (familias >= UmbralGrave)
                return new(NivelSeveridad.Grave, $"{familias} familias afectadas (≥ {UmbralGrave})");

            return new(NivelSeveridad.Moderado, $"{familias} familias afectadas");
        }
    }
}
