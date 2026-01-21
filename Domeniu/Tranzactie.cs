using System;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class Tranzactie
    {
        public string Id { get; private set; }
        public DateTime Data { get; private set; }
        public decimal Suma { get; private set; }

        public string MatcherieNume { get; private set; } = "";

        [JsonIgnore]
        public Matcherie? Matcherie { get; private set; }

        [JsonConstructor]
        public Tranzactie(string id, DateTime data, decimal suma, string matcherieNume)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
            Data = data;
            Suma = suma;
            MatcherieNume = matcherieNume ?? "";
        }

        public Tranzactie(string id, DateTime data, decimal suma, Matcherie matcherie)
            : this(id, data, suma, matcherie?.Nume ?? "")
        {
            Matcherie = matcherie;
        }

        public void SetMatcherie(Matcherie? matcherie)
        {
            Matcherie = matcherie;
            if (matcherie != null) MatcherieNume = matcherie.Nume;
        }
    }
}