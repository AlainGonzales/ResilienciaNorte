using System.Globalization;
using System.Text;
using ResilienciaNorte.Domain;

namespace ResilienciaNorte.Service.Strategies
{
    // Criterio 2: peligrosidad de la quebrada / cauce donde ocurre el evento.
    // Busca el nombre de la quebrada en el sector crítico y la referencia del reporte.
    public class SeveridadPorQuebradaStrategy : ISeveridadStrategy
    {
        // Catálogo de quebradas con historial de activación en la Provincia de Trujillo.
        // Los patrones se comparan sin tildes ni mayúsculas. Ajustar según el análisis de riesgo del COEP.
        private static readonly (string Patron, string Nombre, NivelSeveridad Nivel)[] QuebradasConocidas =
        {
            ("san ildefonso", "Quebrada San Ildefonso", NivelSeveridad.Critico),
            ("san idelfonso", "Quebrada San Ildefonso", NivelSeveridad.Critico),
            ("rio seco",      "Quebrada Río Seco",      NivelSeveridad.Critico),
            ("leon",          "Quebrada León",          NivelSeveridad.Grave),
            ("cabras",        "Quebrada Cabras",        NivelSeveridad.Grave),
        };

        // Términos que indican cauce torrentoso aunque la quebrada no esté catalogada.
        private static readonly string[] TerminosDeCauce = { "quebrada", "huaico", "torrentera" };

        public string Nombre => "Quebrada / cauce";

        public ResultadoEstrategia Evaluar(IncidenteEmergencia incidente)
        {
            string texto = Normalizar($"{incidente.SectorCritico} {incidente.DireccionReferencia}");

            // Si coinciden varias quebradas, prevalece la de mayor nivel.
            var conocida = QuebradasConocidas
                .Where(q => texto.Contains($" {q.Patron} "))
                .OrderByDescending(q => q.Nivel)
                .Cast<(string Patron, string Nombre, NivelSeveridad Nivel)?>()
                .FirstOrDefault();

            if (conocida is { } q)
                return new(q.Nivel, $"{q.Nombre} (cauce de alto riesgo)");

            bool mencionaCauce = TerminosDeCauce.Any(t => texto.Contains($" {t} "))
                || Normalizar(incidente.TipoEvento).Contains(" quebrada ");

            if (mencionaCauce)
                return new(NivelSeveridad.Grave, "Evento asociado a quebrada o cauce torrentoso");

            return new(NivelSeveridad.Moderado, "Sin quebrada identificada");
        }

        // Sin tildes, en minúsculas y con la puntuación convertida en espacios; el resultado va
        // entre espacios para poder buscar palabras completas (" leon " no coincide con "Leoncio").
        private static string Normalizar(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return " ";

            var descompuesto = valor.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToLowerInvariant(c));
                else if (sb.Length > 0 && sb[^1] != ' ')
                    sb.Append(' '); // un solo espacio por separador
            }
            return $" {sb} ";
        }
    }
}
