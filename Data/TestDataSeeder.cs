namespace ConsoleApp5
{
    /// <summary>
    /// Seed de date demo (folosit când sistemul e gol).
    /// </summary>
    public static class TestDataSeeder
    {
        public static void IncarcaDateTest(SistemMatcha sistem)
        {
            sistem.Magazine ??= new List<Matcherie>();
            sistem.Clienti ??= new List<Client>();
            sistem.Administratori ??= new List<AdministratorMatcha>();

            var meniu = new List<Matcha> { new Matcha("Matcha Latte", "Clasic", 22.5m, 100, 120) };
            var m1 = new Matcherie("Matcha Zen", "08-22", 20, meniu, new List<Rezervare>());

            var meniu2 = new List<Matcha> { new Matcha("Matcha Latte vf. Odaia", "Clasic amar", 22.5m, 100, 120) };
            var m3 = new Matcherie("Matcha urzica sanicolau nou", "08-22", 20, meniu2, new List<Rezervare>());

            sistem.Magazine.Add(m1);
            sistem.Magazine.Add(m3);

            sistem.Administratori.Add(new AdministratorMatcha("Admin", "ADM01", "1234", sistem.Magazine));
            sistem.Administratori.Add(new AdministratorMatcha("Admin22", "ADM02", "1234", sistem.Magazine));

            sistem.Clienti.Add(new Client("Andrei", "andrei@email.com", new List<Tranzactie>(), new List<Rezervare>()));
        }
    }
}