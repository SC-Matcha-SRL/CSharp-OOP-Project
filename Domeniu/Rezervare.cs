using System;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class Rezervare : IEquatable<Rezervare>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Tip { get; set; } = "";
        public decimal Pret { get; set; }
        public string Limitari { get; set; } = "";
        public string Beneficii { get; set; } = "";
        public string ClientID { get; set; } = "";

        // Persistam doar numele (evitam cicluri JSON)
        public string MatcherieNume { get; set; } = "";

        [JsonIgnore]
        public Matcherie? Matcherie { get; set; }

        [JsonConstructor]
        public Rezervare(string? id, string tip, decimal pret, string limitari, string beneficii, string clientID, string matcherieNume)
        {
            Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
            Tip = tip ?? "";
            Pret = pret;
            Limitari = limitari ?? "";
            Beneficii = beneficii ?? "";
            ClientID = clientID ?? "";
            MatcherieNume = matcherieNume ?? "";
        }

        public Rezervare(string tip, decimal pret, string limitari, string beneficii, string clientID, Matcherie matcherie)
            : this(Guid.NewGuid().ToString(), tip, pret, limitari, beneficii, clientID, matcherie?.Nume ?? "")
        {
            Matcherie = matcherie;
        }

        public override string ToString() => $"{Tip} - {Pret} RON";

        public bool Equals(Rezervare? other) =>
            other != null && string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => obj is Rezervare r && Equals(r);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Id ?? "");
    }
}