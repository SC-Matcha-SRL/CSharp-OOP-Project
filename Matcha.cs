using System.Text.Json.Serialization;

namespace ConsoleApp5
{
    /// <summary>
    /// Model produs: un item din meniul unei matcherii.
    /// </summary>
    public class Matcha
    {
        [JsonInclude] public string nume { get; set; }
        public string descriere { get; set; }
        public decimal pret { get; set; }
        public int cantitate { get; set; }
        public int calorii { get; set; }

        public Matcha(string nume, string descriere, decimal pret, int cantitate, int calorii)
        {
            this.nume = nume;
            this.descriere = descriere;
            this.pret = pret;
            this.cantitate = cantitate;
            this.calorii = calorii;
        }
    }
}