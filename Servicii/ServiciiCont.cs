using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace ConsoleApp5
{
    public static class ServiciiCont
    {
        public static void CreeazaAdminNou(SistemMatcha sistem)
        {
            sistem.Administratori ??= new List<ContAdmin>();

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[green]Create Admin Account[/]").RuleStyle("grey"));

            string nume = AnsiConsole.Ask<string>("Admin name:");
            string idNou = AnsiConsole.Ask<string>("New Admin ID:");

            if (sistem.Administratori.Any(a => string.Equals(a.AdminId, idNou, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]An admin with this ID already exists.[/]");
                UIComun.Pauza();
                return;
            }

            string parola = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

            sistem.Administratori.Add(new ContAdmin(nume, idNou, parola));
            UIComun.SalvareSistem(sistem);

            AnsiConsole.MarkupLine("[green]Admin created and saved![/]");
            UIComun.Pauza();
        }

        public static void CreeazaContClient(SistemMatcha sistem)
        {
            sistem.Clienti ??= new List<ContClient>();

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[green]Create Client Account[/]").RuleStyle("grey"));

            string nume = AnsiConsole.Ask<string>("Full name:");
            string email = AnsiConsole.Ask<string>("Email:");

            if (sistem.Clienti.Any(c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]A client with this email already exists.[/]");
                UIComun.Pauza();
                return;
            }

            sistem.Clienti.Add(new ContClient(nume, email, new List<Tranzactie>(), new List<Rezervare>()));
            UIComun.SalvareSistem(sistem);

            AnsiConsole.MarkupLine("[green]Client account created and saved![/]");
            UIComun.Pauza();
        }

        public static ContClient? AlegeClientDinSistem(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No clients found. Create an account first.[/]");
                UIComun.Pauza();
                return null;
            }

            var map = new Dictionary<string, ContClient>();
            foreach (var c in sistem.Clienti)
            {
                string keyBase = $"{c.Nume} ({c.Email})";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key))
                {
                    key = $"{keyBase} #{k}";
                    k++;
                }
                map[key] = c;
            }

            AnsiConsole.Clear();
            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select client[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            return map[ales];
        }

        public static ContClient ReparaClientDacaENecesat(SistemMatcha sistem, ContClient client)
        {
            bool needsFix = (client.Istoric == null) || (client.Rezervari == null);
            if (!needsFix) return client;

            client.Istoric ??= new List<Tranzactie>();
            client.Rezervari ??= new List<Rezervare>();
            return client;
        }
    }
}
