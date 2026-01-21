using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class ContClient
    {
        public string Nume { get; private set; }
        public string Email { get; private set; }

        public List<Tranzactie> Istoric { get; set; }
        public List<Rezervare> Rezervari { get; set; }

        [JsonConstructor]
        public ContClient(string nume, string email, List<Tranzactie>? istoric, List<Rezervare>? rezervari)
        {
            Nume = nume ?? "";
            Email = email ?? "";
            Istoric = istoric ?? new List<Tranzactie>();
            Rezervari = rezervari ?? new List<Rezervare>();
        }
    }
}