using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public static class GestiuneDate
    {
        private const string CaleFisier = "baza_date_matcha.json";

        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void SalveazaTot(SistemMatcha sistem)
        {
            sistem.ReconecteazaReferinte();
            string json = JsonSerializer.Serialize(sistem, Options);
            File.WriteAllText(CaleFisier, json);
        }

        public static SistemMatcha IncarcaTot()
        {
            if (!File.Exists(CaleFisier))
                return new SistemMatcha();

            try
            {
                string json = File.ReadAllText(CaleFisier);
                var sistem = JsonSerializer.Deserialize<SistemMatcha>(json, Options) ?? new SistemMatcha();
                sistem.ReconecteazaReferinte();
                return sistem;
            }
            catch
            {
                return new SistemMatcha();
            }
        }
    }
}