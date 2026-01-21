using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp5
{
    public static class SeedDateTest
    {
        public static void IncarcaDateTest(SistemMatcha sistem)
        {
            sistem.AsiguraColectii();

            // Reservation types (EN - user-facing)
            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Family",
                35m,
                "Min 3 people, max 6. Standard duration 2h. Recommended: book at least 12h in advance.",
                "Bigger table + quiet area; 10% off a matcha dessert for the group."
            ));

            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Friends",
                25m,
                "2-8 people. Standard duration 2h. No table changes after confirmation.",
                "1 still water free for the group + boardgames access (where available)."
            ));

            AddTipRezervareIfMissing(sistem, new TipRezervare(
                "Birthday",
                55m,
                "Min 6 people. Requires confirmation at least 24h in advance. Duration 2h 30m.",
                "Small decor + message card + mini matcha dessert for the celebrant (while stock lasts)."
            ));

            // Matcheries
            var m1 = GetOrCreateMatcherie(sistem, "Matcha Zen", "07:30-22:30", 26);
            var m2 = GetOrCreateMatcherie(sistem, "Matcha Odaia", "09:00-01:00", 42);
            var m3 = GetOrCreateMatcherie(sistem, "Matcha Harbor", "10:00-20:00", 18);

            // Products
            var latte = new Matcha("Matcha Latte", "Classic milk + matcha", 22.5m, 120, 180);
            var coffee = new Matcha("Matcha Coffee", "Espresso + matcha", 24.9m, 90, 160);
            var vodka = new Matcha("Matcha Vodka", "Matcha + lime + vodka (18+)", 34.0m, 40, 220);

            var tiramisu = new Matcha("Matcha Tiramisu", "Creamy dessert with intense matcha notes", 19.9m, 35, 330);
            var cheesecake = new Matcha("Matcha Cheesecake", "Crispy crust cheesecake with matcha", 21.5m, 28, 410);
            var mochi = new Matcha("Matcha Mochi", "2 mochi pieces, soft & chewy", 16.0m, 60, 210);
            var brownie = new Matcha("Matcha Brownie", "Dense brownie with matcha", 18.5m, 45, 360);

            var stillWater = new Matcha("Still Water", "Cold still water (0 kcal)", 9.5m, 200, 0);
            var sparkWater = new Matcha("Sparkling Water", "Mineral sparkling water (0 kcal)", 10.0m, 180, 0);

            // Zen
            AddProductIfMissing(m1, Clone(latte));
            AddProductIfMissing(m1, Clone(coffee));
            AddProductIfMissing(m1, Clone(mochi));
            AddProductIfMissing(m1, Clone(tiramisu));
            AddProductIfMissing(m1, Clone(stillWater));

            // Odaia
            AddProductIfMissing(m2, Clone(latte));
            AddProductIfMissing(m2, Clone(coffee));
            AddProductIfMissing(m2, Clone(vodka));
            AddProductIfMissing(m2, Clone(cheesecake));
            AddProductIfMissing(m2, Clone(brownie));
            AddProductIfMissing(m2, Clone(sparkWater));

            // Harbor
            AddProductIfMissing(m3, new Matcha("Matcha Ice Cream", "Creamy matcha ice cream", 17.5m, 55, 250));
            AddProductIfMissing(m3, Clone(latte));
            AddProductIfMissing(m3, new Matcha("Matcha Lemonade", "Lemonade with matcha and mint", 18.0m, 80, 120));
            AddProductIfMissing(m3, Clone(mochi));
            AddProductIfMissing(m3, Clone(stillWater));
            AddProductIfMissing(m3, Clone(sparkWater));

            // Clients
            var c1 = GetOrCreateClient(sistem, "Andrei Popa", "andrei@email.com");
            var c2 = GetOrCreateClient(sistem, "Mara Dima", "mara@email.com");
            var c3 = GetOrCreateClient(sistem, "Radu Popescu", "radu@email.com");
            var c4 = GetOrCreateClient(sistem, "Ioana Stoica", "ioana@email.com");
            var c5 = GetOrCreateClient(sistem, "Vlad Ionescu", "vlad@email.com");

            // Admins
            AddAdminIfMissing(sistem, new ContAdmin("Admin", "ADM01", "1234"));
            AddAdminIfMissing(sistem, new ContAdmin("Admin2", "ADM02", "1234"));

            // Reservations to vary occupancy
            SeedRezervariIfFew(m1, new[]
            {
                new Rezervare("Family", 35m, "Min 3, max 6", "Big table + 10% dessert", c1.Nume, m1),
                new Rezervare("Friends", 25m, "2-8", "Free water + boardgames", c2.Nume, m1),
                new Rezervare("Birthday", 55m, "Min 6, 24h in advance", "Decor + mini dessert", c3.Nume, m1),
            }, minCount: 2);

            SeedRezervariIfFew(m2, new[]
            {
                new Rezervare("Friends", 25m, "2-8", "Free water + boardgames", c4.Nume, m2),
                new Rezervare("Birthday", 55m, "Min 6, 24h in advance", "Decor + mini dessert", c2.Nume, m2),
                new Rezervare("Friends", 25m, "2-8", "Free water + boardgames", c5.Nume, m2),
                new Rezervare("Family", 35m, "Min 3, max 6", "Big table + 10% dessert", c1.Nume, m2),
            }, minCount: 3);

            SeedRezervariIfFew(m3, new[]
            {
                new Rezervare("Family", 35m, "Min 3, max 6", "Big table + 10% dessert", c3.Nume, m3),
            }, minCount: 1);

            // Transactions for charts
            SeedTranzactiiIfEmpty(c1, m1, new[] { 22.5m, 19.9m, 9.5m });
            SeedTranzactiiIfEmpty(c2, m2, new[] { 24.9m, 21.5m });
            SeedTranzactiiIfEmpty(c3, m2, new[] { 34.0m, 18.5m });
            SeedTranzactiiIfEmpty(c4, m3, new[] { 18.0m, 10.0m });
            SeedTranzactiiIfEmpty(c5, m1, new[] { 22.5m });

            sistem.ReconecteazaReferinte();
        }

        private static void AddTipRezervareIfMissing(SistemMatcha sistem, TipRezervare tip)
        {
            if (sistem.TipuriRezervari.Any(x => x.Nume.Equals(tip.Nume, StringComparison.OrdinalIgnoreCase)))
                return;

            sistem.TipuriRezervari.Add(tip);
        }

        private static Matcherie GetOrCreateMatcherie(SistemMatcha sistem, string nume, string program, int capacitate)
        {
            var existing = sistem.Magazine.FirstOrDefault(m => m.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
            if (existing != null) return existing;

            var created = new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>());
            sistem.Magazine.Add(created);
            return created;
        }

        private static void AddProductIfMissing(Matcherie matcherie, Matcha produs)
        {
            if (matcherie.Meniu == null) matcherie.Meniu = new List<Matcha>();

            bool exists = matcherie.Meniu.Any(p => p.Nume.Equals(produs.Nume, StringComparison.OrdinalIgnoreCase));
            if (!exists) matcherie.Meniu.Add(produs);
        }

        private static ContClient GetOrCreateClient(SistemMatcha sistem, string nume, string email)
        {
            var existing = sistem.Clienti.FirstOrDefault(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
            if (existing != null) return existing;

            var created = new ContClient(nume, email, new List<Tranzactie>(), new List<Rezervare>());
            sistem.Clienti.Add(created);
            return created;
        }

        private static void AddAdminIfMissing(SistemMatcha sistem, ContAdmin admin)
        {
            bool exists = sistem.Administratori.Any(a => a.AdminId.Equals(admin.AdminId, StringComparison.OrdinalIgnoreCase));
            if (!exists) sistem.Administratori.Add(admin);
        }

        private static void SeedRezervariIfFew(Matcherie m, IEnumerable<Rezervare> rezervari, int minCount)
        {
            if (m.Rezervari.Count >= minCount) return;

            foreach (var r in rezervari)
            {
                bool exists = m.Rezervari.Any(x =>
                    string.Equals(x.Tip ?? "", r.Tip ?? "", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.ClientID ?? "", r.ClientID ?? "", StringComparison.OrdinalIgnoreCase));

                if (!exists) m.Rezervari.Add(r);

                if (m.Rezervari.Count >= minCount) break;
            }
        }

        private static void SeedTranzactiiIfEmpty(ContClient c, Matcherie m, decimal[] sume)
        {
            if (c.Istoric.Count > 0) return;

            for (int i = 0; i < sume.Length; i++)
            {
                var dt = DateTime.Now.AddDays(-i).AddMinutes(-10 * i);
                c.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), dt, sume[i], m));
            }
        }

        private static Matcha Clone(Matcha p)
            => new Matcha(p.Nume, p.Descriere, p.Pret, p.Cantitate, p.Calorii);
    }
}
