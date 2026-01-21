using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    public class Matcha
    {
        public string Nume { get; set; }
        public string Descriere { get; set; }
        public decimal Pret { get; set; }
        public int Cantitate { get; set; }
        public int Calorii { get; set; }

        [JsonConstructor]
        public Matcha(string nume, string descriere, decimal pret, int cantitate, int calorii)
        {
            Nume = nume ?? "";
            Descriere = descriere ?? "";
            Pret = pret;
            Cantitate = cantitate;
            Calorii = calorii;
        }

        public override string ToString() => $"{Nume} ({Pret} RON)";
    }
}