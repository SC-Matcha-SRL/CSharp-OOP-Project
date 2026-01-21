using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class Matcherie
    {
        public string Nume { get; private set; }
        public string Program { get; private set; }
        public int Capacitate { get; private set; }

        public List<Matcha> Meniu { get; set; }
        public List<Rezervare> Rezervari { get; set; }

        [JsonConstructor]
        public Matcherie(string nume, string program, int capacitate, List<Matcha>? meniu, List<Rezervare>? rezervari)
        {
            Nume = nume ?? "";
            Program = program ?? "";
            Capacitate = capacitate;
            Meniu = meniu ?? new List<Matcha>();
            Rezervari = rezervari ?? new List<Rezervare>();
        }

        public void SetProgram(string noulProgram)
        {
            if (!string.IsNullOrWhiteSpace(noulProgram))
                Program = noulProgram;
        }

        public void SetCapacitate(int nouaCapacitate)
        {
            if (nouaCapacitate > 0)
                Capacitate = nouaCapacitate;
        }
    }
}