using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class SistemMatcha
    {
        public List<Matcherie> Magazine { get; set; } = new();
        public List<ContClient> Clienti { get; set; } = new();
        public List<ContAdmin> Administratori { get; set; } = new();
        public List<TipRezervare> TipuriRezervari { get; set; } = new();

        public SistemMatcha() { }

        [JsonConstructor]
        public SistemMatcha(List<Matcherie>? magazine, List<ContClient>? clienti, List<ContAdmin>? administratori, List<TipRezervare>? tipuriRezervari)
        {
            Magazine = magazine ?? new();
            Clienti = clienti ?? new();
            Administratori = administratori ?? new();
            TipuriRezervari = tipuriRezervari ?? new();
        }

        public void AsiguraColectii()
        {
            Magazine ??= new();
            Clienti ??= new();
            Administratori ??= new();
            TipuriRezervari ??= new();

            foreach (var m in Magazine)
            {
                m.Meniu ??= new List<Matcha>();
                m.Rezervari ??= new List<Rezervare>();
            }

            foreach (var c in Clienti)
            {
                c.Istoric ??= new List<Tranzactie>();
                c.Rezervari ??= new List<Rezervare>();
            }
        }

        /// Reface referintele dupa JSON si elimina duplicatele.
        public void ReconecteazaReferinte()
        {
            AsiguraColectii();

            var matcheriiByName = new Dictionary<string, Matcherie>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in Magazine)
                if (!string.IsNullOrWhiteSpace(m.Nume))
                    matcheriiByName[m.Nume] = m;

            // Canon: rezervari din matcherii (pe Id)
            var rezById = new Dictionary<string, Rezervare>(StringComparer.OrdinalIgnoreCase);

            foreach (var m in Magazine)
            {
                var unique = new Dictionary<string, Rezervare>(StringComparer.OrdinalIgnoreCase);

                foreach (var r in m.Rezervari)
                {
                    if (r == null) continue;

                    if (string.IsNullOrWhiteSpace(r.Id))
                        r.Id = Guid.NewGuid().ToString();

                    r.Matcherie = m;
                    r.MatcherieNume = m.Nume;

                    unique[r.Id] = r;
                    rezById[r.Id] = r;
                }

                m.Rezervari = unique.Values.ToList();
            }

            foreach (var c in Clienti)
            {
                // Rezervari: inlocuieste cu instanta canon (din matcherie), dedup
                var fixedRez = new List<Rezervare>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var r in c.Rezervari)
                {
                    if (r == null) continue;

                    if (string.IsNullOrWhiteSpace(r.Id))
                        r.Id = Guid.NewGuid().ToString();

                    var canon = rezById.TryGetValue(r.Id, out var found) ? found : r;

                    if (canon.Matcherie == null &&
                        !string.IsNullOrWhiteSpace(canon.MatcherieNume) &&
                        matcheriiByName.TryGetValue(canon.MatcherieNume, out var m))
                    {
                        canon.Matcherie = m;
                        canon.MatcherieNume = m.Nume;
                    }

                    if (seen.Add(canon.Id))
                        fixedRez.Add(canon);
                }

                c.Rezervari = fixedRez;

                // Tranzactii: reataseaza matcheria
                foreach (var t in c.Istoric)
                {
                    if (t == null) continue;

                    if (t.Matcherie == null &&
                        !string.IsNullOrWhiteSpace(t.MatcherieNume) &&
                        matcheriiByName.TryGetValue(t.MatcherieNume, out var m))
                    {
                        t.SetMatcherie(m);
                    }
                }
            }
        }
    }
}
