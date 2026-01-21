using System;
using System.Collections.Generic;

namespace ConsoleApp5
{
    public static class ServiciiStatistica
    {
        public static List<(string nume, int val)> GetTopMatcheriiByRezervari(SistemMatcha sistem, int max)
        {
            var list = new List<(string nume, int val)>();
            if (sistem.Magazine == null) return list;

            foreach (var m in sistem.Magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                list.Add((m.Nume, rez));
            }

            list.Sort((a, b) => b.val.CompareTo(a.val));

            if (list.Count > max) list = list.GetRange(0, max);
            return list;
        }

        public static int GetTranzactiiInZi(SistemMatcha sistem, DateTime zi)
        {
            int count = 0;
            if (sistem.Clienti == null) return 0;

            foreach (var c in sistem.Clienti)
            {
                if (c.Istoric == null) continue;
                foreach (var t in c.Istoric)
                    if (t.Data.Date == zi.Date) count++;
            }

            return count;
        }
    }
}